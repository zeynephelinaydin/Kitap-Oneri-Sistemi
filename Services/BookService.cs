using KitapOneri.Data;
using KitapOneri.Models;
using Microsoft.EntityFrameworkCore;

namespace KitapOneri.Services
{
    public class BookService
    {
        private readonly ApplicationDbContext _context;

        public BookService(ApplicationDbContext context)
        {
            _context = context;
        }

        // En yüksek reytinge sahip 50 kitabı getiren metot
        public async Task<List<BookWithRatingViewModel>> GetTopRatedBooksAsync()
        {
            var books = await _context.books
                .Join(_context.ratings,
                    book => book.isbn,
                    rating => rating.isbn,
                    (book, rating) => new BookWithRatingViewModel
                    {
                        name = book.name,
                        author = book.author,
                        average_rating = rating.average_rating,
                        book_img = book.book_img,
                        explanation = book.explanation,
                        isbn = book.isbn
                    })
                .Where(b => !b.name.Contains("Androidler"))
                .OrderByDescending(b => b.average_rating)
                .Take(60)
                .ToListAsync();

            return books;
        }
    }
}

