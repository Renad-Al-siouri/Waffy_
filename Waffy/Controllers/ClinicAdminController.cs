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
        private readonly IAppointmentRepository _apptRepo; // ← جديد
        private readonly INotificationRepository _notifRepo;
        private readonly IPetRepository _petRepo;
        private readonly IWalletRepository _walletRepo;


        public ClinicAdminController(IClinicRepository clinicRepo,
        IWalletRepository walletRepo,
        ILocationRepository locationRepo,
        IClinicScheduleRepository scheduleRepo,
        IClinicServiceRepository serviceRepo,
        IClinicDocumentRepository docRepo,      
        IWebHostEnvironment env,
        IClinicImageRepository imageRepo,
        IAppointmentRepository apptRepo,
        INotificationRepository notifRepo,
        IDoctorRepository doctorRepo,
        IPetRepository petRepo)
        {
            _walletRepo = walletRepo;
            _clinicRepo = clinicRepo;
            _locationRepo = locationRepo;
            _scheduleRepo = scheduleRepo;
            _serviceRepo = serviceRepo;
            _docRepo = docRepo;   
            _env = env;
            _imageRepo = imageRepo;
            _doctorRepo = doctorRepo;
            _apptRepo = apptRepo;
            _notifRepo = notifRepo;
            _petRepo = petRepo;
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


        [HttpGet]
        public async Task<IActionResult> Appointments(string? statusFilter = null)
        {
            var auth = RequireClinicAuth();
            if (auth != null) return auth;

            var clinicId = GetCurrentClinicId();
            if (clinicId == null) return RedirectToAction("Login", "Account");

            // ── 1. جلب جميع المواعيد ─────────────────────────────
            var all = (await _apptRepo.GetByClinicIdAsync(clinicId.Value)).ToList();

            // ── 2. إثراء بصور الحيوانات ──────────────────────────
            foreach (var a in all)
            {
                if (a.IdPet == null || a.IdPet <= 0) continue;
                var pet = await _petRepo.GetByIdAsync(a.IdPet.Value);
                if (pet == null) continue;
                var imgs = (await _petRepo.GetImagesAsync(pet.IdPet)).ToList();
                a.PetImageUrl = imgs.FirstOrDefault(i => i.IsMain)?.ImageUrl
                             ?? imgs.FirstOrDefault()?.ImageUrl
                             ?? pet.ImageUrl;
            }

            // ── 3. فلترة حسب الـ Status ──────────────────────────
            var filtered = string.IsNullOrEmpty(statusFilter) || statusFilter == "All"
                ? all
                : all.Where(a => a.Status == statusFilter).ToList();

            // ── 4. جلب أطباء العيادة + عدد مواعيد كل طبيب اليوم ─
            var today = DateTime.Today;
            var rawDoctors = (await _doctorRepo.GetByClinicIdAsync(clinicId.Value)).ToList();

            var slimDoctors = rawDoctors.Select(d => new ClinicDoctorSlimDto
            {
                IdDoctor = d.IdDoctor,
                FullName = d.FullName,
                Specialization = d.Specialization,
                WorkingDays = null,   // ← إذا غير موجود
                MaxDailyAppointments = null,
                IsAvailable = d.IsAvailable
            }).ToList();

            var docCounts = new Dictionary<int, int>();
            foreach (var doc in rawDoctors)
            {
                var cnt = await _apptRepo.CountByDoctorAndDateAsync(doc.IdDoctor, today);
                docCounts[doc.IdDoctor] = cnt;
            }

            // ── 5. بناء الـ ViewModel ─────────────────────────────
            var vm = new ClinicAppointmentsViewModel
            {
                Appointments = filtered,
                StatusFilter = statusFilter ?? "All",
                TotalCount = all.Count,
                TodayCount = all.Count(a => a.AppointmentDate.Date == today
                                                      && a.Status != "Cancelled"),
                ConfirmedCount = all.Count(a => a.Status == "Confirmed"),
                CompletedCount = all.Count(a => a.Status == "Completed"),
                CancelledCount = all.Count(a => a.Status == "Cancelled"),
                DoctorNames = all
                                            .Select(a => a.DoctorName)
                                            .Where(n => !string.IsNullOrEmpty(n))
                                            .Distinct()
                                            .OrderBy(n => n)
                                            .ToList()!,
                AvailableDoctors = slimDoctors,
                DoctorAppointmentCounts = docCounts
            };
            vm.TodayAppointments = all
    .Where(a => a.AppointmentDate.Date == today
             && a.Status != "Cancelled"
             && a.Status != "NoShow")
    .OrderBy(a => a.StartTime)
    .ToList();
            if (TempData["ApptSuccess"] != null) TempData.Keep("ApptSuccess");
            if (TempData["ApptError"] != null) TempData.Keep("ApptError");

            return View(vm);
        }
        // 3. Complete
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteAppointment(int id)
        {
            var auth = RequireClinicAuth();
            if (auth != null) return auth;

            await _apptRepo.CompleteAsync(id);
            TempData["ApptSuccess"] = "✅ Appointment marked as completed.";
            return RedirectToAction("Appointments");
        }

        // 4. Cancel
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelAppointment(int id, string reason)
        {
            var auth = RequireClinicAuth();
            if (auth != null) return auth;

            // جلب الموعد أولاً للحصول على IdUser
            var appt = await _apptRepo.GetByIdAsync(id);
            if (appt == null)
            {
                TempData["ApptError"] = "Appointment not found.";
                return RedirectToAction("Appointments");
            }

            // إلغاء الموعد
            await _apptRepo.CancelAsync(id, reason);

            // ★ إشعار لليوزر بأن العيادة ألغت موعده
            var clinicName = HttpContext.Session.GetString("ClinicName") ?? "the clinic";
            await _notifRepo.AddAsync(new Notification
            {
                IdUser = appt.IdUser,
                Type = "AppointmentCancelledByClinic",
                Title = "❌ Appointment Cancelled",
                Message = $"Your appointment at {clinicName} on " +
                                    $"{appt.AppointmentDate:dd MMM yyyy} at {appt.StartTime:hh\\:mm} " +
                                    $"has been cancelled by the clinic." +
                                    (!string.IsNullOrEmpty(reason) ? $" Reason: {reason}" : "") +
                                    " Please book another appointment if needed.",
                IsRead = false,
                RelatedEntityType = "Appointment",
                RelatedEntityId = id,
                CreatedAt = DateTime.UtcNow
            });

            TempData["ApptError"] = "❌ Appointment cancelled. User has been notified.";
            return RedirectToAction("Appointments");
        }
        [HttpGet]
        public async Task<IActionResult> Patients()
        {
            var auth = RequireClinicAuth();
            if (auth != null) return auth;

            var clinicId = GetCurrentClinicId();
            if (clinicId == null) return RedirectToAction("Login", "Account");

            // جلب كل المواعيد التي عندها حيوان
            var allAppts = (await _apptRepo.GetByClinicIdAsync(clinicId.Value))
                .Where(a => a.IdPet.HasValue && a.IdPet > 0)
                .ToList();

            // تجميع بالحيوان
            var groups = allAppts.GroupBy(a => a.IdPet!.Value).ToList();
            var patients = new List<ClinicPatientDto>();

            foreach (var g in groups)
            {
                var pet = await _petRepo.GetByIdAsync(g.Key);
                if (pet == null) continue;

                var imgs = (await _petRepo.GetImagesAsync(g.Key)).ToList();
                var img = imgs.FirstOrDefault(i => i.IsMain)?.ImageUrl
                        ?? imgs.FirstOrDefault()?.ImageUrl
                        ?? pet.ImageUrl;

                var latest = g.OrderByDescending(a => a.AppointmentDate).First();

                patients.Add(new ClinicPatientDto
                {
                    IdPet = pet.IdPet,
                    PetName = pet.Name ?? "Unknown",
                    Species = pet.Species,
                    Breed = pet.Breed,
                    Gender = pet.Gender,
                    Color = pet.Color,
                    ImageUrl = img,
                    AgeDisplay = pet.AgeDisplay,
                    MicrochipId = pet.MicrochipId,
                    Weight = pet.Weight,
                    VaccinationStatus = pet.VaccinationStatus,
                    IsNeutered = pet.IsNeutered,
                    MedicalNotes = pet.MedicalNotes,
                    Traits = pet.Traits,
                    EnergyLevel = pet.EnergyLevel,
                    GoodWithChildren = pet.GoodWithChildren,
                    GoodWithPets = pet.GoodWithPets,
                    IsTrained = pet.IsTrained,
                    OwnerName = latest.UserFullName ?? "—",
                    OwnerPhone = latest.UserPhone,
                    LastVisitDate = g.Max(a => a.AppointmentDate),
                    TotalVisits = g.Count(a => a.Status == "Completed"),
                    FirstSeen = g.Min(a => a.CreatedAt),
                    VisitHistory = g.OrderByDescending(a => a.AppointmentDate)
                                        .Select(a => new PatientVisitDto
                                        {
                                            AppointmentId = a.Id,
                                            VisitDate = a.AppointmentDate,
                                            DoctorName = a.DoctorName,
                                            ServiceName = a.ServiceName,
                                            MedicalNote = a.MedicalRecordNote,
                                            Status = a.Status
                                        }).ToList()
                });
            }

            var thisMonth = DateTime.Now.Month;
            var vm = new ClinicPatientsViewModel
            {
                Patients = patients.OrderByDescending(p => p.LastVisitDate).ToList(),
                TotalPatients = patients.Count,
                PatientsWithExpiredVacc = patients.Count(p =>
                    p.VaccinationStatus is "NotVaccinated" or "Partial" or null),
                PatientsVisitedThisMonth = patients.Count(p =>
                    p.LastVisitDate.HasValue &&
                    p.LastVisitDate.Value.Month == thisMonth &&
                    p.LastVisitDate.Value.Year == DateTime.Now.Year)
            };

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> Medical_Records()
        {
            var auth = RequireClinicAuth();
            if (auth != null) return auth;

            var clinicId = GetCurrentClinicId();
            if (clinicId == null) return RedirectToAction("Login", "Account");

            // جلب جميع مواعيد العيادة
            var all = (await _apptRepo.GetByClinicIdAsync(clinicId.Value)).ToList();

            // إثراء بصور الحيوانات
            foreach (var a in all)
            {
                if (a.IdPet == null || a.IdPet <= 0) continue;
                var pet = await _petRepo.GetByIdAsync(a.IdPet.Value);
                if (pet == null) continue;
                var imgs = (await _petRepo.GetImagesAsync(pet.IdPet)).ToList();
                a.PetImageUrl = imgs.FirstOrDefault(i => i.IsMain)?.ImageUrl
                             ?? imgs.FirstOrDefault()?.ImageUrl
                             ?? pet.ImageUrl;
            }

            // نستخدم ClinicAppointmentsViewModel — المشاهدة ستفلتر Completed
            var today = DateTime.Today;
            return View(new ClinicAppointmentsViewModel
            {
                Appointments = all,               // الفلتر يتم في الـ View
                StatusFilter = "Completed",
                TotalCount = all.Count,
                TodayCount = all.Count(a => a.AppointmentDate.Date == today && a.Status != "Cancelled"),
                ConfirmedCount = all.Count(a => a.Status == "Confirmed"),
                CompletedCount = all.Count(a => a.Status == "Completed"),
                CancelledCount = all.Count(a => a.Status == "Cancelled"),
                DoctorNames = all.Select(a => a.DoctorName)
                                    .Where(n => !string.IsNullOrEmpty(n))
                                    .Distinct().OrderBy(n => n)
                                    .ToList()!
            });
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

            // ✅ 1. تحقق من الحقول المطلوبة
            if (string.IsNullOrWhiteSpace(vm.FullName) ||
                string.IsNullOrWhiteSpace(vm.Email) ||
                string.IsNullOrWhiteSpace(vm.Phone) ||
                string.IsNullOrWhiteSpace(vm.Password))
            {
                TempData["DoctorError"] = "Please fill all required fields.";
                return RedirectToAction("Doctors");
            }

            // ✅ 2. تحقق من حجم الملف أولاً — قبل أي عملية على DB
            if (vm.CertificationFile != null && vm.CertificationFile.Length > 5 * 1024 * 1024)
            {
                TempData["DoctorError"] = "Certification file too large. Max 5MB.";
                return RedirectToAction("Doctors");
            }

            // ✅ 3. تحقق من تكرار الإيميل
            if (await _doctorRepo.EmailExistsAsync(vm.Email.Trim()))
            {
                TempData["DoctorError"] = "This email is already registered.";
                return RedirectToAction("Doctors");
            }

            // ✅ 4. إنشاء User (بعد كل التحققات)
            var userId = await _doctorRepo.CreateUserAsync(
                vm.FullName.Trim(), vm.Email.Trim(),
                vm.Phone.Trim(), vm.Password);

            // ✅ 5. حفظ الملف (بعد إنشاء المستخدم)
            string? certUrl = null;
            if (vm.CertificationFile != null && vm.CertificationFile.Length > 0)
            {
                certUrl = await SaveFileAsync(vm.CertificationFile, "doctor-certs");
            }

            await _doctorRepo.AddDoctorAsync(new Doctor
            {
                IdUser = userId,
                IdClinic = clinicId.Value,
                Specialization = vm.Specialization,
                ExperienceYears = vm.ExperienceYears,
                Certifications = certUrl,
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
                Longitude = clinic.Longitude,
                CliqNumber = clinic.CliqNumber   // ← أضيفي هذا
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
            clinic.CliqNumber = vm.CliqNumber?.Trim();
            clinic.Latitude = vm.Latitude;
            clinic.Longitude = vm.Longitude;

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
           // var clinic = await _clinicRepo.GetByIdAsync(clinicId.Value);
            //if (clinic != null && clinic.VerificationStatus == "Approved")
           // {
             //   var origLat = clinic.Latitude;
              //  var origLng = clinic.Longitude;
                //clinic.VerificationStatus = "Under Review";
                //clinic.Latitude = origLat;
                //clinic.Longitude = origLng;
                //await _clinicRepo.UpdateAsync(clinic);
            //}

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

        [HttpGet]
        public async Task<IActionResult> DoctorInfo(int id)
        {
            var auth = RequireClinicAuth();
            if (auth != null) return auth;

            var clinicId = GetCurrentClinicId();
            if (clinicId == null) return RedirectToAction("Login", "Account");

            var doctor = await _doctorRepo.GetFullInfoByDoctorIdAsync(id);
            if (doctor == null || doctor.IdClinic != clinicId.Value)
                return NotFound();

            // جلب كل الوثائق
            var docs = (await _doctorRepo.GetDocumentsByDoctorIdAsync(id)).ToList();

            ViewBag.AllDocs = docs;
            ViewBag.CertDocs = docs.Where(d => d.DocumentType == "Certification").ToList();
            ViewBag.OtherDocs = docs.Where(d => d.DocumentType != "Certification").ToList();

            return View(doctor);
        }

       

       
        // ══════════════════════════════════════════════════════════════════════
        // ملف: DoctorAdminController.cs  → أضيفي في نهاية الـ class
        // ملف: ClinicAdminController.cs  → نفسه لكن RequireClinicAuth()
        // ══════════════════════════════════════════════════════════════════════

        // ── GET /DoctorAdmin/GetPetRecord?petId=X ─────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetPetRecord(int petId)
        {
            var auth = RequireClinicAuth();           // ClinicAdmin → RequireClinicAuth()
            if (auth != null) return Unauthorized();

            var pet = await _petRepo.GetByIdAsync(petId);
            if (pet == null) return NotFound();

            var images = (await _petRepo.GetImagesAsync(petId)).ToList();
            var mainImage = images.FirstOrDefault(i => i.IsMain)?.ImageUrl
                         ?? images.FirstOrDefault()?.ImageUrl
                         ?? pet.ImageUrl;

            return Json(new
            {
                idPet = pet.IdPet,
                name = pet.Name ?? "—",
                species = pet.Species ?? "—",
                breed = pet.Breed ?? "—",
                gender = pet.Gender ?? "—",
                ageDisplay = pet.AgeDisplay,
                color = pet.Color ?? "—",
                microchipId = pet.MicrochipId ?? "—",
                weight = pet.Weight,
                vaccinationStatus = pet.VaccinationStatus ?? "Unknown",
                isNeutered = pet.IsNeutered,
                medicalNotes = pet.MedicalNotes ?? "",
                traits = pet.Traits ?? "",
                energyLevel = pet.EnergyLevel ?? "—",
                goodWithChildren = pet.GoodWithChildren,
                goodWithPets = pet.GoodWithPets,
                isTrained = pet.IsTrained,
                imageUrl = mainImage ?? ""
            });
        }

        // ── POST /DoctorAdmin/UpdatePetMedical ────────────────────────────────
        // ★ [IgnoreAntiforgeryToken] ضروري لأن الطلب AJAX بـ FormData
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> UpdatePetMedical(
            int petId,
            string? vaccinationStatus,
            bool isNeutered,
            string? medicalNotes,
            string? traits,
            string? energyLevel,
            decimal? weight,
            bool goodWithChildren,
            bool goodWithPets,
            bool isTrained)
        {
            var auth = RequireClinicAuth();           // ClinicAdmin → RequireClinicAuth()
            if (auth != null) return Unauthorized();

            var pet = await _petRepo.GetByIdAsync(petId);
            if (pet == null) return Json(new { success = false, message = "Pet not found." });

            // ✅ تحديث الحقول الطبية فقط — الاسم والنوع والفصيلة مسؤولية المالك
            pet.VaccinationStatus = vaccinationStatus;
            pet.IsNeutered = isNeutered;
            pet.MedicalNotes = medicalNotes;
            pet.Traits = traits;
            pet.EnergyLevel = energyLevel;
            pet.Weight = weight;
            pet.GoodWithChildren = goodWithChildren;
            pet.GoodWithPets = goodWithPets;
            pet.IsTrained = isTrained;

            await _petRepo.UpdateAsync(pet);

            return Json(new { success = true });
        }

        // ── 3. حفظ ملاحظات الطبيب على الموعد ────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveDoctorNotes(
            int appointmentId,
            string? diagnosis,
            string? treatment,
            string? prescription)
        {
            var auth = RequireClinicAuth();
            if (auth != null) return Unauthorized();

            var note = new System.Text.StringBuilder();
            if (!string.IsNullOrEmpty(diagnosis)) note.AppendLine($"🩺 Diagnosis: {diagnosis}");
            if (!string.IsNullOrEmpty(treatment)) note.AppendLine($"💊 Treatment: {treatment}");
            if (!string.IsNullOrEmpty(prescription)) note.AppendLine($"📋 Prescription: {prescription}");

            await _apptRepo.UpdateMedicalNoteAsync(appointmentId, note.ToString().Trim());

            return Json(new { success = true });
        }

        // ── Action: الموافقة + تعيين طبيب ────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveAndAssign(int appointmentId, int assignedDoctorId)
        {
            var auth = RequireClinicAuth();
            if (auth != null) return auth;

            // تحقق من توفر الطبيب
            var appt = await _apptRepo.GetByIdAsync(appointmentId);
            if (appt == null)
            {
                TempData["ApptError"] = "Appointment not found.";
                return RedirectToAction("Appointments");
            }

            var slotFree = await _apptRepo.IsSlotAvailableAsync(
                assignedDoctorId, appt.AppointmentDate, appt.StartTime);

            if (!slotFree)
            {
                TempData["ApptError"] = "⚠️ Doctor is not available at this time slot.";
                return RedirectToAction("Appointments");
            }

            var todayCount = await _apptRepo.CountByDoctorAndDateAsync(
                assignedDoctorId, appt.AppointmentDate);
            var docInfo = await _doctorRepo.GetFullInfoByDoctorIdAsync(assignedDoctorId);
            var maxDaily = docInfo?.MaxDailyAppointments ?? 12;

            if (todayCount >= maxDaily)
            {
                TempData["ApptError"] = $"⚠️ Doctor has reached daily limit ({maxDaily} appointments).";
                return RedirectToAction("Appointments");
            }

            await _apptRepo.AssignDoctorAsync(appointmentId, assignedDoctorId);
            // إشعار لليوزر
            await _notifRepo.AddAsync(new Notification
            {
                IdUser = appt.IdUser,
                Type = "AppointmentConfirmed",
                Title = "✅ Appointment Confirmed!",
                Message = $"Your appointment at {HttpContext.Session.GetString("ClinicName")} " +
                                    $"on {appt.AppointmentDate:dd MMM yyyy} at {appt.StartTime:hh\\:mm} " +
                                   $"A doctor has been assigned to your appointment on " +
$"{appt.AppointmentDate:dd MMM yyyy} at {appt.StartTime:hh\\:mm}. " +
$"Dr. {docInfo?.FullName} will review and confirm shortly. 🐾",
                IsRead = false,
                RelatedEntityType = "Appointment",
                RelatedEntityId = appointmentId,
                CreatedAt = DateTime.UtcNow
            });

            TempData["ApptSuccess"] = $"✅ Assigned to Dr. {docInfo?.FullName}. Awaiting doctor confirmation.";
            return RedirectToAction("Appointments");
        }

        // ── Action: رفض الموعد ───────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectAppointment(int id, string reason)
        {
            var auth = RequireClinicAuth();
            if (auth != null) return auth;

            var appt = await _apptRepo.GetByIdAsync(id);
            if (appt == null) return RedirectToAction("Appointments");

            await _apptRepo.CancelAsync(id, reason);

            // إرجاع الإيداع فوراً
            if (appt.DepositPaid && !appt.DepositReturned)
            {
                // -5 من محفظة العيادة لأن المبلغ رجع لليوزر
await _clinicRepo.AddClinicBalanceAsync(appt.IdClinic, -appt.DepositAmount);
            }

            await _notifRepo.AddAsync(new Notification
            {
                IdUser = appt.IdUser,
                Type = "AppointmentRejectedByClinic",
                Title = "❌ Appointment Request Rejected",
                Message = $"Your appointment request at " +
                                    $"{HttpContext.Session.GetString("ClinicName") ?? "the clinic"} " +
                                    $"was rejected. Reason: {reason}. " +
                                    (appt.DepositPaid ? "Your 5 JOD deposit has been returned. " : "") +
                                    "You can book another appointment.",
                IsRead = false,
                RelatedEntityType = "Appointment",
                RelatedEntityId = id,
                CreatedAt = DateTime.UtcNow
            });

            TempData["ApptError"] = "❌ Appointment rejected." +
                                    (appt.DepositPaid ? " Deposit returned to user." : "");
            return RedirectToAction("Appointments");
        }

        // ── Action: تأكيد حضور المريض ────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmAttendance(int id)
        {
            var auth = RequireClinicAuth();
            if (auth != null) return auth;

            var appt = await _apptRepo.GetByIdAsync(id);
            if (appt == null) return RedirectToAction("Appointments");

            await _apptRepo.CompleteAsync(id);
            await _apptRepo.ConfirmAttendanceAsync(id);

            if (appt.DepositPaid && !appt.DepositReturned)
            {
                await ReturnDepositToUserAsync(appt.IdUser, appt.DepositAmount, id);
                // -5 من محفظة العيادة لأن المبلغ رجع لليوزر
                await _clinicRepo.AddClinicBalanceAsync(appt.IdClinic, -appt.DepositAmount);
            }

            await _notifRepo.AddAsync(new Notification
            {
                IdUser = appt.IdUser,
                Type = "AppointmentCompleted",
                Title = "✅ Visit Complete — Deposit Returned",
                Message = "Your visit is marked as complete. " +
                                    "Your 5 JOD deposit has been returned to your wallet. 🐾",
                IsRead = false,
                RelatedEntityType = "Appointment",
                RelatedEntityId = id,
                CreatedAt = DateTime.UtcNow
            });

            TempData["ApptSuccess"] = "✅ Attendance confirmed. Deposit returned.";
            return RedirectToAction("Appointments");
        }

        // ── Action: تسجيل غياب ──────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkNoShow(int id)
        {
            var auth = RequireClinicAuth();
            if (auth != null) return auth;

            var appt = await _apptRepo.GetByIdAsync(id);
            if (appt == null) return RedirectToAction("Appointments");

            await _apptRepo.MarkNoShowAsync(id);

            await _notifRepo.AddAsync(new Notification
            {
                IdUser = appt.IdUser,
                Type = "AppointmentNoShow",
                Title = "⚠️ Missed Appointment",
                Message = $"You missed your appointment on {appt.AppointmentDate:dd MMM yyyy}. " +
                                    "Your 5 JOD deposit has been retained per our cancellation policy.",
                IsRead = false,
                RelatedEntityType = "Appointment",
                RelatedEntityId = id,
                CreatedAt = DateTime.UtcNow
            });

            TempData["ApptError"] = "⚠️ No-show recorded. Deposit retained.";
            return RedirectToAction("Appointments");
        }

        // ── Helper: إرجاع الإيداع ────────────────────────────
        private async Task ReturnDepositToUserAsync(int userId, decimal amount, int appointmentId)
        {
            var wallet = await _walletRepo.GetByUserIdAsync(userId);
            if (wallet == null)
            {
                await _walletRepo.CreateWalletAsync(userId);
                wallet = await _walletRepo.GetByUserIdAsync(userId);
            }
            if (wallet == null) return;

            await _walletRepo.AddBalanceAsync(wallet.IdWallet, amount);
            await _walletRepo.AddTransactionAsync(new WalletTransaction
            {
                IdWallet = wallet.IdWallet,
                TransactionCode = $"DEP-{Guid.NewGuid().ToString()[..8].ToUpper()}",
                Type = "DepositReturn",
                Amount = amount,
                PaymentMethod = "Waffy Deposit",
                Status = "Completed",
                CreatedAt = DateTime.UtcNow
            });
            await _apptRepo.MarkDepositReturnedAsync(appointmentId);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmCliqPayment(int id)
        {
            var auth = RequireClinicAuth();
            if (auth != null) return auth;

            var appt = await _apptRepo.GetByIdAsync(id);
            if (appt == null) return RedirectToAction("Appointments");

            // تأكيد الدفع
            await _apptRepo.MarkDepositPaidAsync(id, "CliQ");

            // تحديد الـ Status التالي بناءً على اختيار الطبيب الأصلي
            var nextStatus = appt.IdDoctor > 0
     ? "PendingDoctorApproval"
     : "PendingClinicApproval";

            await _apptRepo.SetStatusAsync(id, nextStatus);

            // إشعار لليوزر
            await _notifRepo.AddAsync(new Notification
            {
                IdUser = appt.IdUser,
                Type = "CliQPaymentConfirmed",
                Title = "✅ CliQ Payment Confirmed",
                Message = nextStatus == "PendingDoctorApproval"
                    ? "Your 5 JOD CliQ payment has been confirmed. The doctor will review your request shortly. 🐾"
                    : "Your 5 JOD CliQ payment has been confirmed. The clinic will assign a doctor shortly. 🐾",
                IsRead = false,
                RelatedEntityType = "Appointment",
                RelatedEntityId = id,
                CreatedAt = DateTime.UtcNow
            });

            TempData["ApptSuccess"] = $"✅ CliQ payment confirmed. Appointment moved to {nextStatus}.";
            return RedirectToAction("Appointments");
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