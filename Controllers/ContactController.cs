using Microsoft.AspNetCore.Mvc;

namespace KitapOneri.Controllers
{
    public class ContactController : Controller
    {
        public IActionResult Index()
        {
            return RedirectToAction("MyFavorites", "Books");
        }
    }
}