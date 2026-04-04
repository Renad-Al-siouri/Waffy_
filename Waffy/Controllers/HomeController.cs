using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Waffy.Models;

namespace Waffy.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
        public IActionResult Home_Before()
        {
            return View();
        }

        public IActionResult Home_After()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }
        public IActionResult Faq()
        {
            return View();
        }

        public IActionResult Travel()
        {
            return View();
        }

        public IActionResult Suport()
        {
            return View();
        }
        public IActionResult Notifications()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
