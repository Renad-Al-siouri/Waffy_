using Microsoft.AspNetCore.Mvc;

namespace Waffy.Controllers
{
    public class PaymentController : Controller
    {
        public IActionResult Cart()
        {
            return View();
        }

        public IActionResult Check()
        {
            return View();
        }
        public IActionResult Donate()
        {
            return View();
        }

    }
}
