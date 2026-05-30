namespace Waffy.Models
{
    public class Doctor
    {
        public int IdDoctor { get; set; }
        public int? IdUser { get; set; }
        public int? IdClinic { get; set; }
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
        public DateTime? CreatedAt { get; set; }
    }
}