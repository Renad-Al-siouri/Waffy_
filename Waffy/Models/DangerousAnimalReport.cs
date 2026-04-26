namespace Waffy.Models
{
    public class DangerousAnimalReport
    {
        public int IdDangReport { get; set; }
        public int? IdUser { get; set; }
        public string? AnimalType { get; set; }
        public string? ProblemType { get; set; }
        public string? Severity { get; set; }
        public string? Description { get; set; }
        public string? City { get; set; }
        public string? Area { get; set; }
        public string? LocationText { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string? Status { get; set; }
        public string? ReportCode { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}