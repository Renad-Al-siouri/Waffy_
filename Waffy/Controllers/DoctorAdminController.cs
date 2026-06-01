using Microsoft.AspNetCore.Mvc;
using Waffy.Interfaces;
using Waffy.Models;
using Waffy.Models.ViewModels;

namespace Waffy.Controllers
{
    public class DoctorAdminController : Controller
    {
        private readonly IDoctorRepository _doctorRepo;
        private readonly IWebHostEnvironment _env;

        public DoctorAdminController(IDoctorRepository doctorRepo,
                                     IWebHostEnvironment env)
        {
            _doctorRepo = doctorRepo;
            _env = env;
        }

        // ══ AUTH ══════════════════════════════════════════
        private IActionResult? RequireDoctorAuth()
        {
            var roleId = HttpContext.Session.GetInt32("RoleId");
            if (roleId == null || roleId.Value != 4)
                return RedirectToAction("Login", "Account");
            return null;
        }

        private int GetUserId() => HttpContext.Session.GetInt32("UserId") ?? 0;
        private int GetDoctorId() => HttpContext.Session.GetInt32("DoctorId") ?? 0;

        private async Task<string> SaveFileAsync(IFormFile file, string folder)
        {
            var path = Path.Combine(_env.WebRootPath, "uploads", folder);
            Directory.CreateDirectory(path);
            var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            var filePath = Path.Combine(path, fileName);
            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);
            return $"/uploads/{folder}/{fileName}";
        }

        // ══ DASHBOARD ═════════════════════════════════════
        public async Task<IActionResult> Dashboard()
        {
            var auth = RequireDoctorAuth();
            if (auth != null) return auth;

            var info = await _doctorRepo.GetFullInfoByUserIdAsync(GetUserId());
            if (info == null) return RedirectToAction("Login", "Account");

            var vm = new DoctorDashboardViewModel
            {
                FullName = info.FullName,
                Specialization = info.Specialization,
                ClinicName = info.ClinicName,
                Rating = info.Rating,
                VerificationStatus = info.VerificationStatus,
                IsAvailable = info.IsAvailable
            };

            return View(vm);
        }

        // ══ PERSONAL INFORMATION ══════════════════════════
        [HttpGet]
        public async Task<IActionResult> Personal_Information()
        {
            var auth = RequireDoctorAuth();
            if (auth != null) return auth;

            var info = await _doctorRepo.GetFullInfoByUserIdAsync(GetUserId());
            if (info == null) return RedirectToAction("Login", "Account");

            return View(new DoctorPersonalInfoViewModel
            {
                FullName = info.FullName,
                Phone = info.Phone,
                Email = info.Email,
                Bio = info.Bio,
                Specialization = info.Specialization,
                ProfileImageUrl = info.ProfileImageUrl,
                VerificationStatus = info.VerificationStatus,
                Rating = info.Rating,
                ClinicName = info.ClinicName
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Personal_Information(DoctorPersonalInfoViewModel vm)
        {
            var auth = RequireDoctorAuth();
            if (auth != null) return auth;

            if (string.IsNullOrWhiteSpace(vm.FullName))
            {
                TempData["SaveError"] = "Full name is required.";
                return RedirectToAction("Personal_Information");
            }

            // ✅ رفع الصورة + حفظ الـ URL
            string? profileImageUrl = null;
            if (vm.ProfileImage != null && vm.ProfileImage.Length > 0)
            {
                if (vm.ProfileImage.Length > 3 * 1024 * 1024)
                {
                    TempData["SaveError"] = "Image too large. Max 3MB.";
                    return RedirectToAction("Personal_Information");
                }
                profileImageUrl = await SaveFileAsync(vm.ProfileImage, "doctor-profiles");
            }

            // ✅ تمرير profileImageUrl
            await _doctorRepo.UpdatePersonalInfoAsync(
                GetUserId(),
                vm.FullName.Trim(),
                vm.Phone?.Trim(),
                vm.Bio?.Trim(),
                profileImageUrl);

            HttpContext.Session.SetString("UserName", vm.FullName.Trim());
            TempData["SaveSuccess"] = "Personal information saved successfully.";
            return RedirectToAction("Personal_Information");
        }

        // ══ PROFESSIONAL INFORMATION ══════════════════════
        [HttpGet]
        public async Task<IActionResult> Professional_Information()
        {
            var auth = RequireDoctorAuth();
            if (auth != null) return auth;

            var info = await _doctorRepo.GetFullInfoByUserIdAsync(GetUserId());
            if (info == null) return RedirectToAction("Login", "Account");

            // ✅ جلب ملفات الشهادات
            var certFiles = (await _doctorRepo.GetDocumentsByDoctorIdAsync(info.IdDoctor))
                .Where(d => d.DocumentType == "Certification")
                .ToList();

            return View(new DoctorProfessionalInfoViewModel
            {
                Specialization = info.Specialization,
                SecondarySpec = info.SecondarySpec,
                ExperienceYears = info.ExperienceYears,
                Qualification = info.Qualification,
                Certifications = info.Certifications,
                Languages = info.Languages,
                CertificationFiles = certFiles,           
                FullName = info.FullName,
                VerificationStatus = info.VerificationStatus,
                Rating = info.Rating,
                ClinicName = info.ClinicName
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadCertification(IFormFile certFile)
        {
            var auth = RequireDoctorAuth();
            if (auth != null) return auth;

            if (certFile == null || certFile.Length == 0)
            {
                TempData["CertError"] = "Please select a file.";
                return RedirectToAction("Professional_Information");
            }

            if (certFile.Length > 5 * 1024 * 1024)
            {
                TempData["CertError"] = "File too large. Max 5MB.";
                return RedirectToAction("Professional_Information");
            }

            var fileUrl = await SaveFileAsync(certFile, "doctor-certs");
            var idDoctor = GetDoctorId();

            await _doctorRepo.AddDocumentAsync(new DoctorDocument
            {
                IdDoctor = idDoctor,
                DocumentType = "Certification",
                FileUrl = fileUrl,
                Status = "Pending",
                UploadedAt = DateTime.UtcNow
            });

            TempData["CertSuccess"] = "Certificate uploaded successfully.";
            return RedirectToAction("Professional_Information");
        }

        // ══ WORK SCHEDULE ═════════════════════════════════
        [HttpGet]
        public async Task<IActionResult> Work_Schedule()
        {
            var auth = RequireDoctorAuth();
            if (auth != null) return auth;

            var info = await _doctorRepo.GetFullInfoByUserIdAsync(GetUserId());
            if (info == null) return RedirectToAction("Login", "Account");

            return View(BuildWorkScheduleVm(info));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Work_Schedule(DoctorWorkScheduleViewModel vm)
        {
            var auth = RequireDoctorAuth();
            if (auth != null) return auth;

            var workDays = ConvertWorkDaysToString(vm);
            var open = TimeSpan.TryParse(vm.StartTime, out var st) ? st : new TimeSpan(9, 0, 0);
            var close = TimeSpan.TryParse(vm.EndTime, out var et) ? et : new TimeSpan(17, 0, 0);

            await _doctorRepo.UpdateWorkScheduleAsync(GetUserId(), new Doctor
            {
                WorkingDays = workDays,
                StartTime = open,
                EndTime = close,
                AppointmentDuration = vm.AppointmentDuration,
                MaxDailyAppointments = vm.MaxDailyAppointments,
                IsAvailable = vm.IsAvailable
            });

            TempData["SaveSuccess"] = "Work schedule saved successfully.";
            return RedirectToAction("Work_Schedule");
        }

        // ══ DOCUMENTS ════════════════════════════════════
        [HttpGet]
        public async Task<IActionResult> Documents_D()
        {
            var auth = RequireDoctorAuth();
            if (auth != null) return auth;

            var info = await _doctorRepo.GetFullInfoByUserIdAsync(GetUserId());
            if (info == null) return RedirectToAction("Login", "Account");

            var docs = (await _doctorRepo.GetDocumentsByDoctorIdAsync(info.IdDoctor)).ToList();

            return View(new DoctorDocumentsViewModel
            {
                Documents = docs,
                CertificationUrl = info.Certifications,
                FullName = info.FullName,
                Specialization = info.Specialization,
                VerificationStatus = info.VerificationStatus,
                Rating = info.Rating,
                ClinicName = info.ClinicName,
                IdDoctor = info.IdDoctor
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadDocument(IFormFile file, string documentType)
        {
            var auth = RequireDoctorAuth();
            if (auth != null) return auth;

            if (file == null || file.Length == 0)
            {
                TempData["DocError"] = "Please select a file.";
                return RedirectToAction("Documents_D");
            }

            if (file.Length > 10 * 1024 * 1024)
            {
                TempData["DocError"] = "File too large. Max 10MB.";
                return RedirectToAction("Documents_D");
            }

            var fileUrl = await SaveFileAsync(file, "doctor-docs");
            var idDoctor = GetDoctorId();

            await _doctorRepo.AddDocumentAsync(new DoctorDocument
            {
                IdDoctor = idDoctor,
                DocumentType = documentType,
                FileUrl = fileUrl,
                Status = "Pending",
                UploadedAt = DateTime.UtcNow
            });

            TempData["DocSuccess"] = "Document uploaded successfully.";
            return RedirectToAction("Documents_D");
        }

        // ══ PASSWORD ══════════════════════════════════════
        [HttpGet]
        public IActionResult Password()
        {
            var auth = RequireDoctorAuth();
            if (auth != null) return auth;
            return View(new DoctorPasswordViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Password(DoctorPasswordViewModel vm)
        {
            var auth = RequireDoctorAuth();
            if (auth != null) return auth;

            if (vm.NewPassword.Length < 8)
            {
                TempData["PwdError"] = "New password must be at least 8 characters.";
                return View(vm);
            }

            if (vm.NewPassword != vm.ConfirmNewPassword)
            {
                TempData["PwdError"] = "Passwords do not match.";
                return View(vm);
            }

            var changed = await _doctorRepo.ChangePasswordAsync(
                GetUserId(), vm.CurrentPassword, vm.NewPassword);

            if (!changed)
            {
                TempData["PwdError"] = "Current password is incorrect.";
                return View(vm);
            }

            TempData["PwdSuccess"] = "Password changed successfully.";
            return RedirectToAction("Password");
        }

        // ══ OVERVIEW PAGES ════════════════════════════════
        public IActionResult Appointments()
        {
            var auth = RequireDoctorAuth();
            if (auth != null) return auth;
            return View();
        }

        public IActionResult Patients()
        {
            var auth = RequireDoctorAuth();
            if (auth != null) return auth;
            return View();
        }

        public IActionResult Medical_Records()
        {
            var auth = RequireDoctorAuth();
            if (auth != null) return auth;
            return View();
        }

        public IActionResult Notifications()
        {
            var auth = RequireDoctorAuth();
            if (auth != null) return auth;
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }

        // ══ HELPERS ═══════════════════════════════════════
        private static DoctorWorkScheduleViewModel BuildWorkScheduleVm(DoctorFullDto info)
        {
            var days = new HashSet<int>();
            if (!string.IsNullOrEmpty(info.WorkingDays))
                foreach (var d in info.WorkingDays.Split(','))
                    if (int.TryParse(d.Trim(), out var n)) days.Add(n);

            return new DoctorWorkScheduleViewModel
            {
                Sunday = days.Contains(0),
                Monday = days.Contains(1),
                Tuesday = days.Contains(2),
                Wednesday = days.Contains(3),
                Thursday = days.Contains(4),
                Friday = days.Contains(5),
                Saturday = days.Contains(6),
                StartTime = info.StartTime?.ToString(@"hh\:mm") ?? "09:00",
                EndTime = info.EndTime?.ToString(@"hh\:mm") ?? "17:00",
                AppointmentDuration = info.AppointmentDuration ?? 30,
                MaxDailyAppointments = info.MaxDailyAppointments ?? 12,
                IsAvailable = info.IsAvailable ?? false,
                FullName = info.FullName,
                VerificationStatus = info.VerificationStatus,
                Rating = info.Rating,
                ClinicName = info.ClinicName
            };
        }

        private static string ConvertWorkDaysToString(DoctorWorkScheduleViewModel vm)
        {
            var days = new List<int>();
            if (vm.Sunday) days.Add(0);
            if (vm.Monday) days.Add(1);
            if (vm.Tuesday) days.Add(2);
            if (vm.Wednesday) days.Add(3);
            if (vm.Thursday) days.Add(4);
            if (vm.Friday) days.Add(5);
            if (vm.Saturday) days.Add(6);
            return string.Join(",", days);
        }
    }
}