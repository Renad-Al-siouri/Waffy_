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
            var paymentSettings = await _walletRepo.GetPaymentSettingsAsync();
            ViewBag.SuperAdminCliqNumber = paymentSettings?.SuperAdminCliqNumber ?? "Not set";
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
        // ══════════════════════════════════════════════════════════════
        // POST /Appointment/Book
        // ══════════════════════════════════════════════════════════════
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Book(BookAppointmentViewModel vm)
        {
            var userId = GetUserId();
            if (userId == 0) return RedirectToAction("Login", "Account");

            // ── Basic validation ─────────────────────────────────────
            if (vm.ClinicId <= 0 ||
                string.IsNullOrWhiteSpace(vm.SelectedService) ||
                string.IsNullOrWhiteSpace(vm.SelectedDate) ||
                string.IsNullOrWhiteSpace(vm.SelectedTime) ||
                string.IsNullOrWhiteSpace(vm.DepositPayMethod))
            {
                TempData["BookError"] = "⚠️ Please complete all booking steps.";
                return RedirectToAction("Book", new { clinicId = vm.ClinicId });
            }

            if (!DateTime.TryParse(vm.SelectedDate, out var appointmentDate) ||
                !TimeSpan.TryParse(vm.SelectedTime, out var startTime))
            {
                TempData["BookError"] = "⚠️ Invalid appointment date or time.";
                return RedirectToAction("Book", new { clinicId = vm.ClinicId });
            }

            if (appointmentDate.Date < DateTime.Today)
            {
                TempData["BookError"] = "⚠️ You cannot book an appointment in the past.";
                return RedirectToAction("Book", new { clinicId = vm.ClinicId });
            }

            var selectedDoctorId =
                vm.SelectedDoctorId > 0 && vm.SelectedDoctorId != -1
                    ? vm.SelectedDoctorId
                    : (int?)null;

            // ── If user selected a specific doctor, re-check slot on server ──
            if (selectedDoctorId.HasValue)
            {
                var slotFree = await _apptRepo.IsSlotAvailableAsync(
                    selectedDoctorId.Value,
                    appointmentDate,
                    startTime);

                if (!slotFree)
                {
                    TempData["BookError"] = "⚠️ This time slot is no longer available. Please choose another time.";
                    return RedirectToAction("Book", new { clinicId = vm.ClinicId, doctorId = selectedDoctorId.Value });
                }

               
                // ملاحظة: هذا الفحص التفصيلي سيتم تفعيله أكثر عند تعيين الطبيب من العيادة.
                // الآن نعتمد على GetAvailableSlots لأنه فحص وقت الطبيب قبل العرض.
            }

            const decimal depositAmount = 5.00m;

            string? cliqReference = null;

            if (vm.DepositPayMethod == "CliQ")
            {
                cliqReference = string.IsNullOrWhiteSpace(vm.CliqReference)
                    ? GenerateCliqRef()
                    : vm.CliqReference.Trim();
            }

            // ── Status حسب الدفع واختيار الطبيب ─────────────────────
            // Wallet: الدفع يتم فوراً، فيتحرك الموعد مباشرة للعيادة/الطبيب
            // CliQ: يبقى بانتظار تأكيد السوبر أدمن
            var status = vm.DepositPayMethod == "CliQ"
                ? "PendingPaymentVerification"
                : selectedDoctorId.HasValue
                    ? "PendingDoctorApproval"
                    : "PendingClinicAssignment";

            // ── Build Appointment ───────────────────────────────────
            var appt = new Appointment
            {
                IdUser = userId,
                IdClinic = vm.ClinicId,
                IdDoctor = selectedDoctorId,
                IdPet = vm.SelectedPetId > 0 ? vm.SelectedPetId : null,

                AppointmentDate = appointmentDate.Date,
                StartTime = startTime,

                ServiceName = vm.SelectedService,
                Status = status,
                UrgencyLevel = vm.UrgencyLevel,
                Reason = vm.Reason,
                Notes = vm.Notes,
                ShareMedicalRecord = vm.ShareMedicalRecord,

                DepositAmount = depositAmount,
                DepositPaid = false,
                DepositReturned = false,
                AttendanceConfirmed = false,
                AssignedByClinic = false,

                DepositPayMethod = vm.DepositPayMethod,
                CliqReference = cliqReference,
                DepositCliqSubmitted = vm.DepositPayMethod == "CliQ",

                AppointmentType = null,
                ConsultationFee = 0,
                PaymentMethod = null,
                PaymentStatus = null,
                CreatedAt = DateTime.UtcNow
            };

            var apptId = await _apptRepo.BookAsync(appt);

            // ── Wallet Payment: user wallet → Super Admin wallet ─────
            if (vm.DepositPayMethod == "Wallet")
            {
                var userWallet = await _walletRepo.GetOrCreateByUserIdAsync(userId);
                var superAdminWallet = await _walletRepo.GetSuperAdminWalletAsync();

                if (userWallet == null || superAdminWallet == null)
                {
                    await _apptRepo.CancelWithMetadataAsync(
                        apptId,
                        "Wallet setup error",
                        "System");

                    TempData["BookError"] = "⚠️ Wallet setup error. Please contact support.";
                    return RedirectToAction("Book", new { clinicId = vm.ClinicId });
                }

                if (userWallet.Balance < depositAmount)
                {
                    await _apptRepo.CancelWithMetadataAsync(
                        apptId,
                        "Insufficient wallet balance",
                        "System");

                    TempData["BookError"] = "⚠️ Insufficient wallet balance. Please top up or choose CliQ.";
                    return RedirectToAction("Book", new { clinicId = vm.ClinicId });
                }

                var transferred = await _walletRepo.TransferAsync(
                    fromWalletId: userWallet.IdWallet,
                    toWalletId: superAdminWallet.IdWallet,
                    amount: depositAmount,
                    type: "AppointmentDeposit",
                    paymentMethod: "Wallet",
                    referenceId: apptId,
                    referenceType: "Appointment",
                    description: "Booking deposit transferred to Waffy Super Admin wallet");

                if (!transferred)
                {
                    await _apptRepo.CancelWithMetadataAsync(
                        apptId,
                        "Wallet transfer failed",
                        "System");

                    TempData["BookError"] = "⚠️ Could not complete wallet payment.";
                    return RedirectToAction("Book", new { clinicId = vm.ClinicId });
                }

                await _apptRepo.MarkDepositPaidBySuperAdminAsync(
                    appointmentId: apptId,
                    payMethod: "Wallet",
                    adminId: null,
                    holderWalletId: superAdminWallet.IdWallet,
                    note: "Paid automatically from user wallet");
            }

            // ── Notification ────────────────────────────────────────
            string message;

            if (vm.DepositPayMethod == "CliQ")
            {
                message =
                    $"Your booking request was submitted. Please pay {depositAmount:F2} JOD via CliQ using reference code {cliqReference}. " +
                    "Waffy Super Admin will verify your payment.";
            }
            else if (selectedDoctorId.HasValue)
            {
                message =
                    "Your booking deposit was paid successfully. Your appointment request has been sent to the doctor for approval. 🐾";
            }
            else
            {
                message =
                    "Your booking deposit was paid successfully. The clinic will assign an available doctor shortly. 🐾";
            }

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
                ? $"⏳ Booking submitted! Please complete your 5 JOD CliQ payment. Reference code: {cliqReference}"
                : "✅ Booking request submitted! Your deposit was sent to Waffy and will be returned according to the policy.";

            return RedirectToAction("Appointments", "Profile");
        }
        // ══════════════════════════════════════════════════════════════
        // POST /Appointment/Cancel
        // ══════════════════════════════════════════════════════════════
        // ══════════════════════════════════════════════════════════════
        // POST /Appointment/Cancel
        // User cancels appointment
        // ══════════════════════════════════════════════════════════════
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id, string? reason = null)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");

            var userId = GetUserId();
            var appt = await _apptRepo.GetByIdAsync(id);

            if (appt == null || appt.IdUser != userId)
                return Forbid();

            // الحالات التي لا يمكن إلغاؤها
            if (appt.Status is "Completed" or "Cancelled" or "NoShow" or "Rejected")
            {
                TempData["BookError"] = "⚠️ This appointment cannot be cancelled.";
                return RedirectToAction("Appointments", "Profile");
            }

            // شرط 24 ساعة
            var canCancel = await _apptRepo.IsAppointmentCancellableBefore24hAsync(id);

            if (!canCancel)
            {
                TempData["BookError"] =
                    "⚠️ You cannot cancel this appointment because less than 24 hours remain.";
                return RedirectToAction("Appointments", "Profile");
            }

            // إلغاء الموعد
            await _apptRepo.CancelWithMetadataAsync(
                appointmentId: id,
                reason: reason ?? "Cancelled by user",
                cancelledBy: "User");

            var depositReturned = false;

            // إذا الدفع مؤكد، نرجع الإيداع من محفظة السوبر أدمن
            if (appt.DepositPaid && !appt.DepositReturned)
            {
                var refunded = await _walletRepo.RefundFromSuperAdminToUserAsync(
                    userId: appt.IdUser,
                    amount: appt.DepositAmount,
                    appointmentId: appt.Id,
                    adminId: null,
                    description: "Deposit returned after user cancelled appointment before 24 hours");

                if (!refunded)
                {
                    TempData["BookError"] =
                        "⚠️ Appointment cancelled, but refund failed because Super Admin wallet balance is insufficient. Please contact support.";
                    return RedirectToAction("Appointments", "Profile");
                }

                await _apptRepo.MarkDepositReturnedBySuperAdminAsync(
                    appointmentId: id,
                    adminId: null);

                depositReturned = true;
            }

            await _notifRepo.AddAsync(new Notification
            {
                IdUser = appt.IdUser,
                Type = "AppointmentCancelled",
                Title = "❌ Appointment Cancelled",
                Message = depositReturned
                    ? $"Your appointment was cancelled. Your {appt.DepositAmount:F2} JOD deposit has been returned to your wallet."
                    : "Your appointment was cancelled.",
                IsRead = false,
                RelatedEntityType = "Appointment",
                RelatedEntityId = id,
                CreatedAt = DateTime.UtcNow
            });

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

        private async Task<bool> TryReturnDepositAsync(AppointmentDto appt, int? adminId, string reason)
        {
            if (!appt.DepositPaid || appt.DepositReturned)
                return true;

            var refunded = await _walletRepo.RefundFromSuperAdminToUserAsync(
                appt.IdUser,
                appt.DepositAmount,
                appt.Id,
                adminId,
                reason);

            if (!refunded)
                return false;

            await _apptRepo.MarkDepositReturnedBySuperAdminAsync(appt.Id, adminId);
            return true;
        }
    }

}