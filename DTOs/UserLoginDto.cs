using System.ComponentModel.DataAnnotations;

namespace KitapOneri.DTOs
{
    public class UserLoginDto
    {
        // Parametresiz yapıcı ekledik
        public UserLoginDto() { }

        public UserLoginDto(string email, string password)
        {
            Email = email;
            Password = password;
        }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}