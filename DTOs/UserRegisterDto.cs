using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KitapOneri.DTOs
{
    public class UserRegisterDto
    {
        [Required(ErrorMessage = "Kullanıcı adı zorunludur.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "E-posta zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        [Column("email")] // Veritabanındaki sütun adıyla eşleştirme
        public string Email { get; set; }

        [Required(ErrorMessage = "Şifre zorunludur.")]
        [MinLength(6, ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
        [Column("password_hash")] // Veritabanındaki sütun adıyla eşleştirme
        public string Password { get; set; }

        [Required(ErrorMessage = "Şifre tekrarı zorunludur.")]
        [Compare("Password", ErrorMessage = "Şifreler eşleşmiyor.")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Güvenlik sorusu zorunludur.")]
        public string SecurityQuestion { get; set; }

        [Required(ErrorMessage = "Güvenlik cevabı zorunludur.")]
        public string SecurityAnswer { get; set; }
    }
}