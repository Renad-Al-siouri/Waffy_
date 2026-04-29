using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Waffy.Interfaces;
using Waffy.Models;

namespace Waffy.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IComplaintRepository _complaintRepo;
        private readonly IClinicRepository _clinicRepo;
        private readonly IStoreRepository _storeRepo;

        public HomeController(
            ILogger<HomeController> logger,
            IComplaintRepository complaintRepo,
            IClinicRepository clinicRepo,
            IStoreRepository storeRepo)
        {
            _logger = logger;
            _complaintRepo = complaintRepo;
            _clinicRepo = clinicRepo;
            _storeRepo = storeRepo;
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

        public async Task<IActionResult> Suport()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            var clinics = (await _clinicRepo.GetAllAsync())
                .Where(c => c.VerificationStatus == "Approved").ToList();
            var stores = (await _storeRepo.GetAllAsync())
                .Where(s => s.VerificationStatus == "Approved").ToList();

            ViewBag.Clinics = clinics;
            ViewBag.Stores = stores;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitSupport(
            string targetType,
            int? targetId,
            string title,
            string description,
            string priority)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            if (string.IsNullOrWhiteSpace(title) ||
                string.IsNullOrWhiteSpace(description))
            {
                TempData["SupportError"] = "Please fill in all required fields.";
                return RedirectToAction("Suport");
            }

            await _complaintRepo.CreateAsync(new Complaint
            {
                IdUser = userId,
                TargetType = targetType,
                TargetId = targetType == "System" ? null : targetId,
                Title = title,
                Description = description,
                Status = "Open",
                Priority = priority ?? "Medium",
                CreatedAt = DateTime.UtcNow
            });

            TempData["SupportSuccess"] = "✅ Your report was received successfully!";
            return RedirectToAction("Suport");
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
