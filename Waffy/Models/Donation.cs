namespace Waffy.Models
{
    public class Donation
    {
        public int Id { get; set; }
        public int IdUser { get; set; }
        public string? DonationType { get; set; }   // Money / Food / Medicine / Supplies
        public decimal? Amount { get; set; }         // للمال فقط
        public int? Quantity { get; set; }           // للأنواع الأخرى
        public string? Unit { get; set; }
        public string? Description { get; set; }
        public string? ProofImageUrl { get; set; }
        public string? TargetType { get; set; }
        public int? TargetId { get; set; }
        public string? ReferenceCode { get; set; }   // WAF-DON-XXXXXX
        public string? Status { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}