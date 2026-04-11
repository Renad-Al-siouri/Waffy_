namespace Waffy.Models
{
    public class StoreDocument
    {
        public int Id { get; set; }
        public int IdStore { get; set; }
        public string? DocumentType { get; set; }
        public string? FileUrl { get; set; }
        public string? Status { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime UploadedAt { get; set; }
    }
}