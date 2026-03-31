namespace Waffy.Models
{
    using System;

    namespace Waffy.Models
    {
        public class Clinic
        {
            public int Id { get; set; }
            public int OwnerUserId { get; set; }
            public string Name { get; set; } = null!;
            public string? ClinicType { get; set; }
            public string Phone { get; set; } = null!;
            public string? Email { get; set; }
            public string? Description { get; set; }
            public string City { get; set; } = null!;
            public string? Area { get; set; }
            public string? Address { get; set; }
            public decimal? Latitude { get; set; }
            public decimal? Longitude { get; set; }
            public TimeSpan? DefaultOpenTime { get; set; }
            public TimeSpan? DefaultCloseTime { get; set; }
            public bool HasEmergency { get; set; }
            public string VerificationStatus { get; set; } = "Pending";
            public decimal Rating { get; set; }
            public string? LogoUrl { get; set; }
            public string? CoverImageUrl { get; set; }
            public DateTime CreatedAt { get; set; }
        }
    }
}
