using System.Linq;
using KitapOneri.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Cryptography;
using System.Text;

namespace KitapOneri.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Kullanıcı adı ve şifre boş olamaz.";
                return View();
            }

            // Şifreyi hashle (veritabanındakiyle karşılaştırmak için)
            string hashedPassword = HashPassword(password);

            // Kullanıcıyı veritabanında ara
            var user = _context.Users.FirstOrDefault(u => u.Username == username && u.PasswordHash == hashedPassword);

            if (user == null)
            {
                ViewBag.Error = "Kullanıcı adı veya şifre hatalı.";
                return View();
            }

            // Oturum açıldı, kullanıcı bilgilerini sakla
            HttpContext.Session.SetString("Username", user.Username);

            return RedirectToAction("Index", "Home");
        }

        // Şifreyi SHA256 ile hashleme metodu
        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }

                return builder.ToString();
            }
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); // Oturumu temizle
            return RedirectToAction("Login");
        }

    }
}