using Microsoft.AspNetCore.Mvc;

namespace Waffy.Controllers
{
    public class PetController : Controller
    {
        public IActionResult Petrecord()
        {
            return View();
        }

        public IActionResult Adapt()
        {
            return View();
        }
        public IActionResult Lost()
        {
            return View();
        }


    }
}
