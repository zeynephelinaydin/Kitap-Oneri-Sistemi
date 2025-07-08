using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KitapOneri.Models
{
    [Table("authors")] // Tablo adı veriliyor
    public class Author
    {
        [Key]
        [Column("author_id")]
        public int AuthorId { get; set; }

        [Required]
        [StringLength(100)]
        [Column("name")] // Yazar adı
        public string Name { get; set; }

        [Column("biography")] // Biyografi (zorunlu değil)
        public string Biography { get; set; }

        // Eğer yazar ile kitaplar arasında ilişki kuracaksan:
        // public ICollection<Book> Books { get; set; }
    }
}