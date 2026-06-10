namespace Waffy.Models
{
    public class DonationsWallet
    {
        public int IdWallet { get; set; }
        public decimal Balance { get; set; }
        public decimal TotalReceived { get; set; }
        public decimal TotalDistributed { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}