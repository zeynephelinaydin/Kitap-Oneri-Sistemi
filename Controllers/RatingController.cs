using KitapOneri.Data;
using KitapOneri.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
[ApiController]
[Route("[controller]")]
public class RateController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public RateController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost("Rate")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Rate([FromBody] RatingRequest request)
    {
        try
        {
            if (request == null || string.IsNullOrEmpty(request.isbn) || request.rating < 1 || request.rating > 5)
                return BadRequest(new { success = false, message = "Geçersiz istek." });

            if (string.IsNullOrEmpty(request.email))
                return BadRequest(new { success = false, message = "Email adresi gerekli." });

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.email);
            if (user == null)
                return BadRequest(new { success = false, message = "Kullanıcı bulunamadı." });

            var book = await _context.books.FirstOrDefaultAsync(b => b.isbn == request.isbn);
            if (book == null)
                return BadRequest(new { success = false, message = "Kitap bulunamadı." });

            var rating = await _context.ratings.FirstOrDefaultAsync(r => r.isbn == request.isbn);
            if (rating == null)
            {
                rating = new Rating
                {
                    isbn = request.isbn,
                    name = book.name,
                    user_id = user.UserId,
                    one_star_count = 0,
                    two_star_count = 0,
                    three_star_count = 0,
                    four_star_count = 0,
                    five_star_count = 0,
                    average_rating = 0
                };
                _context.ratings.Add(rating);
            }

            switch (request.rating)
            {
                case 1: rating.one_star_count++; break;
                case 2: rating.two_star_count++; break;
                case 3: rating.three_star_count++; break;
                case 4: rating.four_star_count++; break;
                case 5: rating.five_star_count++; break;
                default: return BadRequest(new { success = false, message = "Geçersiz puan." });
            }

            double toplamOy = rating.one_star_count + rating.two_star_count + rating.three_star_count + rating.four_star_count + rating.five_star_count;

            if (toplamOy > 0)
            {
                double toplamPuan = (rating.one_star_count * 1) + (rating.two_star_count * 2) + (rating.three_star_count * 3) + (rating.four_star_count * 4) + (rating.five_star_count * 5);
                rating.average_rating = Math.Round(toplamPuan / toplamOy, 2);
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "Oyunuz başarıyla kaydedildi.",
                average_rating = rating.average_rating,  // burayı değiştir
                totalVotes = toplamOy
            });
        }
        catch (Exception ex)
        {
            // Sadece konsola yaz, kullanıcıya mesaj dönme
            Console.WriteLine($"Rating hatası: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");

            // Kullanıcıya hata mesajı dönme, sadece genel hata kodu dönebiliriz
            return StatusCode(500);
        }

    }

    [HttpGet("GetRatingsByISBN")]
    public async Task<IActionResult> GetRatingsByISBN(string isbn)
    {
        if (string.IsNullOrWhiteSpace(isbn))
            return BadRequest(new { success = false, message = "ISBN gerekli." });

        var rating = await _context.ratings
            .Where(r => r.isbn == isbn)
            .Select(r => new
            {
                r.one_star_count,
                r.two_star_count,
                r.three_star_count,
                r.four_star_count,
                r.five_star_count,
                r.average_rating
            })
            .FirstOrDefaultAsync();

        if (rating == null)
            return NotFound(new { success = false, message = "Kitap bulunamadı." });

        return Ok(new { success = true, rating });
    }
}
