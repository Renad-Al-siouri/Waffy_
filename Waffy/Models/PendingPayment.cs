namespace Waffy.Models
{
    public class PendingPayment
    {
        public int IdPayment { get; set; }
        public int IdUser { get; set; }
        public decimal Amount { get; set; }
        public string Purpose { get; set; } = string.Empty;
        public int? RelatedId { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";
        public string? ReceiptImageUrl { get; set; }
        public string? AdminNote { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ConfirmedAt { get; set; }
        public int? ConfirmedBy { get; set; }
    }
}