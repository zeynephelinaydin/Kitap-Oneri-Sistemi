using KitapOneri.Models;
using KitapOneri.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.ComponentModel.DataAnnotations;
using System.Linq;  // `Any` kullanabilmek için gerekli namespace

namespace KitapOneri.Services
{
    public class AuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // 📌 Geçerli e-posta formatı kontrolü
        private bool IsValidEmailFormat(string email)
        {
            var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            return emailRegex.IsMatch(email);
        }

        // 📌 Sadece belirli e-posta uzantılarını kabul et
        private bool IsValidEmailDomain(string email)
        {
            var allowedDomains = new[] { "@gmail.com", "@hotmail.com" };
            return allowedDomains.Any(domain => email.EndsWith(domain, StringComparison.OrdinalIgnoreCase));
        }

        // 📌 Kullanıcı Kayıt İşlemi
        public async Task<string> RegisterUser(string username, string email, string password)
        {
            if (string.IsNullOrEmpty(email) || !new EmailAddressAttribute().IsValid(email))
                return "Geçersiz e-posta formatı";  // Geçersiz e-posta formatı

            if (!IsValidEmailDomain(email))
                return "Geçersiz e-posta uzantısı";  // Geçersiz e-posta uzantısı

            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email || u.Username == username);
            if (existingUser != null)
                return "Kullanıcı zaten mevcut!";  // Kullanıcı zaten mevcut!

            var newUser = new User
            {
                Username = username,
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),  // Şifreyi hash'le
                RegistrationDate = DateTime.UtcNow  // Kayıt tarihi
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return "Kayıt başarılı";  // Kayıt başarılı
        }

        // 📌 Kullanıcı Giriş İşlemi
        public async Task<bool> LoginUser(string email, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                return false;  // Kullanıcı bulunamadı

            return BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);  // Şifreyi hash ile karşılaştır
        }

        // 📌 Kullanıcıyı E-posta ile al
        public async Task<User> GetUserByEmail(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        // 📌 JWT Token Üretme
        public string GenerateJwtToken(string email)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
                return null;

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, email),
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]));  // Secret key configden alınıyor
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(1),  // Token geçerlilik süresi
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);  // Token'ı döndür
        }
    }
}
