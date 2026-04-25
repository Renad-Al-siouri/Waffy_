using Microsoft.AspNetCore.Mvc;
using Waffy.Interfaces;
using Waffy.Models;

namespace Waffy.Controllers
{
    public class MapController : Controller
    {
        private readonly IDangerousAnimalReportRepository _dangerRepo;

        public MapController(IDangerousAnimalReportRepository dangerRepo)
        {
            _dangerRepo = dangerRepo;
        }

        public IActionResult Gps() => View();

        // ════════════════════════════════════════════════════
        // DANGEROUS — GET
        // ════════════════════════════════════════════════════
        public IActionResult Dangerous()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");
            return View();
        }

        // ════════════════════════════════════════════════════
        // SUBMIT DANGER — POST
        // ════════════════════════════════════════════════════
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitDanger(
            string animalType,
            string problemType,
            string severity,
            string? city,
            string? area,
            string? locationText,
            decimal? latitude,
            decimal? longitude,
            string? description)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            // ✅ التحقق من الحقول المطلوبة
            if (string.IsNullOrWhiteSpace(animalType) ||
                string.IsNullOrWhiteSpace(problemType) ||
                string.IsNullOrWhiteSpace(city) ||
                string.IsNullOrWhiteSpace(description))
            {
                TempData["DangerError"] = "Please fill in all required fields.";
                return RedirectToAction("Dangerous");
            }

            var reportCode = $"WAFFY-D-{Guid.NewGuid().ToString()[..6].ToUpper()}";

            await _dangerRepo.CreateAsync(new DangerousAnimalReport
            {
                IdUser = userId,
                AnimalType = animalType,
                ProblemType = problemType,
                Severity = severity ?? "Medium",
                Description = description,
                City = city,
                Area = area,
                LocationText = locationText,
                Latitude = latitude,
                Longitude = longitude,
                Status = "Pending",
                ReportCode = reportCode,
                CreatedAt = DateTime.UtcNow
            });

            TempData["DangerSuccess"] = $"🚨 Report submitted! Code: {reportCode}";
            return RedirectToAction("My_Danger_Reports", "Profile");
        }
    }
}