using Microsoft.AspNetCore.Mvc;

namespace Waffy.Controllers
{
    public class ProductController : Controller
    {
        public IActionResult Products()
        {
            return View();
        }
        public IActionResult Stores()
        {
            return View();
        }
    }
}
