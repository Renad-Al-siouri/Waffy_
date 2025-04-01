namespace Waffy.Models.ViewModels
{
    public class PendingPaymentAdminDto
    {
        public int IdPayment { get; set; }
        public int IdUser { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Purpose { get; set; } = string.Empty;
        public int? RelatedId { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? ReceiptImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}