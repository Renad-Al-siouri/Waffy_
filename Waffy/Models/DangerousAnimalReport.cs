namespace Waffy.Models
{
    using System;

    namespace Waffy.Models
    {
        public class DangerousAnimalReport
        {
            public int Id { get; set; }
            public int ReporterId { get; set; }
            public string AnimalType { get; set; } = null!;
            public string ProblemType { get; set; } = null!;
            public string Severity { get; set; } = null!;
            public string Description { get; set; } = null!;
            public string City { get; set; } = null!;
            public string? Area { get; set; }
            public string? LocationText { get; set; }
            public decimal? Latitude { get; set; }
            public decimal? Longitude { get; set; }
            public string Status { get; set; } = "Open";
            public string? ReportCode { get; set; }
            public DateTime CreatedAt { get; set; }
        }
    }
}
