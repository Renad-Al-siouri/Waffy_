namespace Waffy.Models
{
    using System;

    namespace Waffy.Models
    {
        public class FoundPetClaim
        {
            public int Id { get; set; }
            public int LostReportId { get; set; }
            public int FinderId { get; set; }
            public string? FoundLocation { get; set; }
            public string? City { get; set; }
            public string? Area { get; set; }
            public decimal? Latitude { get; set; }
            public decimal? Longitude { get; set; }
            public DateTime FoundDate { get; set; }
            public TimeSpan? FoundTime { get; set; }
            public string? Description { get; set; }
            public string? AnimalType { get; set; }
            public string? Color { get; set; }
            public string? SpecialMarks { get; set; }
            public decimal? MatchScore { get; set; }
            public int AttemptsCount { get; set; }
            public string? FinderCode { get; set; }
            public string? OwnerCode { get; set; }
            public bool CodesExchanged { get; set; }
            public string Status { get; set; } = "Pending";
            public DateTime? ExpiresAt { get; set; }
            public DateTime CreatedAt { get; set; }
        }
    }
}
