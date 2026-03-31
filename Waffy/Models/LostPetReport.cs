namespace Waffy.Models
{
    using System;

    namespace Waffy.Models
    {
        public class LostPetReport
        {
            public int Id { get; set; }
            public int UserId { get; set; }
            public int? PetId { get; set; }
            public string? PetName { get; set; }
            public string? AnimalType { get; set; }
            public string? Breed { get; set; }
            public string? Color { get; set; }
            public string LastSeenLocation { get; set; } = null!;
            public string City { get; set; } = null!;
            public string? Area { get; set; }
            public decimal? Latitude { get; set; }
            public decimal? Longitude { get; set; }
            public DateTime DateLost { get; set; }
            public TimeSpan? TimeLost { get; set; }
            public string? Description { get; set; }
            public string? SpecialMarks { get; set; }
            public string? MicrochipId { get; set; }
            public string SecretQuestion { get; set; } = null!;
            public string SecretAnswerHash { get; set; } = null!;
            public decimal? RewardAmount { get; set; }
            public string? RewardStatus { get; set; }
            public string Status { get; set; } = "Active";
            public DateTime? ExpiryDate { get; set; }
            public string? ReportCode { get; set; }
            public DateTime CreatedAt { get; set; }
        }
    }
}
