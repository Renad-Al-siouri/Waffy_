namespace Waffy.Models
{
    public class WalletTransaction
    {
        public int IdWalletTransaction { get; set; }
        public int IdWallet { get; set; }
        public string? TransactionCode { get; set; }
        public string? Type { get; set; }
        public decimal Amount { get; set; }
        public string? PaymentMethod { get; set; }
        public int? ReferenceId { get; set; }
        public string? Status { get; set; }
        public DateTime? CreatedAt { get; set; }

        public string? ReferenceType { get; set; }
        public string? Description { get; set; }
        public decimal? BalanceAfter { get; set; }

        
    }
}