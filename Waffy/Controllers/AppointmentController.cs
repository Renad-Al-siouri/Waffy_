using Microsoft.AspNetCore.Mvc;
using Waffy.Interfaces;
using Waffy.Models;
using Waffy.Models.ViewModels;

namespace Waffy.Controllers
{
    public class AppointmentController : Controller
    {
        private readonly IAppointmentRepository _apptRepo;
        private readonly IClinicRepository _clinicRepo;
        private readonly IDoctorRepository _doctorRepo;
        private readonly IClinicServiceRepository _serviceRepo;
        private readonly IClinicScheduleRepository _scheduleRepo;
        private readonly IPetRepository _petRepo;
        private readonly INotificationRepository _notifRepo;
        private readonly IWalletRepository _walletRepo;
        public AppointmentController(
            IAppointmentRepository apptRepo,
            IWalletRepository walletRepo,
            IClinicRepository clinicRepo,
            IDoctorRepository doctorRepo,
            IClinicServiceRepository serviceRepo,
            IClinicScheduleRepository scheduleRepo,
            IPetRepository petRepo,
            INotificationRepository notifRepo)
        {
            _apptRepo = apptRepo;
            _walletRepo = walletRepo;
            _clinicRepo = clinicRepo;
            _doctorRepo = doctorRepo;
            _serviceRepo = serviceRepo;
            _scheduleRepo = scheduleRepo;
            _petRepo = petRepo;
            _notifRepo = notifRepo;
        }

        private int GetUserId() => HttpContext.Session.GetInt32("UserId") ?? 0;
        private bool IsLoggedIn() => GetUserId() > 0;

        // ══════════════════════════════════════════════════════════════
        // GET /Appointment/Book
        // ══════════════════════════════════════════════════════════════
        [HttpGet]
        public async Task<IActionResult> Book(int clinicId, int? doctorId = null)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");

            var clinic = await _clinicRepo.GetByIdAsync(clinicId);
            if (clinic == null) return NotFound();

            var services = (await _serviceRepo.GetByClinicIdAsync(clinicId))
                .Where(s => !string.IsNullOrEmpty(s.DocumentType))
                .Select(s => s.DocumentType!).Distinct().ToList();

            var doctors = (await _doctorRepo.GetByClinicIdAsync(clinicId))
                .Where(d => d.VerificationStatus == "Approved").ToList();

            var schedule = (await _scheduleRepo.GetByClinicIdAsync(clinicId)).ToList();
            var clinicWorkDays = schedule
                .Where(s => s.IsWorkingDay == true && s.DayOfWeek.HasValue)
                .Select(s => s.DayOfWeek!.Value).ToHashSet();

            var userId = GetUserId();
            var rawPets = (await _petRepo.GetByUserIdAsync(userId)).ToList();
            var petCards = new List<PetCardViewModel>();
            foreach (var pet in rawPets)
            {
                var imgs = (await _petRepo.GetImagesAsync(pet.IdPet)).ToList();
                var mainImg = imgs.FirstOrDefault(i => i.IsMain)?.ImageUrl
                           ?? imgs.FirstOrDefault()?.ImageUrl
                           ?? pet.ImageUrl;
                petCards.Add(new PetCardViewModel
                {
                    IdPet = pet.IdPet,
                    Name = pet.Name ?? "",
                    Species = pet.Species,
                    Breed = pet.Breed,
                    Gender = pet.Gender,
                    Color = pet.Color,
                    ImageUrl = mainImg,
                    AgeDisplay = pet.AgeDisplay,
                    VaccinationStatus = pet.VaccinationStatus,   // ← مطلوب للـ medical record
                    MedicalNotes = pet.MedicalNotes,
                    IsNeutered = pet.IsNeutered
                });

            }

            var vm = new BookAppointmentViewModel
            {
                ClinicId = clinicId,
                ClinicName = clinic.Name,
                Services = services,
                Doctors = doctors,
                UserPets = petCards,
                PreSelectedDoctorId = doctorId
            };

            ViewBag.ClinicWorkDaysJson = System.Text.Json.JsonSerializer.Serialize(clinicWorkDays);
            ViewBag.DoctorsJson = System.Text.Json.JsonSerializer.Serialize(
                doctors.Select(d => new {
                    id = d.IdDoctor,
                    name = d.FullName,
                    spec = d.Specialization ?? "",
                    isAvailable = d.IsAvailable ?? false
                }));

            // ★ بيانات الحيوانات للـ medical record preview في JS
            ViewBag.PetsJson = System.Text.Json.JsonSerializer.Serialize(
                petCards.Select(p => new {
                    id = p.IdPet,
                    name = p.Name,
                    species = p.Species,
                    age = p.AgeDisplay,
                    vaccinationStatus = p.VaccinationStatus,
                    isNeutered = p.IsNeutered,
                    medicalNotes = p.MedicalNotes
                }));

            if (TempData["BookError"] != null) ViewBag.Error = TempData["BookError"];
            if (TempData["BookSuccess"] != null) ViewBag.Success = TempData["BookSuccess"];
            // أضيفي هذا قبل return View(vm)
            var wallet = await _walletRepo.GetByUserIdAsync(userId);
            vm.WalletBalance = wallet?.Balance ?? 0;
            return View(vm);
        }

        // ══════════════════════════════════════════════════════════════
        // GET /Appointment/GetDoctorSchedule?doctorId=X
        // ══════════════════════════════════════════════════════════════
        [HttpGet]
        public async Task<IActionResult> GetDoctorSchedule(int doctorId)
        {
            var info = await _doctorRepo.GetFullInfoByDoctorIdAsync(doctorId);
            if (info == null) return Json(new { error = "Doctor not found" });

            return Json(new
            {
                workDays = ParseWorkDays(info.WorkingDays),
                startTime = info.StartTime?.ToString(@"hh\:mm") ?? "09:00",
                endTime = info.EndTime?.ToString(@"hh\:mm") ?? "17:00",
                duration = info.AppointmentDuration ?? 30,
                maxDaily = info.MaxDailyAppointments ?? 12
            });
        }

        // ══════════════════════════════════════════════════════════════
        // GET /Appointment/GetAvailableSlots?doctorId=X&date=yyyy-MM-dd
        // ══════════════════════════════════════════════════════════════
        [HttpGet]
        public async Task<IActionResult> GetAvailableSlots(int doctorId, string date)
        {
            if (!DateTime.TryParse(date, out var apptDate))
                return Json(new { error = "Invalid date" });

            var info = await _doctorRepo.GetFullInfoByDoctorIdAsync(doctorId);
            if (info == null || !info.StartTime.HasValue || !info.EndTime.HasValue)
                return Json(new { slots = Array.Empty<string>() });

            var duration = info.AppointmentDuration ?? 30;
            var maxDaily = info.MaxDailyAppointments ?? 12;

            var bookedCount = await _apptRepo.CountByDoctorAndDateAsync(doctorId, apptDate);
            if (bookedCount >= maxDaily)
                return Json(new { slots = Array.Empty<string>(), maxReached = true });

            var allSlots = GenerateSlots(info.StartTime.Value, info.EndTime.Value, duration);
            var bookedSlots = (await _apptRepo.GetBookedSlotsAsync(doctorId, apptDate)).ToHashSet();

            var now = DateTime.Now;
            var isToday = apptDate.Date == now.Date;
            var available = allSlots
                .Where(s => !bookedSlots.Contains(s))
                .Where(s => !isToday || s > now.TimeOfDay.Add(TimeSpan.FromMinutes(30)))
                .Select(s => s.ToString(@"hh\:mm"))
                .ToList();

            return Json(new { slots = available, duration });
        }

        // ══════════════════════════════════════════════════════════════
        // GET /Appointment/GetAnyDoctorSlots?clinicId=X&date=yyyy-MM-dd
        // ★ جديد — لـ "Any Available Doctor"
        // ══════════════════════════════════════════════════════════════
        [HttpGet]
        public async Task<IActionResult> GetAnyDoctorSlots(int clinicId, string date)
        {
            if (!DateTime.TryParse(date, out var apptDate))
                return Json(new { error = "Invalid date" });

            var doctors = (await _doctorRepo.GetByClinicIdAsync(clinicId))
                .Where(d => d.VerificationStatus == "Approved" && d.IsAvailable == true)
                .ToList();

            if (!doctors.Any())
                return Json(new { slots = Array.Empty<string>() });

            var now = DateTime.Now;
            var isToday = apptDate.Date == now.Date;
            var availableSlots = new HashSet<string>();

            foreach (var doc in doctors)
            {
                var info = await _doctorRepo.GetFullInfoByDoctorIdAsync(doc.IdDoctor);
                if (info == null || !info.StartTime.HasValue || !info.EndTime.HasValue) continue;

                var workDays = ParseWorkDays(info.WorkingDays);
                if (!workDays.Contains((int)apptDate.DayOfWeek)) continue;

                var duration = info.AppointmentDuration ?? 30;
                var maxDaily = info.MaxDailyAppointments ?? 12;
                var booked = await _apptRepo.CountByDoctorAndDateAsync(doc.IdDoctor, apptDate);
                if (booked >= maxDaily) continue;

                var allSlots = GenerateSlots(info.StartTime.Value, info.EndTime.Value, duration);
                var bookedSlots = (await _apptRepo.GetBookedSlotsAsync(doc.IdDoctor, apptDate)).ToHashSet();

                foreach (var slot in allSlots)
                {
                    if (isToday && slot <= now.TimeOfDay.Add(TimeSpan.FromMinutes(30))) continue;
                    if (bookedSlots.Contains(slot)) continue;
                    availableSlots.Add(slot.ToString(@"hh\:mm"));
                }
            }

            var sorted = availableSlots.OrderBy(s => s).ToList();
            return Json(new { slots = sorted, duration = 30 });
        }

        // ══════════════════════════════════════════════════════════════
        // POST /Appointment/Book
        // ══════════════════════════════════════════════════════════════
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Book(BookAppointmentViewModel vm)
        {
            var userId = GetUserId();
            if (userId == 0) return RedirectToAction("Login", "Account");

            // ── 1. Status حسب اختيار الطبيب ──────────────────────────
            // CliQ دائماً يبدأ بـ PendingPayment بغض النظر عن الطبيب
            var status = vm.DepositPayMethod == "CliQ"
                ? "PendingPayment"
                : (vm.SelectedDoctorId > 0 && vm.SelectedDoctorId != -1)
                    ? "PendingDoctorApproval"
                    : "PendingClinicApproval";

            // ── 2. بناء الـ Appointment ───────────────────────────────
            var appt = new Appointment
            {
                IdUser = userId,
                IdClinic = vm.ClinicId,
                IdDoctor = (vm.SelectedDoctorId > 0 && vm.SelectedDoctorId != -1)
                           ? vm.SelectedDoctorId : (int?)null,
                IdPet = vm.SelectedPetId > 0 ? vm.SelectedPetId : null,
                AppointmentDate = DateTime.Parse(vm.SelectedDate!),
                StartTime = TimeSpan.Parse(vm.SelectedTime!),
                ServiceName = vm.SelectedService,
                Status = status,
                UrgencyLevel = vm.UrgencyLevel,
                Reason = vm.Reason,
                Notes = vm.Notes,
                ShareMedicalRecord = vm.ShareMedicalRecord,
                DepositAmount = 5.00m,
                DepositPayMethod = vm.DepositPayMethod,   // ← مرة واحدة فقط
                CliqReference = GenerateCliqRef(),
                AppointmentType = null,
                ConsultationFee = 0,
                PaymentMethod = null,
                PaymentStatus = null,
                CreatedAt = DateTime.UtcNow
            };
            var apptId = await _apptRepo.BookAsync(appt);

            // ── 3. الإيداع ────────────────────────────────────────────
            if (vm.DepositPayMethod == "Wallet")
            {
                var wallet = await _walletRepo.GetByUserIdAsync(userId);

                if (wallet == null || wallet.Balance < 5)
                {
                    await _apptRepo.CancelAsync(apptId, "Insufficient wallet balance");
                    TempData["BookError"] = "⚠️ Insufficient wallet balance. Please top up or choose CliQ.";
                    return RedirectToAction("Book", new { clinicId = vm.ClinicId });
                }

                await _walletRepo.AddBalanceAsync(wallet.IdWallet, -5);
                await _walletRepo.AddTransactionAsync(new WalletTransaction
                {
                    IdWallet = wallet.IdWallet,
                    TransactionCode = $"DEP-{Guid.NewGuid().ToString()[..8].ToUpper()}",
                    Type = "AppointmentDeposit",
                    Amount = -5,
                    PaymentMethod = "Wallet",
                    Status = "Completed",
                    CreatedAt = DateTime.UtcNow
                });
                await _apptRepo.MarkDepositPaidAsync(apptId, "Wallet");
                // +5 للعيادة عند الدفع
                await _clinicRepo.AddClinicBalanceAsync(vm.ClinicId, 5.00m);

            }
            // CliQ: DepositPaid يبقى false حتى يؤكد الادمن

            // ── 4. إشعار ─────────────────────────────────────────────
            var message = status == "PendingDoctorApproval"
                ? "Your appointment request has been sent to the doctor for approval. 🐾"
                : "Your appointment request has been received. The clinic will assign a doctor shortly. 🐾";

            await _notifRepo.AddAsync(new Notification
            {
                IdUser = userId,
                Type = "AppointmentPending",
                Title = "⏳ Appointment Request Submitted",
                Message = message,
                IsRead = false,
                RelatedEntityType = "Appointment",
                RelatedEntityId = apptId,
                CreatedAt = DateTime.UtcNow
            });

            TempData["BookSuccess"] = vm.DepositPayMethod == "CliQ"
     ? "⏳ Booking submitted! Please complete your 5 JOD CliQ payment — see instructions below."
     : "✅ Booking request submitted! You'll be notified once the appointment is confirmed. 🐾";
            return RedirectToAction("Appointments", "Profile");
        }

        // ══════════════════════════════════════════════════════════════
        // POST /Appointment/Cancel
        // ══════════════════════════════════════════════════════════════
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id, string? reason = null)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");

            var appt = await _apptRepo.GetByIdAsync(id);
            if (appt == null || appt.IdUser != GetUserId()) return Forbid();

            // PendingPayment و PendingPaymentVerification: لا مال مدفوع → إلغاء حر
            // Confirmed + CliQ: ممنوع الإلغاء المباشر
            if (appt.Status == "Confirmed"
                && appt.DepositPayMethod == "CliQ"
                && appt.DepositPaid)
            {
                TempData["BookError"] =
                    "⚠️ Your CliQ deposit was already paid. " +
                    "To cancel, please contact the clinic directly for a refund.";
                return RedirectToAction("Appointments");
            }

            var cancellable = new[]
            {
    "PendingPayment",             // CliQ لم يدفع بعد
    "PendingPaymentVerification", // CliQ انتظر تأكيد
    "PendingDoctorApproval",
    "PendingClinicApproval",
    "Confirmed"                   // Wallet فقط يصل هنا
};
            if (!cancellable.Contains(appt.Status))
            {
                TempData["BookError"] = "⚠️ This appointment cannot be cancelled.";
                return RedirectToAction("Appointments", "Profile");
            }

            await _apptRepo.CancelAsync(id, reason ?? "Cancelled by user");

            // ── إرجاع الإيداع ─────────────────────────────────────
            // Wallet: DepositPaid=true فوراً عند الحجز
            // CliQ مؤكد: DepositPaid=true بعد تأكيد الادمن
            // CliQ غير مؤكد: DepositPaid=false → لا يوجد مبلغ مدفوع أصلاً
            var depositReturned = false;

            if (appt.DepositPaid && !appt.DepositReturned)
            {
                // تأكد من وجود المحفظة أو إنشاؤها
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

            TempData["BookSuccess"] = depositReturned
                ? $"✅ Appointment cancelled. Your {appt.DepositAmount:F2} JOD deposit has been returned to your wallet."
                : "✅ Appointment cancelled.";

            return RedirectToAction("Appointments", "Profile");
        }
        // ══════════════════════════════════════════════════════════════
        // GET /Appointment/MyAppointments (kept for backward compat)
        // ══════════════════════════════════════════════════════════════
        [HttpGet]
        public IActionResult MyAppointments()
            => RedirectToAction("Appointments", "Profile");

        // ── Private helpers ───────────────────────────────────────────
        private static List<TimeSpan> GenerateSlots(TimeSpan start, TimeSpan end, int duration)
        {
            var slots = new List<TimeSpan>();
            var current = start;
            while (current.Add(TimeSpan.FromMinutes(duration)) <= end)
            {
                slots.Add(current);
                current = current.Add(TimeSpan.FromMinutes(duration));
            }
            return slots;
        }

        private static string GenerateCliqRef()
        {
            // مثال: WF-X4K9M2PQ (عشوائي تماماً، غير متسلسل)
            var chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            var rnd = new Random();
            var code = new string(Enumerable.Range(0, 8)
                            .Select(_ => chars[rnd.Next(chars.Length)])
                            .ToArray());
            return $"WF-{code}";
        }
        private static List<int> ParseWorkDays(string? workDays)
        {
            if (string.IsNullOrEmpty(workDays)) return new List<int>();
            return workDays.Split(',')
                .Where(d => int.TryParse(d.Trim(), out _))
                .Select(d => int.Parse(d.Trim()))
                .ToList();
        }

        [HttpGet]
        public async Task<IActionResult> Confirmation(int id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");

            var appt = await _apptRepo.GetByIdAsync(id);
            if (appt == null || appt.IdUser != GetUserId())
                return RedirectToAction("Appointments", "Profile");

            return View(appt);
        }
    }
}