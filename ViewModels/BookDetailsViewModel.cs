using KitapOneri.Models;  // Book sınıfını kullanabilmek için bu satırı eklemelisiniz.

namespace KitapOneri.ViewModels
{
    public class BookDetailsViewModel
    {
        public Book? Book { get; set; }
        public List<Book> RecommendedBooks { get; set; }
        public bool IsUserLoggedIn { get; set; }
        public bool IsInWishlist { get; set; } // EKLENDİ ✅

        public double average_rating { get; set; } // Ortalama rating (Rating.average_rating) - NULL değilse double

        
        
        
        public Rating rating { get; set; }
        public Author Author { get; set; } // ✅ Doğru türde tanım
        public string AuthorBiography => Author?.Biography ?? "";

    }
}

