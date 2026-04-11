namespace Waffy.Models
{
    public class Clinic
    {
        public int IdClinic { get; set; }
        public int OwnerUserId { get; set; }
        public string Name { get; set; }
        public string ClinicType { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string? Description { get; set; }
        public string City { get; set; }
        public string Area { get; set; }
        public string Address { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public TimeSpan? DefaultOpenTime { get; set; }
        public TimeSpan? DefaultCloseTime { get; set; }
        public bool HasEmergency { get; set; }
        public string VerificationStatus { get; set; } = "Pending";
        public decimal Rating { get; set; } = 0.00m; // ✅ قيمة افتراضية
        public string? LogoUrl { get; set; }
        public string? CoverImageUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}