namespace Waffy.Models
{
    public class Wallet
    {
        public int IdWallet { get; set; }
        public int? UserId { get; set; }
        public decimal Balance { get; set; }
        public string? Status { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}