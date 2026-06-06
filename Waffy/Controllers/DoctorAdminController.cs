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
        private readonly IAppointmentRepository _apptRepo;
        private readonly INotificationRepository _notifRepo;
        private readonly IPetRepository _petRepo;
        private readonly IWalletRepository _walletRepo;
        private readonly IClinicRepository _clinicRepo;
        private readonly IReviewRepository _reviewRepo;
        public DoctorAdminController(
            IDoctorRepository doctorRepo,
            IWebHostEnvironment env,
            IAppointmentRepository apptRepo,
            INotificationRepository notifRepo,
            IPetRepository petRepo,
            IWalletRepository walletRepo,
            IClinicRepository clinicRepo,
            IReviewRepository reviewRepo)
        {
            _doctorRepo = doctorRepo;
            _env = env;
            _apptRepo = apptRepo;
            _notifRepo = notifRepo;
            _petRepo = petRepo;
            _walletRepo = walletRepo;
            _clinicRepo = clinicRepo;
            _reviewRepo = reviewRepo;
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

        public async Task<IActionResult> Appointments(string? statusFilter = null)
        {
            var auth = RequireDoctorAuth();
            if (auth != null) return auth;

            var doctorId = GetDoctorId();
            var info = await _doctorRepo.GetFullInfoByUserIdAsync(GetUserId());

            var all = (await _apptRepo.GetByDoctorIdAsync(doctorId)).ToList();
            var today = (await _apptRepo.GetTodayByDoctorIdAsync(doctorId)).ToList();

            var filtered = string.IsNullOrEmpty(statusFilter) || statusFilter == "All"
                ? all
                : all.Where(a => a.Status == statusFilter).ToList();

            var vm = new DoctorAppointmentsViewModel
            {
                Appointments = filtered,
                TodayAppointments = today,
                StatusFilter = statusFilter ?? "All",
                TotalCount = all.Count,
                TodayCount = today.Count,
                ConfirmedCount = all.Count(a => a.Status == "Confirmed"),
                CompletedCount = all.Count(a => a.Status == "Completed"),
                CancelledCount = all.Count(a => a.Status == "Cancelled"),
                FullName = info?.FullName ?? HttpContext.Session.GetString("UserName") ?? "Doctor",
                ClinicName = info?.ClinicName,
                VerificationStatus = info?.VerificationStatus,
                PendingCount = all.Count(a => a.Status == "PendingDoctorApproval"),
                Rating = info?.Rating
            };

            if (TempData["ApptSuccess"] != null) TempData.Keep("ApptSuccess");
            if (TempData["ApptError"] != null) TempData.Keep("ApptError");
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

            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAttended(int id)
        {
            var auth = RequireDoctorAuth();
            if (auth != null) return auth;

            var appt = await _apptRepo.GetByIdAsync(id);
            if (appt == null)
            {
                TempData["ApptError"] = "Appointment not found.";
                return RedirectToAction("Appointments");
            }

            await _apptRepo.CompleteAsync(id);
            await _apptRepo.ConfirmAttendanceAsync(id);

            // إرجاع الإيداع إذا كان مدفوعاً
            if (appt.DepositPaid && !appt.DepositReturned)
            {
                var wallet = await _walletRepo.GetByUserIdAsync(appt.IdUser);
                if (wallet == null)
                {
                    await _walletRepo.CreateWalletAsync(appt.IdUser);
                    wallet = await _walletRepo.GetByUserIdAsync(appt.IdUser);
                }
                if (wallet != null)
                {
                    await _walletRepo.AddBalanceAsync(wallet.IdWallet, appt.DepositAmount);
                    await _walletRepo.AddTransactionAsync(new WalletTransaction
                    {
                        IdWallet = wallet.IdWallet,
                        TransactionCode = $"REF-{Guid.NewGuid().ToString()[..8].ToUpper()}",
                        Type = "DepositReturn",
                        Amount = appt.DepositAmount,
                        PaymentMethod = "Waffy Wallet",
                        Status = "Completed",
                        CreatedAt = DateTime.UtcNow
                    });

                    await _apptRepo.MarkDepositReturnedAsync(id);
                    // -5 من محفظة العيادة لأن المبلغ رجع لليوزر
                    await _clinicRepo.AddClinicBalanceAsync(appt.IdClinic, -appt.DepositAmount);

                }
            }

            var doctorName = HttpContext.Session.GetString("UserName") ?? "Doctor";
            await _notifRepo.AddAsync(new Notification
            {
                IdUser = appt.IdUser,
                Type = "AppointmentCompleted",
                Title = "🏁 Visit Complete!",
                Message = $"Your visit with Dr. {doctorName} is complete. " +
                                    (appt.DepositPaid
                                        ? $"Your {appt.DepositAmount:F2} JOD deposit has been returned. 🐾"
                                        : "Thank you for visiting! 🐾"),
                IsRead = false,
                RelatedEntityType = "Appointment",
                RelatedEntityId = id,
                CreatedAt = DateTime.UtcNow
            });

            TempData["ApptSuccess"] = appt.DepositPaid
                ? $"🏁 Visit confirmed. {appt.DepositAmount:F2} JOD deposit returned to patient."
                : "🏁 Visit marked as complete.";
            return RedirectToAction("Appointments");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkNoShow(int id)
        {
            var auth = RequireDoctorAuth();
            if (auth != null) return auth;

            var appt = await _apptRepo.GetByIdAsync(id);
            if (appt == null)
            {
                TempData["ApptError"] = "Appointment not found.";
                return RedirectToAction("Appointments");
            }

            await _apptRepo.MarkNoShowAsync(id);

            // الإيداع يبقى محتجزاً — لا إرجاع

            await _notifRepo.AddAsync(new Notification
            {
                IdUser = appt.IdUser,
                Type = "AppointmentNoShow",
                Title = "⚠️ Missed Appointment",
                Message = $"You missed your appointment on " +
                                    $"{appt.AppointmentDate:dd MMM yyyy}. " +
                                    "Your 5 JOD deposit has been retained per our no-show policy.",
                IsRead = false,
                RelatedEntityType = "Appointment",
                RelatedEntityId = id,
                CreatedAt = DateTime.UtcNow
            });

            TempData["ApptError"] = "⚠️ No-show recorded. Deposit retained.";
            return RedirectToAction("Appointments");
        }
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelAppointment(int id, string reason)
        {
            var auth = RequireDoctorAuth();
            if (auth != null) return auth;

            var appt = await _apptRepo.GetByIdAsync(id);
            if (appt == null)
            {
                TempData["ApptError"] = "Appointment not found.";
                return RedirectToAction("Appointments");
            }

            await _apptRepo.CancelAsync(id, reason);

            var depositReturned = false;

            // إرجاع Wallet فوراً
            if (appt.DepositPaid && !appt.DepositReturned && appt.DepositPayMethod == "Wallet")
            {
                var wallet = await _walletRepo.GetByUserIdAsync(appt.IdUser);
                if (wallet == null)
                {
                    await _walletRepo.CreateWalletAsync(appt.IdUser);
                    wallet = await _walletRepo.GetByUserIdAsync(appt.IdUser);
                }
                if (wallet != null)
                {
                    await _walletRepo.AddBalanceAsync(wallet.IdWallet, appt.DepositAmount);
                    await _walletRepo.AddTransactionAsync(new WalletTransaction
                    {
                        IdWallet = wallet.IdWallet,
                        TransactionCode = $"REF-{Guid.NewGuid().ToString()[..8].ToUpper()}",
                        Type = "DepositReturn",
                        Amount = appt.DepositAmount,
                        PaymentMethod = "Waffy Wallet",
                        Status = "Completed",
                        CreatedAt = DateTime.UtcNow
                    });
                    await _apptRepo.MarkDepositReturnedAsync(id);
                    // -5 من محفظة العيادة لأن المبلغ رجع لليوزر
                    await _clinicRepo.AddClinicBalanceAsync(appt.IdClinic, -appt.DepositAmount);
                    depositReturned = true;
                }
            }
            // CliQ مؤكد → يحتاج إرجاع يدوي من العيادة
            else if (appt.DepositPaid && !appt.DepositReturned && appt.DepositPayMethod == "CliQ")
            {
                await _apptRepo.MarkCliqRefundPendingAsync(id);
            }

            var doctorName = HttpContext.Session.GetString("UserName") ?? "the doctor";
            await _notifRepo.AddAsync(new Notification
            {
                IdUser = appt.IdUser,
                Type = "AppointmentCancelledByDoctor",
                Title = "❌ Appointment Cancelled",
                Message = $"Your appointment with Dr. {doctorName} on " +
                                    $"{appt.AppointmentDate:dd MMM yyyy} was cancelled." +
                                    (!string.IsNullOrEmpty(reason) ? $" Reason: {reason}." : "") +
                                    (depositReturned ? $" Your {appt.DepositAmount:F2} JOD has been returned." :
                                     appt.DepositPayMethod == "CliQ" && appt.DepositPaid
                                        ? " The clinic will process your CliQ refund shortly." : ""),
                IsRead = false,
                RelatedEntityType = "Appointment",
                RelatedEntityId = id,
                CreatedAt = DateTime.UtcNow
            });

            TempData["ApptError"] = depositReturned
                ? $"❌ Cancelled. Deposit returned to patient."
                : "❌ Appointment cancelled. User notified.";
            return RedirectToAction("Appointments");
        }

        public async Task<IActionResult> Patients()
        {
            var auth = RequireDoctorAuth();
            if (auth != null) return auth;

            var doctorId = GetDoctorId();
            var info = await _doctorRepo.GetFullInfoByUserIdAsync(GetUserId());
            var drName = info?.FullName
                        ?? HttpContext.Session.GetString("UserName")
                        ?? "Doctor";

            var allAppts = (await _apptRepo.GetByDoctorIdAsync(doctorId))
                .Where(a => a.IdPet.HasValue && a.IdPet > 0)
                .ToList();

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
                                             DoctorName = drName,
                                             ServiceName = a.ServiceName,
                                             MedicalNote = a.MedicalRecordNote,
                                             Status = a.Status
                                         }).ToList()
                });
            }

            var thisMonth = DateTime.Now.Month;
            return View(new ClinicPatientsViewModel
            {
                Patients = patients.OrderByDescending(p => p.LastVisitDate).ToList(),
                TotalPatients = patients.Count,
                PatientsWithExpiredVacc = patients.Count(p =>
                    p.VaccinationStatus is "NotVaccinated" or "Partial" or null),
                PatientsVisitedThisMonth = patients.Count(p =>
                    p.LastVisitDate.HasValue &&
                    p.LastVisitDate.Value.Month == thisMonth &&
                    p.LastVisitDate.Value.Year == DateTime.Now.Year)
            });
        }

        [HttpGet]
        public async Task<IActionResult> Medical_Records()
        {
            var auth = RequireDoctorAuth();
            if (auth != null) return auth;

            var doctorId = GetDoctorId();
            var info = await _doctorRepo.GetFullInfoByUserIdAsync(GetUserId());

            // جلب المواعيد المكتملة فقط (السجلات الطبية)
            var all = (await _apptRepo.GetByDoctorIdAsync(doctorId)).ToList();
            var completed = all.Where(a => a.Status == "Completed")
                               .OrderByDescending(a => a.AppointmentDate)
                               .ToList();

            // إثراء بصور الحيوانات
            foreach (var a in completed)
            {
                if (a.IdPet == null || a.IdPet <= 0) continue;
                var pet = await _petRepo.GetByIdAsync(a.IdPet.Value);
                if (pet == null) continue;
                var imgs = (await _petRepo.GetImagesAsync(pet.IdPet)).ToList();
                a.PetImageUrl = imgs.FirstOrDefault(i => i.IsMain)?.ImageUrl
                             ?? imgs.FirstOrDefault()?.ImageUrl
                             ?? pet.ImageUrl;
            }

            // نستخدم DoctorAppointmentsViewModel مع المواعيد المكتملة
            return View(new DoctorAppointmentsViewModel
            {
                Appointments = completed,
                TodayAppointments = new List<AppointmentDto>(),
                StatusFilter = "Completed",
                TotalCount = all.Count,
                TodayCount = 0,
                ConfirmedCount = all.Count(a => a.Status == "Confirmed"),
                CompletedCount = completed.Count,
                CancelledCount = all.Count(a => a.Status == "Cancelled"),
                FullName = info?.FullName
                                     ?? HttpContext.Session.GetString("UserName")
                                     ?? "Doctor",
                ClinicName = info?.ClinicName,
                VerificationStatus = info?.VerificationStatus,
                Rating = info?.Rating
            });
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

        // ── 3. حفظ ملاحظات الطبيب على الموعد ────────────────────────────
       [HttpPost]
[IgnoreAntiforgeryToken]
public async Task<IActionResult> SaveDoctorNotes(
            int appointmentId,
            string? diagnosis,
            string? treatment,
            string? prescription)
        {
            var auth = RequireDoctorAuth();
            if (auth != null) return Unauthorized();

            var note = new System.Text.StringBuilder();
            if (!string.IsNullOrEmpty(diagnosis)) note.AppendLine($"🩺 Diagnosis: {diagnosis}");
            if (!string.IsNullOrEmpty(treatment)) note.AppendLine($"💊 Treatment: {treatment}");
            if (!string.IsNullOrEmpty(prescription)) note.AppendLine($"📋 Prescription: {prescription}");

            await _apptRepo.UpdateMedicalNoteAsync(appointmentId, note.ToString().Trim());

            return Json(new { success = true });
        }

        // ══════════════════════════════════════════════════════════════════════
        // ملف: DoctorAdminController.cs  → أضيفي في نهاية الـ class
        // ملف: ClinicAdminController.cs  → نفسه لكن RequireClinicAuth()
        // ══════════════════════════════════════════════════════════════════════

        // ── GET /DoctorAdmin/GetPetRecord?petId=X ─────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetPetRecord(int petId)
        {
            var auth = RequireDoctorAuth();           // ClinicAdmin → RequireClinicAuth()
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
            var auth = RequireDoctorAuth();           // ClinicAdmin → RequireClinicAuth()
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

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveAppointment(int id)
        {
            var auth = RequireDoctorAuth();
            if (auth != null) return auth;

            var appt = await _apptRepo.GetByIdAsync(id);
            if (appt == null)
            {
                TempData["ApptError"] = "Appointment not found.";
                return RedirectToAction("Appointments");
            }

            // منع الموافقة إذا CliQ غير مؤكد من العيادة
            if (appt.DepositPayMethod == "CliQ" && !appt.DepositPaid)
            {
                TempData["ApptError"] =
                    "⚠️ Cannot approve yet — CliQ deposit not confirmed by clinic admin.";
                return RedirectToAction("Appointments");
            }

            await _apptRepo.ApproveByDoctorAsync(id);

            var doctorName = HttpContext.Session.GetString("UserName") ?? "Doctor";
            await _notifRepo.AddAsync(new Notification
            {
                IdUser = appt.IdUser,
                Type = "AppointmentConfirmed",
                Title = "✅ Appointment Confirmed!",
                Message = $"Your appointment with Dr. {doctorName} on " +
                                    $"{appt.AppointmentDate:dd MMM yyyy} at " +
                                    $"{appt.StartTime:hh\\:mm} has been confirmed. 🐾",
                IsRead = false,
                RelatedEntityType = "Appointment",
                RelatedEntityId = id,
                CreatedAt = DateTime.UtcNow
            });

            TempData["ApptSuccess"] = "✅ Appointment approved and confirmed.";
            return RedirectToAction("Appointments");
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> SaveDoctorNotes(
    int appointmentId,
    string? diagnosis,
    string? symptoms,
    string? treatment,
    string? prescription)
        {
            var auth = RequireDoctorAuth();
            if (auth != null) return Unauthorized();

            var note = new System.Text.StringBuilder();
            if (!string.IsNullOrEmpty(diagnosis)) note.AppendLine($"🩺 Diagnosis: {diagnosis}");
            if (!string.IsNullOrEmpty(symptoms)) note.AppendLine($"🔍 Symptoms: {symptoms}");
            if (!string.IsNullOrEmpty(treatment)) note.AppendLine($"💊 Treatment: {treatment}");
            if (!string.IsNullOrEmpty(prescription)) note.AppendLine($"📋 Prescription: {prescription}");

            await _apptRepo.UpdateMedicalNoteAsync(appointmentId, note.ToString().Trim());
            return Json(new { success = true });
        }

        [HttpGet]
        public async Task<IActionResult> Reviews()
        {
            var doctorId = HttpContext.Session.GetInt32("DoctorId");
            if (doctorId == null) return RedirectToAction("Login", "Account");

            var reviews = (await _reviewRepo.GetByTargetAsync("Doctor", doctorId.Value)).ToList();
            var avg = reviews.Any() ? reviews.Average(r => r.Rating) : 0.0;

            ViewBag.Reviews = reviews;
            ViewBag.TotalCount = reviews.Count;
            ViewBag.AvgRating = Math.Round(avg, 1);
            ViewBag.Star5 = reviews.Count(r => r.Rating == 5);
            ViewBag.Star4 = reviews.Count(r => r.Rating == 4);
            ViewBag.Star3 = reviews.Count(r => r.Rating == 3);
            ViewBag.Star2 = reviews.Count(r => r.Rating == 2);
            ViewBag.Star1 = reviews.Count(r => r.Rating == 1);

            return View();
        }

    }
}