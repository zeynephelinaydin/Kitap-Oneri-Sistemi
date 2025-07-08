using Microsoft.AspNetCore.Mvc;

namespace KitapOneri.Controllers
{
    public class CategoryController : Controller
    {
        public IActionResult Index()
        {
            // Kategorileri almak ve view'da göstermek için service veya repository kullanabilirsiniz
            return View("~/Views/ShopCategory/Index.cshtml");
        }
    }
}