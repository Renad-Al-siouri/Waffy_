namespace Waffy.Models
{
    public class StoreDocument
    {
        public int Id { get; set; }
        public int IdStore { get; set; }
        public string? DocumentType { get; set; }
        public string? FileUrl { get; set; }
        public string? Status { get; set; }      // Pending | Approved | Rejected
        public DateTime? ExpiryDate { get; set; }
        public DateTime UploadedAt { get; set; }
        // حقول جديدة للأدمن
        public string? AdminNote { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public int? ReviewedByAdminId { get; set; }
    }
}