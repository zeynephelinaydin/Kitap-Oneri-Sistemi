using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace KitapOneri.Models
{
    public class UserBook
    {
        [ForeignKey("User")]
        public int UserId { get; set; }

        [ForeignKey("Book")]
        public string Isbn { get; set; }

        public bool Wishlist { get; set; } = false;
        public bool ReadBook { get; set; } = false;
        public DateTime AddedDate { get; set; } = DateTime.Now;
        public DateTime? FinishedDate { get; set; }

        public virtual User User { get; set; }
        public virtual Book Book { get; set; }
    }
}