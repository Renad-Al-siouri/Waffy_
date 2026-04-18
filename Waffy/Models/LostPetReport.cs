namespace Waffy.Models
{
    public class LostPetReport
    {
        public int IdLostReport { get; set; }
        public int IdUser { get; set; }
        public int? IdPet { get; set; }

        // Pet info (snapshot at time of report)
        public string? PetName { get; set; }
        public string? AnimalType { get; set; }
        public string? Breed { get; set; }
        public string? Color { get; set; }

        // Location
        public string? LastSeenLocation { get; set; }
        public string? City { get; set; }
        public string? Area { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        // Time
        public DateTime? DateLost { get; set; }
        public TimeSpan? TimeLost { get; set; }

        // Details
        public string? Description { get; set; }
        public string? SpecialMarks { get; set; }
        public string? MicrochipId { get; set; }

        // Security & Reward
        public string? SecretQuestion { get; set; }
        public string? SecretAnswerHash { get; set; }
        public decimal? RewardAmount { get; set; }
        public string? RewardStatus { get; set; }    // "Reserved" | "Released" | "None"

        // Status
        public string? Status { get; set; }           // "Active" | "Found" | "Closed"
        public DateTime? ExpiryDate { get; set; }
        public string? ReportCode { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    }
}
