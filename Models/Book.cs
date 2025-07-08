using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KitapOneri.Models
{
    public class Book
    {
        public string name { get; set; }          // Kitap ismi
        public string author { get; set; }        // Yazar ismi
        [Column("author_id")]
        public int? AuthorId { get; set; }  // int? nullable int


        public virtual Author Author { get; set; } // Navigation property
        public string book_img { get; set; }      // Kitap resmi URL'si
        public string explanation { get; set; }   // Kitap açıklaması

        public string publisher { get; set; }

        [Key]
        public string isbn { get; set; }          // ISBN numarası

        [Column("book_type")]
        public string genre { get; set; }         // Kitap türü

        
        // New properties added
        public string new_name_root { get; set; }     // Yeni Zemberek ile köklerine ayrılmış kitap ismi
        public string new_explanation_root { get; set; } // Yeni Zemberek ile köklerine ayrılmış açıklama

        // Navigation properties
        public ICollection<UserBook> UserBooks { get; set; }
        public virtual Rating rating { get; set; }
    }
}