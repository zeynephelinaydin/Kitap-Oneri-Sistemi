using KitapOneri.Services;
using KitapOneri.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http; // Session için gerekli namespace

namespace KitapOneri.Controllers
{
    public class AuthController : Controller
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        // Kayıt sayfası (GET)
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // Kayıt işlemi (POST)
        [HttpPost]
        public async Task<IActionResult> Register(UserRegisterDto model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Kullanıcıyı kaydetme
            var result = await _authService.RegisterUser(model.Username, model.Email, model.Password);

            if (result == "Kayıt başarılı")
                return RedirectToAction("Login"); // Başarıyla kaydedildiyse giriş sayfasına yönlendir

            // Hata durumunda mesaj ekle
            ModelState.AddModelError("", result); // result string mesajını ekle
            return View(model); // Hata ile tekrar kayıt sayfasını döndür
        }

        // Giriş sayfası (GET)
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // Giriş işlemi (POST)
        [HttpPost]
        public async Task<IActionResult> Login(UserLoginDto model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Kullanıcı doğrulama işlemi
            var isLoggedIn = await _authService.LoginUser(model.Email, model.Password);

            if (!isLoggedIn)
            {
                ModelState.AddModelError("", "E-posta veya şifre yanlış!");
                return View(model); // Hata varsa giriş sayfasına geri dön
            }

            // Giriş başarılı, kullanıcıyı session'a kaydediyoruz
            var user = await _authService.GetUserByEmail(model.Email); // Kullanıcıyı e-posta ile alıyoruz
            if (user != null)
            {
                // Oturum açan kullanıcının bilgilerini session'a kaydediyoruz
                HttpContext.Session.SetInt32("UserId", user.UserId);  // Kullanıcı ID'si session'a kaydedildi
                HttpContext.Session.SetString("Username", user.Username);  // Kullanıcı adı session'a kaydedildi
            }

            // Giriş başarılı, token oluşturuluyor
            var token = _authService.GenerateJwtToken(model.Email); // JWT token oluştur

            if (token != null)
            {
                // Token'ı döndürüyoruz (giriş yapan kullanıcının token'ı)
                return RedirectToAction("Index", "Home"); // Başarılı girişte anasayfaya yönlendiriyoruz
            }

            return Unauthorized("Geçersiz kullanıcı"); // Geçersiz kullanıcı hatası
        }

        // Çıkış işlemi
        public IActionResult Logout()
        {
            // Çıkış işlemi, session'ı temizle
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home"); // Çıkış sonrası anasayfaya yönlendir
        }
    }
}
