namespace Waffy.Models
{
    public class ClinicService
    {
        public int Id { get; set; }
        public int? IdClinic { get; set; }
        public string? DocumentType { get; set; } // ← ServiceName
        public string? Status { get; set; }
        public DateTime? UploadedAt { get; set; }
    }
}