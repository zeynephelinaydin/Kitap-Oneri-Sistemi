using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic; // Koleksiyonları kullanabilmek için gerekli

namespace KitapOneri.Models
{
    [Table("users")] // 👈 Bu satırı ekle

    public class User
    {
        [Key]
        [Column("user_id")] // UserId'yi birincil anahtar olarak belirliyoruz
        public int UserId { get; set; }

        // Kullanıcı ile ilişkili kitapları tutan koleksiyon
        //public ICollection<UserBook> UserBooks { get; set; } // UserBook koleksiyonu eklendi

        public ICollection<UserBook> UserBooks { get; set; }

        [Required] // Kullanıcı adı zorunlu
        [StringLength(50)] 
        [Column("username")] // Kullanıcı adı uzunluğunu sınırlıyoruz
        public string Username { get; set; }

        [Column("email")] 
        public string Email { get; set; }

        [Required] // Şifre hash'inin zorunlu olması
        [StringLength(255)] 
        [Column("password_hash")]
        public string PasswordHash { get; set; }

        [Column("registration_date")]
        public DateTime RegistrationDate { get; set; }

        // Güvenlik sorusu ve cevabı
        [Column("security_question")]
        public string SecurityQuestion { get; set; }

        [Column("security_answer_hash")]
        public string SecurityAnswerHash { get; set; }
    }
}