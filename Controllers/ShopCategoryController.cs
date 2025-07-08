using KitapOneri.Data;
using KitapOneri.Models;
using KitapOneri.ViewModels;
using KitapOneri.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace KitapOneri.Controllers
{
    [Route("category")]
    public class ShopCategoryController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly RecommendationService _recommendationService;
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public ShopCategoryController(ApplicationDbContext context, RecommendationService recommendationService, IConfiguration configuration)
        {
            _context = context;
            _recommendationService = recommendationService;
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection");
        }

        // Ana kategori sayfasını render eder.
        [HttpGet("")]
        public IActionResult Index(string type = null, string publisher = null, int page = 1, int pageSize = 50)
        {
            try
            {
                // Tüm kitaplar için null yap
                if (string.IsNullOrEmpty(type) || type == "Tüm Kitaplar")
                {
                    type = null;
                }

                // Yayınevlerini getir, başına "Tüm Yayınevleri" ekle
                var publishers = GetPublishers();
                publishers.Insert(0, "Tüm Yayınevleri");

                ViewBag.Publishers = publishers;
                ViewBag.SelectedPublisher = publisher ?? "Tüm Yayınevleri";
                ViewBag.SelectedType = type;

                var booksQuery = _context.books.AsQueryable();

                if (!string.IsNullOrEmpty(type))
                {
                    booksQuery = booksQuery.Where(b => b.genre == type);
                }

                if (!string.IsNullOrEmpty(publisher) && publisher != "Tüm Yayınevleri")
                {
                    booksQuery = booksQuery.Where(b => b.publisher == publisher);
                }

                int totalBooks = booksQuery.Count();
                int totalPages = (int)Math.Ceiling((double)totalBooks / pageSize);
                int skip = (page - 1) * pageSize;

                var books = booksQuery
                    .OrderBy(b => b.name)
                    .Skip(skip)
                    .Take(pageSize)
                    .ToList();

                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;

                return View("~/Views/ShopCategory/Index.cshtml", books);
            }
            catch (Exception ex)
            {
                return View("Error", new ErrorViewModel { ErrorMessage = "Kategori sayfası yüklenirken hata oluştu: " + ex.Message });
            }
        }

        // Tür bazlı kitap listesini JSON olarak döner.
        [HttpGet("ListByType")]
        public IActionResult ListByType(string type, int page = 1, int pageSize = 50, string publisher = null)
        {
            try
            {
                if (!string.IsNullOrEmpty(type) && type == "Tüm Kitaplar")
                {
                    type = null;
                }

                var query = _context.books.AsQueryable();

                if (!string.IsNullOrEmpty(type))
                {
                    query = query.Where(b => b.genre == type);
                }

                if (!string.IsNullOrEmpty(publisher) && publisher != "Tüm Yayınevleri")
                {
                    query = query.Where(b => b.publisher == publisher);
                }

                int totalBooks = query.Count();
                int totalPages = (int)Math.Ceiling((double)totalBooks / pageSize);
                int skip = (page - 1) * pageSize;

                var books = query
                    .OrderBy(b => b.name)
                    .Skip(skip)
                    .Take(pageSize)
                    .Select(b => new
                    {
                        b.name,
                        b.author,
                        rating = b.rating == null ? "Yok" : b.rating.average_rating.ToString(),
                        b.book_img,
                        b.isbn
                    })
                    .ToList();

                return Json(new
                {
                    books,
                    currentPage = page,
                    totalPages,
                    totalBooks
                });
            }
            catch (Exception ex)
            {
                return Json(new { error = "Kitap listesi alınırken hata oluştu: " + ex.Message });
            }
        }

        // Kitap detayları ve öneriler
        [HttpGet("Details")]
        public async Task<IActionResult> Details(string isbn)
        {
            try
            {
                if (string.IsNullOrEmpty(isbn))
                    return NotFound();

                var book = await _context.books.FirstOrDefaultAsync(b => b.isbn == isbn);
                if (book == null)
                    return NotFound();

                // RecommendationService içinde async metot olarak GetHybridRecommendations var
                var recommendations = await _recommendationService.GetHybridRecommendations(
                    newNameRoot: book.name,
                    newExplanationRoot: book.new_explanation_root,
                    explanation: book.explanation,
                    nameRoot: book.new_name_root,
                    author: book.author,
                    genre: book.genre,
                    totalRecommendations: 8
                );

                var viewModel = new BookDetailsViewModel
                {
                    Book = book,
                    RecommendedBooks = recommendations
                };

                return View("~/Views/Books/Details.cshtml", viewModel);
            }
            catch (Exception ex)
            {
                return View("Error", new ErrorViewModel { ErrorMessage = "Kitap detayları yüklenirken hata oluştu: " + ex.Message });
            }
        }

        // Veritabanından yayınevlerini raw SQL ile getirir.
        private List<string> GetPublishersRawSql()
        {
            List<string> publishers = new List<string>();

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    string sql = "SELECT DISTINCT publisher FROM books WHERE publisher IS NOT NULL ORDER BY publisher ASC";

                    using (var command = new SqlCommand(sql, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            publishers.Add(reader.GetString(0));
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Hata durumunda boş liste dönebilir veya loglayabilirsin
            }

            return publishers;
        }

        // EF Core kullanarak yayınevlerini getirir.
        private List<string> GetPublishers()
        {
            try
            {
                return _context.books
                    .Where(b => !string.IsNullOrEmpty(b.publisher))
                    .Select(b => b.publisher)
                    .Distinct()
                    .OrderBy(p => p)
                    .ToList();
            }
            catch (Exception)
            {
                return new List<string>();
            }
        }
    }
}
