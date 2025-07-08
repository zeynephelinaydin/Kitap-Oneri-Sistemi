using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KitapOneri.Models;
using System.Security.Claims;
using KitapOneri.Data;
using Microsoft.EntityFrameworkCore;

namespace KitapOneri.Controllers
{
    public class BlogController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BlogController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Blog Index Sayfası
        public IActionResult Index()
        {
            return View();
        }

        // Blog Detay Sayfası
        public async Task<IActionResult> Detail(int id)
        {
            // Blog ID'si geçerli değilse, 404 hatası döndür
            if (id < 1 || id > 5)
            {
                return NotFound(); // Sadece 1-5 arası izin veriyoruz.
            }

            string pageName = $"single-blog{id}";
            string viewName = pageName;

            // Yorumları yükle
            var comments = await _context.BlogComments
                .Where(c => c.PageName == pageName)
                .Include(c => c.User)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            // Yorumları ViewBag ile view'a geçiyoruz
            ViewBag.Comments = comments;
            ViewBag.BlogId = id;
            ViewBag.PageName = pageName;

            return View(viewName);
        }

        // Yorum Ekleme (POST)
        [HttpPost]
        public async Task<IActionResult> AddComment(string Subject, string Message, string PageName)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Index", "Login", new { returnUrl = Url.Action("Detail", "Blog", new { id = GetIdFromPageName(PageName) }) });
            }

            // Yorum mesajı boşsa, hata mesajı ile geri dön
            if (string.IsNullOrEmpty(Message))
            {
                ModelState.AddModelError("", "Yorum mesajı boş olamaz.");
                return RedirectToAction("Detail", "Blog", new { id = GetIdFromPageName(PageName) });
            }

            // Kullanıcı ID'sini session'dan alıyoruz
            var userId = HttpContext.Session.GetInt32("UserId").Value;

            // Yorum eklemek için yeni BlogComment nesnesi oluşturuluyor
            var newComment = new BlogComment
            {
                UserId = userId,
                Subject = Subject,
                Message = Message,
                PageName = PageName,
                CreatedAt = DateTime.UtcNow
            };

            try
            {
                // Yeni yorumu veritabanına ekle
                _context.BlogComments.Add(newComment);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Yorum kaydedilirken hata oluşursa kullanıcıya hata mesajı göster
                ViewBag.ErrorMessage = "Yorum kaydedilirken bir hata oluştu: " + ex.Message;
                return RedirectToAction("Detail", "Blog", new { id = GetIdFromPageName(PageName) });
            }

            // Yorum başarıyla kaydedildiyse, blog sayfasına geri dön
            return RedirectToAction("Detail", "Blog", new { id = GetIdFromPageName(PageName) });
        }

        // Yorum Silme (POST)
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteComment(int commentId, int blogId)
        {
            // Yorumun veritabanından bulunması
            var comment = await _context.BlogComments.FindAsync(commentId);
            if (comment == null)
            {
                return NotFound(); // Yorum bulunamazsa 404 döndür
            }

            // Kullanıcının kimliğini alıyoruz
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return RedirectToAction("Index", "Login"); // Eğer kullanıcı giriş yapmamışsa, login sayfasına yönlendir
            }

            var currentUserId = int.Parse(userIdClaim);

            // Yorumun sahibi ile giriş yapan kullanıcının kimliği aynı mı kontrol ediyoruz
            if (comment.UserId != currentUserId)
            {
                return Forbid(); // Eğer yorumun sahibi değilse, yetkisiz erişim
            }

            // Yorum silme işlemi
            try
            {
                _context.BlogComments.Remove(comment); // Yorum veritabanından siliniyor
                await _context.SaveChangesAsync(); // Değişiklikler kaydediliyor
            }
            catch (Exception ex)
            {
                // Hata yönetimi
                ViewBag.ErrorMessage = "Yorum silinirken bir hata oluştu: " + ex.Message;
                return RedirectToAction("Detail", new { id = blogId }); // Hata oluşursa, blog detay sayfasına dön
            }

            return RedirectToAction("Detail", new { id = blogId }); // Başarılı silme sonrası detay sayfasına yönlendir
        }

        // Sayfa ismine göre ID döndüren yardımcı metod
        private int GetIdFromPageName(string pageName)
        {
            if (string.IsNullOrEmpty(pageName))
                return 1;

            if (pageName.StartsWith("single-blog"))
            {
                string idPart = pageName.Replace("single-blog", "");
                if (int.TryParse(idPart, out int id))
                    return id;
            }

            return 1; // Default: 1
        }
    }
}
