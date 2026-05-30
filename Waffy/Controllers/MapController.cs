using Microsoft.AspNetCore.Mvc;
using Waffy.Interfaces;
using Waffy.Models;

namespace Waffy.Controllers
{
    public class MapController : Controller
    {
        private readonly IDangerousAnimalReportRepository _dangerRepo;
        private readonly IStoreRepository _storeRepo;
        private readonly IClinicRepository _clinicRepo;
        private readonly IAdminMapMarkerRepository _markerRepo;

        public MapController(
            IDangerousAnimalReportRepository dangerRepo,
            IStoreRepository storeRepo,
            IClinicRepository clinicRepo,
            IAdminMapMarkerRepository markerRepo)
        {
            _dangerRepo = dangerRepo;
            _storeRepo = storeRepo;
            _clinicRepo = clinicRepo;
            _markerRepo = markerRepo;
        }

        private bool IsAdmin() =>
            HttpContext.Session.GetString("RoleName") == "Admin";

        // ════════════════════════════════════════════════════════
        // USER GPS PAGE
        // ════════════════════════════════════════════════════════
        public IActionResult Gps() => View();

        // ════════════════════════════════════════════════════════
        // MAP DATA API — shared by User + Admin maps
        // GET /Map/GetMapData
        // ════════════════════════════════════════════════════════
        [HttpGet]
        public async Task<IActionResult> GetMapData()
        {
            // ── Stores ──────────────────────────────────────────
            var stores = await _storeRepo.GetAllAsync();
            var storesData = stores
                .Where(s => s.VerificationStatus == "Approved"
                         && s.Latitude.HasValue && s.Longitude.HasValue)
                .Select(s => new
                {
                    id = s.IdStore,
                    name = s.Name ?? "—",
                    lat = (double)s.Latitude!.Value,
                    lng = (double)s.Longitude!.Value,
                    city = s.City ?? "",
                    area = s.Area ?? "",
                    logo = s.LogoUrl ?? "",
                    phone = s.Phone ?? "",
                    hours = (s.OpenTime.HasValue && s.CloseTime.HasValue)
    ? $"{s.OpenTime.Value:hh\\:mm} – {s.CloseTime.Value:hh\\:mm}" +
      (!string.IsNullOrEmpty(s.WorkingDays) ? $" · {s.WorkingDays}" : "")
    : s.WorkingDays
                });

            // ── Clinics ─────────────────────────────────────────
            var clinics = await _clinicRepo.GetAllAsync();
            var clinicsData = clinics
                .Where(c => c.VerificationStatus == "Approved"
                         && c.Latitude.HasValue && c.Longitude.HasValue)
                .Select(c => new
                {
                    id = c.IdClinic,
                    name = c.Name ?? "—",
                    lat = (double)c.Latitude!.Value,
                    lng = (double)c.Longitude!.Value,
                    city = c.City ?? "",
                    area = c.Area ?? "",
                    logo = c.LogoUrl ?? "",
                    phone = c.Phone?.Trim() ?? "",
                    hasEmergency = c.HasEmergency,
                    hours = (c.DefaultOpenTime.HasValue && c.DefaultCloseTime.HasValue)
    ? $"{c.DefaultOpenTime.Value:hh\\:mm} – {c.DefaultCloseTime.Value:hh\\:mm}"
    : null
                });

            // ── Admin Markers ────────────────────────────────────
            var markers = await _markerRepo.GetActiveAsync();
            var markersData = markers
                .Select(m => new
                {
                    id = m.Id,
                    markerType = m.MarkerType,
                    name = m.Label,
                    lat = (double)m.Latitude,
                    lng = (double)m.Longitude,
                    city = m.City ?? "",
                    area = m.Area ?? "",
                    phone = m.Phone ?? "",
                    hours = m.WorkingHours ?? "",
                    description = m.Description ?? "",
                    website = m.Website ?? "",
                    notes = m.Notes ?? "",
                    iconColor = m.IconColor ?? "",
                    logo = m.LogoUrl ?? "",
                    image = m.ImageUrl ?? ""   // ← main photo
                });

            // ── Danger Reports ───────────────────────────────────
            var dangerReports = await _dangerRepo.GetActiveForMapAsync();
            var dangerData = dangerReports
                .Select(d => new
                {
                    id = d.IdDangReport,
                    name = $"{d.AnimalType} — {d.ProblemType}",
                    lat = (double)d.Latitude!.Value,
                    lng = (double)d.Longitude!.Value,
                    city = d.City ?? "",
                    area = d.Area ?? "",
                    severity = d.Severity ?? "Medium",
                    status = d.Status ?? "Pending",
                    reportCode = d.ReportCode ?? "",
                    animalType = d.AnimalType ?? ""
                });

            return Json(new
            {
                stores = storesData,
                clinics = clinicsData,
                dangerReports = dangerData,
                adminMarkers = markersData
            });
        }

        // ════════════════════════════════════════════════════════
        // GET MARKER BY ID — للـ Edit form
        // GET /Map/GetMarkerById?id=5
        // ════════════════════════════════════════════════════════
        [HttpGet]
        public async Task<IActionResult> GetMarkerById(int id)
        {
            if (!IsAdmin()) return Json(new { success = false });

            var m = await _markerRepo.GetByIdAsync(id);
            if (m == null) return Json(new { success = false });

            return Json(new
            {
                success = true,
                id = m.Id,
                markerType = m.MarkerType,
                label = m.Label,
                lat = (double)m.Latitude,
                lng = (double)m.Longitude,
                city = m.City ?? "",
                area = m.Area ?? "",
                phone = m.Phone ?? "",
                workingHours = m.WorkingHours ?? "",
                description = m.Description ?? "",
                website = m.Website ?? "",
                notes = m.Notes ?? "",
                imageUrl = m.ImageUrl ?? "",
                logoUrl = m.LogoUrl ?? ""
            });
        }

        // ════════════════════════════════════════════════════════
        // ADD ADMIN MARKER — POST (with image upload)
        // POST /Map/AddAdminMarker
        // ════════════════════════════════════════════════════════
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAdminMarker(
            string markerType,
            string label,
            decimal latitude,
            decimal longitude,
            string? city,
            string? area,
            string? phone,
            string? workingHours,
            string? description,
            string? website,
            string? notes,
            IFormFile? image)
        {
            if (!IsAdmin())
                return Json(new { success = false, message = "Unauthorized" });

            if (string.IsNullOrWhiteSpace(label))
                return Json(new { success = false, message = "Name is required" });

            // ── Save image if uploaded ────────────────────────────
            string? imageUrl = null;
            if (image != null && image.Length > 0)
            {
                var folder = Path.Combine(
                    Directory.GetCurrentDirectory(), "wwwroot", "uploads", "map-markers");
                Directory.CreateDirectory(folder);
                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(image.FileName)}";
                var filePath = Path.Combine(folder, fileName);
                using var stream = new FileStream(filePath, FileMode.Create);
                await image.CopyToAsync(stream);
                imageUrl = $"/uploads/map-markers/{fileName}";
            }

            var id = await _markerRepo.CreateAsync(new AdminMapMarker
            {
                MarkerType = markerType,
                Label = label.Trim(),
                Latitude = latitude,
                Longitude = longitude,
                City = city?.Trim(),
                Area = area?.Trim(),
                Phone = phone?.Trim(),
                WorkingHours = workingHours?.Trim(),
                Description = description?.Trim(),
                Website = website?.Trim(),
                Notes = notes?.Trim(),
                ImageUrl = imageUrl,
                Status = "Active",
                CreatedByAdminId = HttpContext.Session.GetInt32("UserId"),
                CreatedAt = DateTime.UtcNow
            });

            return Json(new { success = true, id, imageUrl });
        }

        // ════════════════════════════════════════════════════════
        // EDIT ADMIN MARKER — POST
        // POST /Map/EditAdminMarker
        // ════════════════════════════════════════════════════════
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAdminMarker(
            int id,
            string label,
            decimal latitude,
            decimal longitude,
            string? city,
            string? area,
            string? phone,
            string? workingHours,
            string? description,
            string? website,
            string? notes,
            IFormFile? image)
        {
            if (!IsAdmin())
                return Json(new { success = false, message = "Unauthorized" });

            var existing = await _markerRepo.GetByIdAsync(id);
            if (existing == null)
                return Json(new { success = false, message = "Marker not found" });

            // ── Save new image if uploaded ─────────────────────────
            string? imageUrl = existing.ImageUrl;
            if (image != null && image.Length > 0)
            {
                var folder = Path.Combine(
                    Directory.GetCurrentDirectory(), "wwwroot", "uploads", "map-markers");
                Directory.CreateDirectory(folder);
                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(image.FileName)}";
                var filePath = Path.Combine(folder, fileName);
                using var stream = new FileStream(filePath, FileMode.Create);
                await image.CopyToAsync(stream);
                imageUrl = $"/uploads/map-markers/{fileName}";
            }

            existing.Label = label.Trim();
            existing.Latitude = latitude;
            existing.Longitude = longitude;
            existing.City = city?.Trim();
            existing.Area = area?.Trim();
            existing.Phone = phone?.Trim();
            existing.WorkingHours = workingHours?.Trim();
            existing.Description = description?.Trim();
            existing.Website = website?.Trim();
            existing.Notes = notes?.Trim();
            existing.ImageUrl = imageUrl;

            await _markerRepo.UpdateAsync(existing);

            return Json(new { success = true, imageUrl });
        }

        // ════════════════════════════════════════════════════════
        // DELETE ADMIN MARKER — POST
        // ════════════════════════════════════════════════════════
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAdminMarker(int id)
        {
            if (!IsAdmin())
                return Json(new { success = false, message = "Unauthorized" });

            await _markerRepo.DeleteAsync(id);
            return Json(new { success = true });
        }

        // ════════════════════════════════════════════════════════
        // TOGGLE MARKER STATUS — POST (Active/Hidden)
        // ════════════════════════════════════════════════════════
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleAdminMarker(int id, string status)
        {
            if (!IsAdmin())
                return Json(new { success = false, message = "Unauthorized" });

            await _markerRepo.UpdateStatusAsync(id, status);
            return Json(new { success = true });
        }

        // ════════════════════════════════════════════════════════
        // DANGEROUS REPORT PAGE — GET
        // ════════════════════════════════════════════════════════
        public IActionResult Dangerous()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "Account");
            return View();
        }

        // ════════════════════════════════════════════════════════
        // SUBMIT DANGER REPORT — POST
        // ════════════════════════════════════════════════════════
        [HttpPost, ValidateAntiForgeryToken]
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

            if (string.IsNullOrWhiteSpace(animalType) ||
                string.IsNullOrWhiteSpace(problemType) ||
                string.IsNullOrWhiteSpace(city) ||
                string.IsNullOrWhiteSpace(description))
            {
                TempData["DangerError"] = "Please fill in all required fields.";
                return RedirectToAction("Dangerous");
            }

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
                ReportCode = $"WAFFY-D-{Guid.NewGuid().ToString()[..6].ToUpper()}",
                CreatedAt = DateTime.UtcNow
            });

            TempData["DangerSuccess"] = "🚨 Report submitted successfully!";
            return RedirectToAction("My_Danger_Reports", "Profile");
        }
    }
}