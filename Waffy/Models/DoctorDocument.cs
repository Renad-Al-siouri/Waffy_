namespace Waffy.Models
{
    public class DoctorDocument
    {
        public int Id { get; set; }
        public int IdDoctor { get; set; }
        public string DocumentType { get; set; } = "";
        public string FileUrl { get; set; } = "";
        public string Status { get; set; } = "Pending";
        public DateTime UploadedAt { get; set; }
        public string? AdminNote { get; set; }
        public DateTime? ReviewedAt { get; set; }
    }
}