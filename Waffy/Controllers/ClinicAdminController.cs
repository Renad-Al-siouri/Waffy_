using Microsoft.AspNetCore.Mvc;
using Waffy.Interfaces;
using Waffy.Models;
using Waffy.Models.ViewModels;

namespace Waffy.Controllers
{
    public class ClinicAdminController : Controller
    {
        private readonly IClinicRepository _clinicRepo;
        private readonly ILocationRepository _locationRepo;
        private readonly IClinicScheduleRepository _scheduleRepo;
        private readonly IClinicServiceRepository _serviceRepo;
        private readonly IClinicDocumentRepository _docRepo;
        private readonly IWebHostEnvironment _env;
        private readonly IClinicImageRepository _imageRepo;
        private readonly IDoctorRepository _doctorRepo;

        public ClinicAdminController(IClinicRepository clinicRepo,
        ILocationRepository locationRepo,
        IClinicScheduleRepository scheduleRepo,
        IClinicServiceRepository serviceRepo,
        IClinicDocumentRepository docRepo,      
        IWebHostEnvironment env,
        IClinicImageRepository imageRepo,
        IDoctorRepository doctorRepo)
        {
            _clinicRepo = clinicRepo;
            _locationRepo = locationRepo;
            _scheduleRepo = scheduleRepo;
            _serviceRepo = serviceRepo;
            _docRepo = docRepo;   
            _env = env;
            _imageRepo = imageRepo;
            _doctorRepo = doctorRepo;
        }

        // ══════════════════════════════════════════════════════════
        // AUTH HELPERS
        // ══════════════════════════════════════════════════════════

        /// <summary>يجلب ClinicId من الـ Session</summary>
        private int? GetCurrentClinicId()
            => HttpContext.Session.GetInt32("ClinicId");

        /// <summary>
        /// يتحقق أن المستخدم مسجل دخوله كـ Clinic(3) أو Doctor(4)
        /// null = مصرح | IActionResult = redirect للـ Login
        /// </summary>
        private IActionResult? RequireClinicAuth()
        {
            var roleId = HttpContext.Session.GetInt32("RoleId");
            if (roleId == null || roleId.Value != 3)
                return RedirectToAction("Login", "Account");
            return null;
        }
        private async Task<string> SaveFileAsync(IFormFile file, string folder)
        {
            var uploadsPath = Path.Combine(_env.WebRootPath, "uploads", folder);
            Directory.CreateDirectory(uploadsPath);
            var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            var filePath = Path.Combine(uploadsPath, fileName);
            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);
            return $"/uploads/{folder}/{fileName}";
        }


        // ══════════════════════════════════════════════════════════
        // LOGIN — redirect للـ Account/Login المركزي
        // ══════════════════════════════════════════════════════════


        [HttpGet]
        public IActionResult Login()
        {
            if (HttpContext.Session.GetInt32("RoleId") == 3)
                return RedirectToAction("Dashboard");
            return RedirectToAction("Login", "Account");
        }
        // ══════════════════════════════════════════════════════════
        // LOGOUT
        // ══════════════════════════════════════════════════════════

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }

        // ══════════════════════════════════════════════════════════
        // PAGES — كلها محمية
        // ══════════════════════════════════════════════════════════

        public IActionResult Index() => View();

        public IActionResult Dashboard()
        {
            var auth = RequireClinicAuth();
            if (auth != null) return auth;
            return View();
        }

        public IActionResult Appointments()
        {
            var auth = RequireClinicAuth();
            if (auth != null) return auth;
            return View();
        }

        public IActionResult Patients()
        {
            var auth = RequireClinicAuth();
            if (auth != null) return auth;
            return View();
        }

        public IActionResult Medical_Records()
        {
            var auth = RequireClinicAuth();
            if (auth != null) return auth;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Doctors()
        {
            var auth = RequireClinicAuth();
            if (auth != null) return auth;

            var clinicId = GetCurrentClinicId();
            if (clinicId == null) return RedirectToAction("Login", "Account");

            var doctors = (await _doctorRepo.GetByClinicIdAsync(clinicId.Value)).ToList();
            return View(new DoctorsViewModel { Doctors = doctors });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddDoctor(AddDoctorViewModel vm)
        {
            var auth = RequireClinicAuth();
            if (auth != null) return auth;

            var clinicId = GetCurrentClinicId();
            if (clinicId == null) return RedirectToAction("Login", "Account");

            if (string.IsNullOrWhiteSpace(vm.FullName) ||
                string.IsNullOrWhiteSpace(vm.Email) ||
                string.IsNullOrWhiteSpace(vm.Phone) ||
                string.IsNullOrWhiteSpace(vm.Password))
            {
                TempData["DoctorError"] = "Please fill all required fields.";
                return RedirectToAction("Doctors");
            }

            // ✅ تحقق من تكرار الإيميل
            if (await _doctorRepo.EmailExistsAsync(vm.Email.Trim()))
            {
                TempData["DoctorError"] = "This email is already registered.";
                return RedirectToAction("Doctors");
            }

            // ✅ إنشاء User
            var userId = await _doctorRepo.CreateUserAsync(
                vm.FullName.Trim(), vm.Email.Trim(),
                vm.Phone.Trim(), vm.Password);

            // ✅ إنشاء Doctor
            var doctorId = await _doctorRepo.GetNextDoctorIdAsync();
            await _doctorRepo.AddDoctorAsync(new Doctor
            {
                IdDoctor = doctorId,
                IdUser = userId,
                IdClinic = clinicId.Value,
                Specialization = vm.Specialization,
                ExperienceYears = vm.ExperienceYears,
                Certifications = vm.Certifications,
                IsAvailable = false,
                VerificationStatus = "Pending",
                CreatedAt = DateTime.UtcNow
            });

            TempData["DoctorSuccess"] =
                $"Dr. {vm.FullName} added successfully. Awaiting SuperAdmin approval.";
            return RedirectToAction("Doctors");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveDoctor(int idDoctor)
        {
            var auth = RequireClinicAuth();
            if (auth != null) return auth;

            await _doctorRepo.RemoveFromClinicAsync(idDoctor);
            TempData["DoctorSuccess"] = "Doctor removed from clinic.";
            return RedirectToAction("Doctors");
        }

        [HttpGet]
        public async Task<IActionResult> Basic_Information()
        {
            var auth = RequireClinicAuth();
            if (auth != null) return auth;

            var clinicId = GetCurrentClinicId();
            if (clinicId == null) return RedirectToAction("Login", "Account");

            var clinic = await _clinicRepo.GetByIdAsync(clinicId.Value);
            if (clinic == null) return RedirectToAction("Login", "Account");

            // ✅ جلب districts بناءً على المدينة الحالية
            var governorates = (await _locationRepo.GetAllGovernoratesAsync()).ToList();
            var allDistricts = (await _locationRepo.GetAllDistrictsAsync()).ToList();

            var currentGov = governorates.FirstOrDefault(g =>
                g.NameEn.Equals(clinic.City, StringComparison.OrdinalIgnoreCase));
            ViewBag.AllGovsJson = System.Text.Json.JsonSerializer.Serialize(
                governorates.Select(g => new { id = g.Id, name = g.NameEn }));
            ViewBag.AllDistsJson = System.Text.Json.JsonSerializer.Serialize(
                allDistricts.Select(d => new { govId = d.GovernorateId, name = d.NameEn }));

            ViewBag.AreaOptions = currentGov != null
                ? allDistricts
                    .Where(d => d.GovernorateId == currentGov.Id)
                    .OrderBy(d => d.NameEn)
                    .Select(d => d.NameEn)
                    .ToList()
                : new List<string>();

            var vm = new ClinicBasicInfoViewModel
            {
                Name = clinic.Name,
                ClinicType = clinic.ClinicType,
                City = clinic.City,
                Area = clinic.Area,
                Phone = clinic.Phone?.Trim() ?? "",
                Email = clinic.Email,
                Description = clinic.Description,
                Address = clinic.Address,
                Latitude = clinic.Latitude,
                Longitude = clinic.Longitude
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Basic_Information(ClinicBasicInfoViewModel vm)
        {
            var auth = RequireClinicAuth();
            if (auth != null) return auth;

            var clinicId = GetCurrentClinicId();
            if (clinicId == null) return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
            {
                TempData["SaveError"] = "Please fill all required fields.";
                return View(vm);
            }

            var clinic = await _clinicRepo.GetByIdAsync(clinicId.Value);
            if (clinic == null) return RedirectToAction("Login", "Account");

            // ✅ احفظي القيم الأصلية قبل التعديل
            var origLat = clinic.Latitude;
            var origLng = clinic.Longitude;

            clinic.Name = vm.Name.Trim();
            clinic.ClinicType = vm.ClinicType ?? "Veterinary";
            clinic.City = vm.City;
            clinic.Area = vm.Area ?? "";
            clinic.Phone = vm.Phone?.Trim() ?? "";
            clinic.Email = vm.Email ?? "";
            clinic.Description = vm.Description;
            clinic.Address = vm.Address ?? "";
            clinic.Latitude = origLat;  
            clinic.Longitude = origLng;  

            await _clinicRepo.UpdateAsync(clinic);

            // ✅ تحديث اسم العيادة في الـ Session
            HttpContext.Session.SetString("ClinicName", clinic.Name);

            TempData["SaveSuccess"] = "Basic information saved successfully.";
            return RedirectToAction("Basic_Information");
        }
        [HttpGet]
        public async Task<IActionResult> Working_Hours()
        {
            var auth = RequireClinicAuth();
            if (auth != null) return auth;

            var clinicId = GetCurrentClinicId();
            if (clinicId == null) return RedirectToAction("Login", "Account");

            var clinic = await _clinicRepo.GetByIdAsync(clinicId.Value);
            if (clinic == null) return RedirectToAction("Login", "Account");

            var schedules = (await _scheduleRepo.GetByClinicIdAsync(clinicId.Value)).ToList();

            bool GetDay(int dow) =>
                schedules.Any(s => s.DayOfWeek == dow && s.IsWorkingDay == true);

            var firstWorking = schedules.FirstOrDefault(s => s.IsWorkingDay == true);

            var vm = new WorkingHoursViewModel
            {
                Sunday = GetDay(0),
                Monday = GetDay(1),
                Tuesday = GetDay(2),
                Wednesday = GetDay(3),
                Thursday = GetDay(4),
                Friday = GetDay(5),
                Saturday = GetDay(6),
                OpenTime = firstWorking?.OpenTime.HasValue == true
                    ? firstWorking.OpenTime!.Value.ToString(@"hh\:mm")
                    : clinic.DefaultOpenTime.HasValue
                        ? clinic.DefaultOpenTime.Value.ToString(@"hh\:mm")
                        : "09:00",
                CloseTime = firstWorking?.CloseTime.HasValue == true
                    ? firstWorking.CloseTime!.Value.ToString(@"hh\:mm")
                    : clinic.DefaultCloseTime.HasValue
                        ? clinic.DefaultCloseTime.Value.ToString(@"hh\:mm")
                        : "18:00",
                HasEmergency = clinic.HasEmergency
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Working_Hours(WorkingHoursViewModel vm)
        {
            var auth = RequireClinicAuth();
            if (auth != null) return auth;

            var clinicId = GetCurrentClinicId();
            if (clinicId == null) return RedirectToAction("Login", "Account");

            // ✅ Parse times
            var openTs = TimeSpan.TryParse(vm.OpenTime, out var ot)
                ? ot : new TimeSpan(9, 0, 0);
            var closeTs = TimeSpan.TryParse(vm.CloseTime, out var ct)
                ? ct : new TimeSpan(18, 0, 0);

            // ✅ حذف القديم + إدراج الجديد (MAX+1)
            var nextId = await _scheduleRepo.GetNextIdAsync();
            await _scheduleRepo.DeleteByClinicIdAsync(clinicId.Value);

            var days = new[]
            {
        (0, vm.Sunday),
        (1, vm.Monday),
        (2, vm.Tuesday),
        (3, vm.Wednesday),
        (4, vm.Thursday),
        (5, vm.Friday),
        (6, vm.Saturday)
    };

            foreach (var (dow, isWorking) in days)
            {
                await _scheduleRepo.InsertAsync(new ClinicSchedule
                {
                    Id = nextId++,
                    IdClinic = clinicId.Value,
                    DayOfWeek = dow,
                    OpenTime = isWorking ? openTs : null,
                    CloseTime = isWorking ? closeTs : null,
                    IsWorkingDay = isWorking
                });
            }

            // ✅ تحديث الـ Clinic defaults + HasEmergency
            var clinic = await _clinicRepo.GetByIdAsync(clinicId.Value);
            if (clinic != null)
            {
                var origLat = clinic.Latitude;
                var origLng = clinic.Longitude;
                clinic.DefaultOpenTime = openTs;
                clinic.DefaultCloseTime = closeTs;
                clinic.HasEmergency = vm.HasEmergency;
                clinic.Latitude = origLat;
                clinic.Longitude = origLng;
                await _clinicRepo.UpdateAsync(clinic);
            }

            TempData["SaveSuccess"] = "Working hours saved successfully.";
            return RedirectToAction("Working_Hours");
        }

        [HttpGet]
        public async Task<IActionResult> Services()
        {
            var auth = RequireClinicAuth();
            if (auth != null) return auth;

            var clinicId = GetCurrentClinicId();
            if (clinicId == null) return RedirectToAction("Login", "Account");

            var existing = await _serviceRepo.GetByClinicIdAsync(clinicId.Value);

            var vm = new ServicesViewModel
            {
                SelectedServices = existing
                    .Where(s => s.DocumentType != null)
                    .Select(s => s.DocumentType!)
                    .ToList()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Services(ServicesViewModel vm)
        {
            var auth = RequireClinicAuth();
            if (auth != null) return auth;

            var clinicId = GetCurrentClinicId();
            if (clinicId == null) return RedirectToAction("Login", "Account");

            var nextId = await _serviceRepo.GetNextIdAsync();
            await _serviceRepo.DeleteByClinicIdAsync(clinicId.Value);

            foreach (var serviceName in vm.SelectedServices ?? new List<string>())
            {
                await _serviceRepo.InsertAsync(new ClinicService
                {
                    Id = nextId++,
                    IdClinic = clinicId.Value,
                    DocumentType = serviceName,
                    Status = "Active",
                    UploadedAt = DateTime.UtcNow
                });
            }

            TempData["SaveSuccess"] = "Services updated successfully.";
            return RedirectToAction("Services");
        }

        [HttpGet]
        public async Task<IActionResult> Documents_C()
        {
            var auth = RequireClinicAuth();
            if (auth != null) return auth;

            var clinicId = GetCurrentClinicId();
            if (clinicId == null) return RedirectToAction("Login", "Account");

            var clinic = await _clinicRepo.GetByIdAsync(clinicId.Value);
            if (clinic == null) return RedirectToAction("Login", "Account");

            var docs = (await _docRepo.GetByClinicIdAsync(clinicId.Value)).ToList();

            var vm = new DocumentsCViewModel
            {
                Documents = docs,
                ClinicVerificationStatus = clinic.VerificationStatus
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadDocument(IFormFile file, string documentType)
        {
            var auth = RequireClinicAuth();
            if (auth != null) return auth;

            var clinicId = GetCurrentClinicId();
            if (clinicId == null) return RedirectToAction("Login", "Account");

            // ✅ Validations
            if (file == null || file.Length == 0)
            {
                TempData["DocError"] = "Please select a file.";
                return RedirectToAction("Documents_C");
            }

            if (file.Length > 10 * 1024 * 1024)
            {
                TempData["DocError"] = "File too large. Max 10MB.";
                return RedirectToAction("Documents_C");
            }

            if (string.IsNullOrWhiteSpace(documentType))
            {
                TempData["DocError"] = "Please select a document type.";
                return RedirectToAction("Documents_C");
            }

            // ✅ حفظ الملف
            var fileUrl = await SaveFileAsync(file, "clinic-docs");

            // ✅ إضافة السجل بـ Status = Pending
            await _docRepo.AddAsync(new ClinicDocument
            {
                IdClinic = clinicId.Value,
                DocumentType = documentType,
                FileUrl = fileUrl,
                Status = "Pending",
                UploadedAt = DateTime.UtcNow
            });

            // ✅ تغيير VerificationStatus إلى "Under Review" فقط لو كانت Approved
            var clinic = await _clinicRepo.GetByIdAsync(clinicId.Value);
            if (clinic != null && clinic.VerificationStatus == "Approved")
            {
                var origLat = clinic.Latitude;
                var origLng = clinic.Longitude;
                clinic.VerificationStatus = "Under Review";
                clinic.Latitude = origLat;
                clinic.Longitude = origLng;
                await _clinicRepo.UpdateAsync(clinic);
            }

            TempData["DocSuccess"] = "Document uploaded successfully. Awaiting SuperAdmin review.";
            return RedirectToAction("Documents_C");
        }

        [HttpGet]
        public async Task<IActionResult> Images()
        {
            var auth = RequireClinicAuth();
            if (auth != null) return auth;

            var clinicId = GetCurrentClinicId();
            if (clinicId == null) return RedirectToAction("Login", "Account");

            var clinic = await _clinicRepo.GetByIdAsync(clinicId.Value);
            if (clinic == null) return RedirectToAction("Login", "Account");

            // ✅ Gallery من الجدول الجديد
            var gallery = (await _imageRepo.GetByClinicIdAsync(clinicId.Value)).ToList();

            ViewBag.LogoUrl = clinic.LogoUrl;
            ViewBag.CoverImageUrl = clinic.CoverImageUrl;
            ViewBag.GalleryImages = gallery;

            return View();
        }

        // ── Upload Logo ──────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadLogo(IFormFile file)
        {
            var auth = RequireClinicAuth();
            if (auth != null) return auth;

            var clinicId = GetCurrentClinicId();
            if (clinicId == null) return RedirectToAction("Login", "Account");

            if (file == null || file.Length == 0)
            { TempData["ImgError"] = "Please select a file."; return RedirectToAction("Images"); }

            if (file.Length > 2 * 1024 * 1024)
            { TempData["ImgError"] = "Logo too large. Max 2MB."; return RedirectToAction("Images"); }

            var fileUrl = await SaveFileAsync(file, "clinic-logos");

            var clinic = await _clinicRepo.GetByIdAsync(clinicId.Value);
            if (clinic != null)
            {
                var origLat = clinic.Latitude;
                var origLng = clinic.Longitude;
                clinic.LogoUrl = fileUrl;
                clinic.Latitude = origLat;
                clinic.Longitude = origLng;
                await _clinicRepo.UpdateAsync(clinic);
            }

            TempData["ImgSuccess"] = "Logo updated successfully.";
            return RedirectToAction("Images");
        }

        // ── Upload Cover ─────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadCover(IFormFile file)
        {
            var auth = RequireClinicAuth();
            if (auth != null) return auth;

            var clinicId = GetCurrentClinicId();
            if (clinicId == null) return RedirectToAction("Login", "Account");

            if (file == null || file.Length == 0)
            { TempData["ImgError"] = "Please select a file."; return RedirectToAction("Images"); }

            if (file.Length > 5 * 1024 * 1024)
            { TempData["ImgError"] = "Cover too large. Max 5MB."; return RedirectToAction("Images"); }

            var fileUrl = await SaveFileAsync(file, "clinic-covers");

            var clinic = await _clinicRepo.GetByIdAsync(clinicId.Value);
            if (clinic != null)
            {
                var origLat = clinic.Latitude;
                var origLng = clinic.Longitude;
                clinic.CoverImageUrl = fileUrl;
                clinic.Latitude = origLat;
                clinic.Longitude = origLng;
                await _clinicRepo.UpdateAsync(clinic);
            }

            TempData["ImgSuccess"] = "Cover image updated successfully.";
            return RedirectToAction("Images");
        }

        // ── Upload Gallery Image ──────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadGalleryImage(IFormFile file)
        {
            var auth = RequireClinicAuth();
            if (auth != null) return auth;

            var clinicId = GetCurrentClinicId();
            if (clinicId == null) return RedirectToAction("Login", "Account");

            if (file == null || file.Length == 0)
            { TempData["ImgError"] = "Please select a file."; return RedirectToAction("Images"); }

            if (file.Length > 2 * 1024 * 1024)
            { TempData["ImgError"] = "Image too large. Max 2MB."; return RedirectToAction("Images"); }

            // ✅ تحقق من الحد الأقصى
            var count = await _imageRepo.CountByClinicIdAsync(clinicId.Value);
            if (count >= 8)
            { TempData["ImgError"] = "Maximum 8 gallery images allowed."; return RedirectToAction("Images"); }

            var fileUrl = await SaveFileAsync(file, "clinic-gallery");

            await _imageRepo.AddAsync(new ClinicImage
            {
                IdClinic = clinicId.Value,
                ImageUrl = fileUrl,
                SortOrder = count,
                UploadedAt = DateTime.UtcNow
            });

            TempData["ImgSuccess"] = "Image added to gallery.";
            return RedirectToAction("Images");
        }

        // ── Delete Gallery Image ──────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteGalleryImage(int id)
        {
            var auth = RequireClinicAuth();
            if (auth != null) return auth;

            await _imageRepo.DeleteAsync(id);

            TempData["ImgSuccess"] = "Image removed from gallery.";
            return RedirectToAction("Images");
        }

        public IActionResult Revenue()
        {
            var auth = RequireClinicAuth();
            if (auth != null) return auth;
            return View();
        }

        public IActionResult Adoption()
        {
            var auth = RequireClinicAuth();
            if (auth != null) return auth;
            return View();
        }

        public IActionResult Reviews()
        {
            var auth = RequireClinicAuth();
            if (auth != null) return auth;
            return View();
        }

        public IActionResult Reports()
        {
            var auth = RequireClinicAuth();
            if (auth != null) return auth;
            return View();
        }

        public IActionResult Personal_nformation()
        {
            var auth = RequireClinicAuth();
            if (auth != null) return auth;
            return View();
        }

        public IActionResult Professional_nformation()
        {
            var auth = RequireClinicAuth();
            if (auth != null) return auth;
            return View();
        }

        public IActionResult Work_chedule()
        {
            var auth = RequireClinicAuth();
            if (auth != null) return auth;
            return View();
        }

        public IActionResult Documents_D()
        {
            var auth = RequireClinicAuth();
            if (auth != null) return auth;
            return View();
        }

        public IActionResult Password()
        {
            var auth = RequireClinicAuth();
            if (auth != null) return auth;
            return View();
        }

        public IActionResult Notifications()
        {
            var auth = RequireClinicAuth();
            if (auth != null) return auth;
            return View();
        }
    }
}