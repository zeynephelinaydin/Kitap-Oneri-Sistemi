namespace KitapOneri.Models
{
    public class BookWithRatingViewModel
    {
        public string name { get; set; }          // Kitap adı (Book.name)
        public string author { get; set; }        // Yazar adı (Book.author)
        public double average_rating { get; set; } // Ortalama rating (Rating.average_rating) - NULL değilse double
        public string book_img { get; set; }      // Kitap resmi (Book.book_img)
        public string explanation { get; set; }   // Açıklama (Book.explanation)
        public string isbn { get; set; }          // ISBN (Book.isbn)
        public string BookImg { get; set; }       // Kitap görseli (book_img)
        public string genre { get; set; }
    }
}