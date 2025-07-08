using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KitapOneri.Models
{
    [Table("ratings")] 
    public class Rating
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Bu satırı ekleyin
        public int id { get; set; }

        public string isbn { get; set; }          
        public double average_rating { get; set; } 
        public double one_star_count { get; set; }
        public double two_star_count { get; set; }
        public double three_star_count { get; set; }
        public double four_star_count { get; set; }
        public double five_star_count { get; set; }
      
        public int? user_id { get; set; }

        // Navigation property
        public virtual Book book { get; set; }
        public string name { get; set; }
    }
}