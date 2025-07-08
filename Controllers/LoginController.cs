using Microsoft.AspNetCore.Mvc;
using KitapOneri.Models;
using KitapOneri.Data;
using KitapOneri.DTOs;
using Microsoft.AspNetCore.Http;
using System.Text.RegularExpressions;
using System.Linq;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;

namespace KitapOneri.Controllers
{
    public class LoginController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LoginController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Login/Index
        public IActionResult Index(string returnUrl = null)
        {
            // If the user is already logged in, redirect them to the home page
            if (HttpContext.Session.GetString("UserEmail") != null)
            {
                return RedirectToAction("Index", "Books");
            }

            ViewBag.ReturnUrl = returnUrl;
            return View("Index");
        }

        // POST: Login/Index
        [HttpPost]
        public IActionResult Index(UserLoginDto userLoginDto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ErrorMessage = "Geçersiz giriş bilgileri!";
                return View("Index");
            }

            var user = _context.Users.FirstOrDefault(u => u.Email == userLoginDto.Email);

            // Kullanıcı bulundu ve şifre doğrulaması yapılıyor
            if (user != null && VerifyPassword(userLoginDto.Password, user.PasswordHash))
            {
                // Session bilgilerini saklamak yerine, Claims-based authentication kullanmak daha güvenlidir.
                HttpContext.Session.SetString("UserEmail", user.Email);
                HttpContext.Session.SetString("Username", user.Username);
                HttpContext.Session.SetInt32("UserId", user.UserId);

                // Redirect to the home page after login
                return RedirectToAction("Index", "Home");
            }

            ViewBag.ErrorMessage = "Kullanıcı adı veya şifre hatalı!";
            return View("Index");
        }

        // Şifre doğrulama için yardımcı metod
        private bool VerifyPassword(string enteredPassword, string storedHash)
        {
            // Bcrypt veya başka bir hash algoritması kullanarak doğrulama yapılabilir
            return enteredPassword == storedHash; // Bu basit karşılaştırma şifreleme eklenince değişecek
        }

        // GET: Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Login");
        }

        // GET: Güvenlik sorusunu getir
        [HttpGet]
        public IActionResult GetSecurityQuestion(string email)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
                return Json(new { success = false });

            return Json(new
            {
                success = true,
                question = user.SecurityQuestion
            });
        }

        // POST: Güvenlik sorusu cevabı kontrol
        [HttpPost]
        public IActionResult VerifySecurityAnswer([FromBody] SecurityVerificationDto model)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == model.Email);
            if (user == null)
                return Json(new { success = false });

            if (user.SecurityAnswerHash == model.SecurityAnswer)
                return Json(new { success = true });

            return Json(new { success = false });
        }

        // POST: Şifre sıfırlama
        [HttpPost]
        public IActionResult ResetPassword(string email, string newPassword, string confirmPassword)
        {
            if (newPassword != confirmPassword)
            {
                ViewBag.ErrorMessage = "Şifreler uyuşmuyor.";
                return View("Index");
            }

            var regex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$");
            if (!regex.IsMatch(newPassword))
            {
                ViewBag.ErrorMessage = "Şifre en az 8 karakter, 1 büyük harf ve 1 rakam içermelidir.";
                return View("Index");
            }

            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                ViewBag.ErrorMessage = "Bu e-posta adresiyle kayıtlı kullanıcı bulunamadı.";
                return View("Index");
            }

            // Şifreyi hashleme işlemi
            user.PasswordHash = HashPassword(newPassword);
            _context.SaveChanges();

            ViewBag.SuccessMessage = "Şifreniz başarıyla güncellendi.";
            return View("Index");
        }

        // Şifreyi hashlemek için yardımcı metod
        private string HashPassword(string password)
        {
            // Bcrypt veya benzeri bir şifreleme algoritması kullanabilirsiniz
            using (var hmac = new HMACSHA512())
            {
                var hashBytes = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashBytes);
            }
        }
    }
}
