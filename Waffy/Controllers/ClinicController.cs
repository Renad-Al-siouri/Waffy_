using Microsoft.AspNetCore.Mvc;

namespace Waffy.Controllers
{
    public class ClinicController : Controller
    {
        public IActionResult clinics()
        {
            return View();
        }
    }
}
