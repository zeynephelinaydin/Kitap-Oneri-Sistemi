using KitapOneri.Data;
using Microsoft.AspNetCore.Mvc;
using KitapOneri.Models;
using Microsoft.EntityFrameworkCore;
using KitapOneri.Services;
using KitapOneri.ViewModels;

public class BooksController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly RecommendationService _recommendationService;

    public BooksController(ApplicationDbContext context, RecommendationService recommendationService)
    {
        _context = context;
        _recommendationService = recommendationService;
    }

    public IActionResult Index()
    {
        if (HttpContext.Session.GetInt32("UserId") == null)
            return RedirectToAction("Index", "Login");

        var randomBook = _context.books.OrderBy(b => Guid.NewGuid()).FirstOrDefault();
        if (randomBook != null)
            return RedirectToAction("Details", new { isbn = randomBook.isbn });

        return RedirectToAction("Details", new { isbn = "default" });
    }

    public async Task<IActionResult> Details(string isbn)
    {
        var userId = HttpContext.Session.GetInt32("UserId");

        if (userId == null)
            return RedirectToAction("Index", "Login");

        if (string.IsNullOrEmpty(isbn))
        {
            TempData["ErrorMessage"] = "Geçersiz ISBN!";
            return RedirectToAction("Index", "Books");
        }

        // Kitabı, rating ve author ile birlikte al
        var book = await _context.books
            .Include(b => b.rating)
            .Include(b => b.Author)
            .FirstOrDefaultAsync(b => b.isbn == isbn);

        if (book == null)
            return NotFound();

        // Ayrı olarak rating verilerini al (puan dağılımı için)
        var rating = await _context.ratings.FirstOrDefaultAsync(r => r.isbn == isbn);

        // Kitap önerilerini al
        List<Book> recommendations = await _recommendationService.GetHybridRecommendations(
            book.name,
            book.new_explanation_root,
            book.explanation,
            book.new_name_root,
            book.author,
            book.genre,
            totalRecommendations: 20);

        // Önerilen kitapları rating'leriyle al
        var recommendedBookIsbns = recommendations.Select(r => r.isbn).ToList();
        var recommendedBooksWithRating = _context.books
            .Include(b => b.rating)
            .Where(b => recommendedBookIsbns.Contains(b.isbn))
            .ToList();

        // Favorilerde mi kontrol et
        bool isInWishlist = _context.user_books.Any(ub =>
            ub.UserId == userId &&
            ub.Isbn == isbn &&
            ub.Wishlist == true);

        // ViewModel'e tüm verileri ata
        var viewModel = new BookDetailsViewModel
        {
            Book = book,
            RecommendedBooks = recommendedBooksWithRating,
            IsUserLoggedIn = true,
            IsInWishlist = isInWishlist,
            average_rating = book.rating?.average_rating ?? 0,
            rating = rating,              // ⭐ Eksikti: rating puan dağılımı için
            Author = book.Author          // ⭐ Eksikti: yazar biyografisi için
        };

        return View(viewModel);
    }

    [HttpPost]
    public IActionResult AddToWishlist(string isbn)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return RedirectToAction("Index", "Login");

        if (string.IsNullOrEmpty(isbn))
        {
            TempData["ErrorMessage"] = "Geçersiz ISBN!";
            return RedirectToAction("Details", new { isbn = isbn });
        }

        var userBook = _context.user_books
            .FirstOrDefault(ub => ub.UserId == userId && ub.Isbn == isbn);

        if (userBook != null)
        {
            TempData["Message"] = "Bu kitap zaten favorilerinizde.";
            return RedirectToAction("MyFavorites");
        }

        var newUserBook = new UserBook
        {
            UserId = userId.Value,
            Isbn = isbn,
            Wishlist = true,
            AddedDate = DateTime.UtcNow
        };

        _context.user_books.Add(newUserBook);
        _context.SaveChanges();

        TempData["Message"] = "Kitap favorilere eklendi!";
        return RedirectToAction("MyFavorites");
    }

    [HttpGet]
    public IActionResult Search(string query, int page = 1)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            TempData["ErrorMessage"] = "Lütfen bir arama terimi girin.";
            return RedirectToAction("Index", "Home");
        }

        const int pageSize = 30;
        var loweredQuery = query.ToLower();

        var queryable = _context.books
            .Include(b => b.rating)
            .Where(b =>
                b.name.ToLower().Contains(loweredQuery) ||
                b.author.ToLower().Contains(loweredQuery) ||
                b.explanation.ToLower().Contains(loweredQuery) ||
                b.genre.ToLower().Contains(loweredQuery));

        var totalResults = queryable.Count();
        var totalPages = (int)Math.Ceiling((double)totalResults / pageSize);

        page = Math.Clamp(page, 1, totalPages);

        var results = queryable
            .OrderByDescending(b => b.name.ToLower().Contains(loweredQuery))
            .ThenByDescending(b => b.author.ToLower().Contains(loweredQuery))
            .ThenByDescending(b => b.explanation.ToLower().Contains(loweredQuery))
            .ThenByDescending(b => b.genre.ToLower().Contains(loweredQuery))
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        ViewData["SearchQuery"] = query;
        ViewData["CurrentPage"] = page;
        ViewData["TotalPages"] = totalPages;

        return View(results);
    }

    [HttpPost]
    public IActionResult RemoveFromWishlist(string isbn)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return RedirectToAction("Index", "Login");

        if (string.IsNullOrEmpty(isbn))
        {
            TempData["ErrorMessage"] = "Geçersiz ISBN!";
            return RedirectToAction("MyFavorites");
        }

        try
        {
            var userBook = _context.user_books
                .FirstOrDefault(ub => ub.UserId == userId && ub.Isbn == isbn && ub.Wishlist);

            if (userBook == null)
            {
                TempData["ErrorMessage"] = "Bu kitap favorilerinizde bulunamadı.";
                return RedirectToAction("MyFavorites");
            }

            if (userBook.ReadBook)
            {
                userBook.Wishlist = false;
                _context.user_books.Update(userBook);
            }
            else
            {
                _context.user_books.Remove(userBook);
            }

            _context.SaveChanges();
            TempData["Message"] = "Kitap favorilerden çıkarıldı.";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Hata: {ex.Message}");
            TempData["ErrorMessage"] = "Bir hata oluştu. Lütfen tekrar deneyin.";
        }

        return RedirectToAction("MyFavorites");
    }

    public IActionResult MyFavorites()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return RedirectToAction("Index", "Login");

        var favoriteBooks = _context.user_books
            .Where(ub => ub.UserId == userId && ub.Wishlist)
            .Include(ub => ub.Book)
            .ThenInclude(b => b.rating)
            .Select(ub => ub.Book)
            .ToList();

        return View(favoriteBooks);
    }

    [HttpGet]
    public async Task<IActionResult> UserRecommendations()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return Unauthorized();

        var userBooks = _context.user_books
            .Where(ub => ub.UserId == userId && (ub.Wishlist || ub.ReadBook))
            .Include(ub => ub.Book)
            .ThenInclude(b => b.rating)
            .Select(ub => ub.Book)
            .ToList();

        if (!userBooks.Any())
            return Ok(new List<BookWithRatingViewModel>());

        var userBookIsbns = userBooks.Select(b => b.isbn).ToHashSet();
        var weightedResults = new Dictionary<string, double>();

        foreach (var book in userBooks)
        {
            var rootExplanationMatches = await _recommendationService.GetSimilarBooksByRootMatch(book.new_explanation_root, book.new_name_root, 15);
            var vectorExplanationMatches = await _recommendationService.GetSimilarBooks(book.explanation, book.name, 15);
            var nameMatches = await _recommendationService.GetBooksByNameSimilarity(book.new_name_root, 10);
            var authorMatches = await _recommendationService.GetBooksByAuthor(book.author, book.name, 10);
            var genreMatches = await _recommendationService.GetBooksByGenre(book.genre, book.name, 10);

            AddWeightedRecommendations(weightedResults, rootExplanationMatches, userBookIsbns, 1.5);
            AddWeightedRecommendations(weightedResults, vectorExplanationMatches, userBookIsbns, 1.3);
            AddWeightedRecommendations(weightedResults, nameMatches, userBookIsbns, 2.0);
            AddWeightedRecommendations(weightedResults, authorMatches, userBookIsbns, 1.8);
            AddWeightedRecommendations(weightedResults, genreMatches, userBookIsbns, 1.2);
        }

        var finalBooks = GetTopRecommendations(weightedResults, 20);

        var viewModels = finalBooks.Select(b => new BookWithRatingViewModel
        {
            name = b.name,
            author = b.author,
            isbn = b.isbn,
            book_img = b.book_img,
            explanation = b.explanation,
            genre = b.genre,
            average_rating = b.rating?.average_rating ?? 0
        }).ToList();

        return Ok(viewModels);
    }

    private void AddWeightedRecommendations(Dictionary<string, double> weightedResults, IEnumerable<Book> matches, HashSet<string> userBookIsbns, double weight)
    {
        foreach (var b in matches)
        {
            if (!userBookIsbns.Contains(b.isbn))
            {
                double ratingBoost = ((b.rating?.average_rating ?? 0) / 5.0) * 4.0;
                weightedResults[b.isbn] = weightedResults.GetValueOrDefault(b.isbn, 0) + weight + ratingBoost;
            }
        }
    }

    private List<Book> GetTopRecommendations(Dictionary<string, double> weightedResults, int topN)
    {
        var allBooks = _context.books.Include(b => b.rating).ToList();

        var finalRecommendations = weightedResults
            .OrderByDescending(x => x.Value)
            .ThenBy(x => Guid.NewGuid())
            .Take(topN)
            .Select(x => allBooks.FirstOrDefault(b => b.isbn == x.Key))
            .Where(b => b != null)
            .ToList();

        return finalRecommendations;
    }
    
    
    
}