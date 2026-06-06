using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.Arm;
using System.Xml.Linq;
using Waffy.Interfaces;
using Waffy.Models;
using Waffy.Models.ViewModels;
using static System.Collections.Specialized.BitVector32;
using static System.Net.Mime.MediaTypeNames;
namespace Waffy.Controllers
{
    public class ClinicController : Controller
    {
        private readonly IClinicRepository _clinicRepo;
        private readonly IClinicServiceRepository _serviceRepo;
        private readonly IClinicScheduleRepository _scheduleRepo;
        private readonly IClinicImageRepository _imageRepo;
        private readonly IDoctorRepository _doctorRepo;
        private readonly IReviewRepository _reviewRepo;

        public ClinicController(
            IClinicRepository clinicRepo,
            IClinicServiceRepository serviceRepo,
            IClinicScheduleRepository scheduleRepo,
            IClinicImageRepository imageRepo,
            IReviewRepository reviewRepo,
            IDoctorRepository doctorRepo)
        {
            _clinicRepo = clinicRepo;
            _serviceRepo = serviceRepo;
            _scheduleRepo = scheduleRepo;
            _imageRepo = imageRepo;
            _doctorRepo = doctorRepo;
            _reviewRepo = reviewRepo;
        }
        // ══════════════════════════════════════════════════
        // GET /Clinic/Clinics
        // ══════════════════════════════════════════════════
        [HttpGet]
        public async Task<IActionResult> Clinics(
            string? search, string? city, string? service)
        {
            var all = (await _clinicRepo.GetAllAsync())
                .Where(c => c.VerificationStatus == "Approved" ||
                            c.VerificationStatus == "Under Review")
                .ToList();

            var cards = new List<ClinicCardViewModel>();

            foreach (var clinic in all)
            {
                var services = (await _serviceRepo.GetByClinicIdAsync(clinic.IdClinic))
                    .Where(s => s.DocumentType != null)
                    .Select(s => s.DocumentType!)
                    .ToList();

                if (!string.IsNullOrEmpty(service) && service != "All")
                    if (!services.Contains(service)) continue;

                if (!string.IsNullOrEmpty(city) && city != "All")
                    if (clinic.City != city) continue;

                if (!string.IsNullOrEmpty(search))
                {
                    var q = search.ToLower();
                    if (!clinic.Name.ToLower().Contains(q) &&
                        !(clinic.City ?? "").ToLower().Contains(q) &&
                        !(clinic.Description ?? "").ToLower().Contains(q))
                        continue;
                }

                cards.Add(new ClinicCardViewModel
                {
                    IdClinic = clinic.IdClinic,
                    Name = clinic.Name,
                    ClinicType = clinic.ClinicType,
                    City = clinic.City,
                    Area = clinic.Area,
                    Phone = clinic.Phone,
                    LogoUrl = clinic.LogoUrl,
                    CoverImageUrl = clinic.CoverImageUrl,
                    HasEmergency = clinic.HasEmergency,
                    DefaultOpenTime = clinic.DefaultOpenTime,
                    DefaultCloseTime = clinic.DefaultCloseTime,
                    Services = services,
                    Description = clinic.Description,
                    Rating = clinic.Rating
                });
            }

            // ✅ من الـ all المجلوب مسبقاً
            var cities = all
                .Select(c => c.City)
                .Where(c => !string.IsNullOrEmpty(c))
                .Distinct()
                .OrderBy(c => c)
                .ToList();

            var vm = new ClinicsListViewModel
            {
                Clinics = cards,
                Cities = cities!,
                AllServices = new List<string>
        {
            "Vaccinations","General checkup","Surgery","Dentistry",
            "Lab Tests","X-Ray","Grooming","Home Visit","Emergency"
        },
                SelectedCity = city ?? "All",
                SearchQuery = search,
                SelectedService = service ?? "All"
            };

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> ClinicDetail(int id)
        {
            var clinic = await _clinicRepo.GetByIdAsync(id);

            // ✅ يسمح بـ Under Review أيضاً
            if (clinic == null ||
                (clinic.VerificationStatus != "Approved" &&
                 clinic.VerificationStatus != "Under Review"))
                return NotFound();

            var services = (await _serviceRepo.GetByClinicIdAsync(id))
                .Where(s => s.DocumentType != null)
                .Select(s => s.DocumentType!)
                .ToList();

            var schedule = (await _scheduleRepo.GetByClinicIdAsync(id))
                .OrderBy(s => s.DayOfWeek ?? 0)
                .ToList();

            var gallery = (await _imageRepo.GetByClinicIdAsync(id)).ToList();

            // ✅ فقط Approved للأطباء
            var doctors = (await _doctorRepo.GetByClinicIdAsync(id))
                .Where(d => d.VerificationStatus == "Approved")
                .ToList();
            var reviews = (await _reviewRepo.GetByTargetAsync("Clinic", id)).ToList();

            var currentUserId = HttpContext.Session.GetInt32("UserId");
            var hasReviewed = currentUserId.HasValue
                && await _reviewRepo.HasReviewedAsync(currentUserId.Value, "Clinic", id);
            var vm = new ClinicDetailViewModel
            {
                IdClinic = clinic.IdClinic,
                Name = clinic.Name,
                ClinicType = clinic.ClinicType,
                City = clinic.City,
                Area = clinic.Area,
                Phone = clinic.Phone,
                Email = clinic.Email,
                Description = clinic.Description,
                Address = clinic.Address,
                Latitude = clinic.Latitude,
                Longitude = clinic.Longitude,
                HasEmergency = clinic.HasEmergency,
                DefaultOpenTime = clinic.DefaultOpenTime,
                DefaultCloseTime = clinic.DefaultCloseTime,
                LogoUrl = clinic.LogoUrl,
                CoverImageUrl = clinic.CoverImageUrl,
                Rating = clinic.Rating,
                Services = services,
                Schedule = schedule,
                GalleryImages = gallery,
                Reviews = reviews,
                CurrentUserHasReviewed= hasReviewed,
                ReviewCount = reviews.Count,
                Doctors = doctors
            };

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> DoctorDetail(int id)
        {
            var doctor = await _doctorRepo.GetDetailByIdAsync(id);
            if (doctor == null) return NotFound();

            var certDocs = (await _doctorRepo.GetDocumentsByDoctorIdAsync(id))
                .Where(d => d.DocumentType == "Certification")
                .ToList();
            doctor.CertificationDocs = certDocs;

            var drReviews = (await _reviewRepo.GetByTargetAsync("Doctor", id)).ToList();

            var currentUserId = HttpContext.Session.GetInt32("UserId"); // ← أضف هذا
            var drHasReview = currentUserId.HasValue
                && await _reviewRepo.HasReviewedAsync(currentUserId.Value, "Doctor", id);

            // ← هذه الثلاثة يجب أن تكون قبل return
            doctor.Reviews = drReviews;
            doctor.CurrentUserHasReviewed = drHasReview;
            doctor.ReviewCount = drReviews.Count;

            return View(doctor); // ← الأخير
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitReview(
    string targetType, int targetId, int rating,
    string? title, string? comment, string returnUrl = "")
        {
            // يجب تسجيل الدخول
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            // تحقق من الـ rating
            if (rating < 1 || rating > 5)
            {
                TempData["ReviewError"] = "Please select a rating between 1 and 5 stars.";
                return targetType == "Clinic"
                    ? RedirectToAction("ClinicDetail", new { id = targetId })
                    : RedirectToAction("DoctorDetail", new { id = targetId });
            }

            // تحقق من التكرار
            var already = await _reviewRepo.HasReviewedAsync(userId.Value, targetType, targetId);
            if (already)
            {
                TempData["ReviewError"] = "You have already reviewed this " +
                                          (targetType == "Clinic" ? "clinic." : "doctor.");
                return targetType == "Clinic"
                    ? RedirectToAction("ClinicDetail", new { id = targetId })
                    : RedirectToAction("DoctorDetail", new { id = targetId });
            }

            await _reviewRepo.AddAsync(new Waffy.Models.Review
            {
                IdUser = userId.Value,
                TargetType = targetType,
                TargetId = targetId,
                Rating = rating,
                Title = title?.Trim(),
                Comment = comment?.Trim(),
                Status = "Active",
                CreatedAt = DateTime.UtcNow
            });

            TempData["ReviewSuccess"] = "⭐ Thank you! Your review has been submitted.";

            return targetType == "Clinic"
                ? RedirectToAction("ClinicDetail", new { id = targetId })
                : RedirectToAction("DoctorDetail", new { id = targetId });
        }

    }
}



















