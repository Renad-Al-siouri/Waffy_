using Waffy.Models;
using Waffy.Models.ViewModels;

namespace Waffy.Models.ViewModels
{
    // ── Returned from queries ──────────────────────────────────────────
    public class AppointmentDto
    {
        public int Id { get; set; }
        public int IdUser { get; set; }
        public int IdDoctor { get; set; }
        public int IdClinic { get; set; }
        public int? IdPet { get; set; }
        public DateTime AppointmentDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public string? ServiceName { get; set; }
        public string Status { get; set; } = "";
        public string? UrgencyLevel { get; set; }
        public string? Reason { get; set; }
        public string? Notes { get; set; }
        public string? CancellationReason { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? AppointmentDuration { get; set; }
        public string? DoctorNotes { get; set; }

        // Joined
        public string DoctorName { get; set; } = "";
        public string? DoctorSpecialization { get; set; }
        public string ClinicName { get; set; } = "";
        public string? UserFullName { get; set; }
        public string? UserPhone { get; set; }
        public string? PetName { get; set; }
        public string? PetSpecies { get; set; }
        public string? PetImageUrl { get; set; }
        public string? MedicalRecordNote { get; set; }
        
        public bool ShareMedicalRecord { get; set; }  // إذن اليوزر

        public decimal DepositAmount { get; set; } = 5.00m;
        public bool DepositPaid { get; set; }
        public bool DepositReturned { get; set; }
        public string? DepositPayMethod { get; set; }
        // في AppointmentDto أضيفي بعد DepositPayMethod:
        public string? CliqReference { get; set; }
        public bool DepositCliqSubmitted { get; set; }
        public string? ClinicCliqNumber { get; set; }
        public bool AttendanceConfirmed { get; set; }
        public bool AssignedByClinic { get; set; }
        // في AppointmentDto أضيفي بعد DepositPayMethod:
        
        // Helpers
        public string StatusCss => Status.ToLower() switch
        {
            "confirmed" => "s-confirmed",
            "cancelled" => "s-cancelled",
            "completed" => "s-new",
            "noshow" => "s-pending",
            _ => "s-pending"
        };
        public string StatusEmoji => Status.ToLower() switch
        {
            "confirmed" => "✅",
            "cancelled" => "❌",
            "completed" => "✔️",
            "noshow" => "⚠️",
            _ => "⏳"
        };
        public string TimeDisplay =>
            $"{StartTime:hh\\:mm}" +
            (EndTime.HasValue ? $" – {EndTime.Value:hh\\:mm}" : "");
    }

    // ── For the booking wizard page ────────────────────────────────────
    public class BookAppointmentViewModel
    {
        // Context (from URL)
        public int ClinicId { get; set; }
        public string ClinicName { get; set; } = "";
        public int? PreSelectedDoctorId { get; set; }

        // Populate from DB (for wizard display)
        public List<string> Services { get; set; } = new();
        public List<DoctorListItemViewModel> Doctors { get; set; } = new();
        public List<PetCardViewModel> UserPets { get; set; } = new();

        // Form POST fields
        public string? SelectedService { get; set; }
        public int SelectedDoctorId { get; set; }
        public string? SelectedDate { get; set; }   // "yyyy-MM-dd"
        public string? SelectedTime { get; set; }   // "HH:mm"
        public int SelectedPetId { get; set; }      // 0 = no pet
        public string? Reason { get; set; }
        public string? Notes { get; set; }
        public string UrgencyLevel { get; set; } = "Normal";
        public bool ShareMedicalRecord { get; set; }
        // أضيفي بعد: public bool ShareMedicalRecord { get; set; }
        public decimal WalletBalance { get; set; }   // رصيد المحفظة
        public string DepositPayMethod { get; set; } = ""; // "Wallet" أو "CliQ"
    }

    // ── Pet card for booking ───────────────────────────────────────────
    public class PetCardViewModel
    {
        public int? IdPet { get; set; }
        public string Name { get; set; } = "";
        public string? Species { get; set; }
        public string? Breed { get; set; }
        public string? Gender { get; set; }
        public string? Color { get; set; }
        public string? ImageUrl { get; set; }
        public string? AgeDisplay { get; set; }
        public string? VaccinationStatus { get; set; }   // ← هنا
        public bool IsNeutered { get; set; }   // ← هنا
        public string? MedicalNotes { get; set; }   // ← هنا
        public bool ShareMedicalRecord { get; set; }
    }

    // ── Doctor Appointments page (DoctorAdmin) ─────────────────────────
    public class DoctorAppointmentsViewModel
    {
        public List<AppointmentDto> Appointments { get; set; } = new();
        public List<AppointmentDto> TodayAppointments { get; set; } = new();
        public string StatusFilter { get; set; } = "All";
        public int TotalCount { get; set; }
        public int TodayCount { get; set; }
        public int ConfirmedCount { get; set; }
        public int CompletedCount { get; set; }
        public int CancelledCount { get; set; }
        // Display
        public string FullName { get; set; } = "";
        public string? ClinicName { get; set; }
        public string? VerificationStatus { get; set; }
        public decimal? Rating { get; set; }
        public bool ShareMedicalRecord { get; set; }
        public int PendingCount { get; set; }
    }

    // ── Clinic Appointments page (ClinicAdmin) ─────────────────────────
    public class ClinicAppointmentsViewModel
    {
        public List<AppointmentDto> Appointments { get; set; } = new();
        public string StatusFilter { get; set; } = "All";
        public string? DoctorFilter { get; set; }
        public int TotalCount { get; set; }
        public int TodayCount { get; set; }
        public int ConfirmedCount { get; set; }
        public int CompletedCount { get; set; }
        public int CancelledCount { get; set; }
        public List<string> DoctorNames { get; set; } = new();
        public List<ClinicDoctorSlimDto> AvailableDoctors { get; set; } = new();
        public Dictionary<int, int> DoctorAppointmentCounts { get; set; } = new();
        public bool ShareMedicalRecord { get; set; }
        public List<AppointmentDto> TodayAppointments { get; set; } = new();
    }


    // أضيفيه في AppointmentViewModels.cs
    public class AppointmentWithPetDto
    {
        // كل حقول AppointmentDto
        public int Id { get; set; }
        public int IdClinic { get; set; }
        public int IdDoctor { get; set; }
        public int? IdPet { get; set; }
        public string ClinicName { get; set; } = "";
        public string DoctorName { get; set; } = "";
        public string? DoctorSpecialization { get; set; }
        public string? ServiceName { get; set; }
        public string? PetName { get; set; }
        public string? PetSpecies { get; set; }
        public string? PetImageUrl { get; set; }   // ← جديد
        public DateTime AppointmentDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public string Status { get; set; } = "";
        public string? Reason { get; set; }
        public string? Notes { get; set; }
        public string? CancellationReason { get; set; }
        public string? UrgencyLevel { get; set; }
        public string? MedicalRecordNote { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool ShareMedicalRecord { get; set; }
        // Helpers
        public string StatusCss => Status?.ToLower() switch
        {
            "confirmed" => "s-confirmed",
            "completed" => "s-completed",
            "cancelled" => "s-cancelled",
            _ => "s-pending"
        };
        public string StatusEmoji => Status?.ToLower() switch
        {
            "confirmed" => "✅",
            "completed" => "✔️",
            "cancelled" => "❌",
            _ => "⏳"
        };
    }

    // ViewModel لصفحة Profile/Appointments
    public class ProfileAppointmentsViewModel
    {
        // ── Sidebar ──────────────────────────────────────────
        public string UserFullName { get; set; } = "";
        public string UserEmail { get; set; } = "";
        public string? UserProfileImage { get; set; }
        public decimal Balance { get; set; }
        public bool ShareMedicalRecord { get; set; }

        // ── Appointments ─────────────────────────────────────
        public List<AppointmentDto> Appointments { get; set; } = new();

        // ── Computed ─────────────────────────────────────────
        public int UpcomingCount => Appointments.Count(a =>
     a.AppointmentDate >= DateTime.Today &&
     a.Status is "Confirmed" or "PendingDoctorApproval" or "PendingClinicApproval");
        public int CompletedCount => Appointments.Count(a => a.Status == "Completed");
        public int CancelledCount => Appointments.Count(a => a.Status == "Cancelled");
    }

    public class ClinicDoctorSlimDto
    {
        public int IdDoctor { get; set; }
        public string FullName { get; set; } = "";
        public string? Specialization { get; set; }
        public string? WorkingDays { get; set; }
        public int? MaxDailyAppointments { get; set; }
        public bool? IsAvailable { get; set; }
    }

    public class SuperAdminAppointmentsViewModel
    {
        public List<AppointmentDto> Appointments { get; set; } = new();
        public List<Clinic> AvailableClinics { get; set; } = new();

        // KPIs
        public int TotalCount { get; set; }
        public int PendingCount { get; set; }
        public int ConfirmedCount { get; set; }
        public int CompletedCount { get; set; }
        public int CancelledCount { get; set; }
        public int NoShowCount { get; set; }

        // Deposits
        public decimal TotalDepositsCollected { get; set; }
        public decimal TotalDepositsReturned { get; set; }
        public decimal TotalDepositsRetained { get; set; }
    }
}