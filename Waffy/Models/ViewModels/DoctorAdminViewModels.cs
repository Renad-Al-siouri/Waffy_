using Waffy.Models;

namespace Waffy.Models.ViewModels
{
    // ── DTO شامل يُجلب مرة واحدة ──────────────────────
    public class DoctorFullDto
    {
        public int IdDoctor { get; set; }
        public int? IdClinic { get; set; }
        // Doctor fields
        public string? Specialization { get; set; }
        public string? SecondarySpec { get; set; }
        public int? ExperienceYears { get; set; }
        public string? Qualification { get; set; }
        public string? Certifications { get; set; }
        public string? Bio { get; set; }
        public string? Languages { get; set; }
        public string? WorkingDays { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public int? AppointmentDuration { get; set; }
        public int? MaxDailyAppointments { get; set; }
        public bool? IsAvailable { get; set; }
        public string? VerificationStatus { get; set; }
        public decimal? Rating { get; set; }
        // User fields
        public string FullName { get; set; } = "";
        public string? Email { get; set; }
        public string? Phone { get; set; }
        // Clinic name
        public string? ClinicName { get; set; }

        public string? ProfileImageUrl { get; set; }
    }

    // ── ViewModels للصفحات ─────────────────────────────
    public class DoctorPersonalInfoViewModel
    {
        public string FullName { get; set; } = "";
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Bio { get; set; }
        public IFormFile? ProfileImage { get; set; }

        public string? ProfileImageUrl { get; set; }
        // Display
        public string? Specialization { get; set; }
        public string? VerificationStatus { get; set; }
        public decimal? Rating { get; set; }
        public string? ClinicName { get; set; }
    }

    public class DoctorProfessionalInfoViewModel
    {
        public string? Specialization { get; set; }
        public string? SecondarySpec { get; set; }
        public int? ExperienceYears { get; set; }
        public string? Qualification { get; set; }
        public string? Certifications { get; set; }   // ← النص
        public string? Languages { get; set; }
        public List<DoctorDocument> CertificationFiles { get; set; } = new(); // ← الملفات
        // Display
        public string FullName { get; set; } = "";
        public string? VerificationStatus { get; set; }
        public decimal? Rating { get; set; }
        public string? ClinicName { get; set; }
    }

    public class DoctorWorkScheduleViewModel
    {
        public bool Sunday { get; set; }
        public bool Monday { get; set; }
        public bool Tuesday { get; set; }
        public bool Wednesday { get; set; }
        public bool Thursday { get; set; }
        public bool Friday { get; set; }
        public bool Saturday { get; set; }
        public string StartTime { get; set; } = "09:00";
        public string EndTime { get; set; } = "17:00";
        public int AppointmentDuration { get; set; } = 30;
        public int MaxDailyAppointments { get; set; } = 12;
        public bool IsAvailable { get; set; }
        // Display
        public string FullName { get; set; } = "";
        public string? VerificationStatus { get; set; }
        public decimal? Rating { get; set; }
        public string? ClinicName { get; set; }
    }

    public class DoctorDocumentsViewModel
    {
        public List<DoctorDocument> Documents { get; set; } = new();
        public string? CertificationUrl { get; set; }
        // Display
        public string FullName { get; set; } = "";
        public string? Specialization { get; set; }
        public string? VerificationStatus { get; set; }
        public decimal? Rating { get; set; }
        public string? ClinicName { get; set; }
        public int IdDoctor { get; set; }
    }

    public class DoctorPasswordViewModel
    {
        public string CurrentPassword { get; set; } = "";
        public string NewPassword { get; set; } = "";
        public string ConfirmNewPassword { get; set; } = "";
    }

    public class DoctorDashboardViewModel
    {
        public string FullName { get; set; } = "";
        public string? Specialization { get; set; }
        public string? ClinicName { get; set; }
        public decimal? Rating { get; set; }
        public string? VerificationStatus { get; set; }
        public bool? IsAvailable { get; set; }
    }
}