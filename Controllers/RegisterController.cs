using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using KitapOneri.Data;
using KitapOneri.DTOs;
using KitapOneri.Models;
using System.Linq;
using System;

public class RegisterController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly string[] allowedDomains = { "gmail.com", "hotmail.com", "outlook.com", "yahoo.com" };

    public RegisterController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Register
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Index(UserRegisterDto userDto)
    {
        if (ModelState.IsValid)
        {
            // E-posta formatı doğrulama
            var emailPattern = new Regex(@"^[a-zA-Z0-9._%+-]+@([a-zA-Z0-9.-]+\.[a-zA-Z]{2,})$");
            if (!emailPattern.IsMatch(userDto.Email))
            {
                ModelState.AddModelError("Email", "Geçerli bir e-posta adresi giriniz.");
                return View();
            }

            // Alan adı doğrulama
            var domain = userDto.Email.Split('@').Last();
            if (!allowedDomains.Contains(domain))
            {
                ModelState.AddModelError("Email", "Sadece @gmail.com, @hotmail.com, @outlook.com, @yahoo.com gibi adresler kabul edilir.");
                return View();
            }

            // Kullanıcı adı ve e-posta kontrolü
            var existingUserByUsername = _context.Users.FirstOrDefault(u => u.Username == userDto.Username);
            var existingUserByEmail = _context.Users.FirstOrDefault(u => u.Email == userDto.Email);

            if (existingUserByUsername != null)
            {
                ModelState.AddModelError("Username", "Bu kullanıcı adı zaten alınmış.");
                return View();
            }

            if (existingUserByEmail != null)
            {
                ModelState.AddModelError("Email", "Bu e-posta adresi zaten kayıtlı.");
                return View();
            }

            // Kullanıcıyı veritabanına ekle
            var user = new User
            {
                Username = userDto.Username,
                Email = userDto.Email,
                PasswordHash = userDto.Password, // Şifreyi hashlemeden kaydediyoruz
                SecurityQuestion = userDto.SecurityQuestion,
                SecurityAnswerHash = userDto.SecurityAnswer, // Burada cevabı da hashlemeden kaydediyoruz
                RegistrationDate = DateTime.UtcNow
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            // Başarı mesajı ekle
            TempData["SuccessMessage"] = "Başarıyla kaydoldunuz!";
            return View(userDto); // userDto'yu view'a gönder

            return View();
        }

        return View();
    }
}