namespace Waffy.Models
{
    public class DangerousAnimalReport
    {
        // ── DB Columns ─────────────────────────────────────────
        public int IdDangReport { get; set; }   // PK — IDENTITY
        public int? IdUser { get; set; }
        public string? AnimalType { get; set; }
        public string? ProblemType { get; set; }
        public string? Severity { get; set; }   // High / Medium / Low
        public string? Description { get; set; }
        public string? City { get; set; }
        public string? Area { get; set; }
        public string? LocationText { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string? Status { get; set; }   // Pending / Under Review / Resolved
        public string? ReportCode { get; set; }
        public DateTime? CreatedAt { get; set; }

        // ── Navigation (من JOIN مع Users — Dapper يملأها تلقائياً) ──
        public string? ReporterName { get; set; }
        public string? ReporterPhone { get; set; }
    }
}