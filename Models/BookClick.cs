// Veritabanı için yeni model
namespace KitapOneri.Models
{
    public class BookClick
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string BookIsbn { get; set; }
        public DateTime ClickTime { get; set; }
        
        // İlişkiler
        public Book Book { get; set; }
    }
}