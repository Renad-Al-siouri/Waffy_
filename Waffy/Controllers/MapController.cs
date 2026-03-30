using Microsoft.AspNetCore.Mvc;

namespace Waffy.Controllers
{
    public class MapController : Controller
    {
        public IActionResult Gps()
        {
            return View();
        }
       
            public IActionResult Dangerous()
        {
            return View();
        }
    }
}
