namespace Waffy.Models
{
    public class ClinicDocument
    {
        public int Id { get; set; }
        public int IdClinic { get; set; }
        public string DocumentType { get; set; }
        public string FileUrl { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime? ExpiryDate { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.Now;
        public string? AdminNote { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public int? ReviewedByAdminId { get; set; }
    }
}