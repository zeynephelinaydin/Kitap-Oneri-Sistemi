using Microsoft.AspNetCore.Mvc;
using KitapOneri.Models;
using KitapOneri.Data;
using System.Linq;
using System;

namespace KitapOneri.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // En yüksek reytinge sahip 60 kitabı listeleyen metod
        public IActionResult Index()
        {
            try
            {
                var booksWithRatings = (from book in _context.books
                    join rating in _context.ratings on book.isbn equals rating.isbn
                    orderby rating.average_rating descending
                    select new BookWithRatingViewModel
                    {
                        name = book.name,                   // Kitap adı
                        author = book.author,               // Yazar adı
                        average_rating = rating.average_rating, // Ortalama puan
                        book_img = book.book_img,           // Kitap görseli
                        explanation = book.explanation,     // Açıklama
                        isbn = book.isbn                    // ISBN numarası
                    }).Take(60).ToList();

                return View(booksWithRatings);
            }
            catch (Exception ex)
            {
                // Hata durumunda hata mesajını döndürüyoruz
                return Content("Hata: " + ex.Message);
            }
        }
    }
}