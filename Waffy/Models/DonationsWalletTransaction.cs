namespace Waffy.Models
{
    public class DonationsWalletTransaction
    {
        public int Id { get; set; }
        public int IdWallet { get; set; }
        public string Type { get; set; } = string.Empty; // Incoming | Distribution
        public decimal Amount { get; set; }
        public int? IdDonation { get; set; }
        public int? IdAnimal { get; set; }
        public string? Note { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation (اختياري للعرض)
        public string? AnimalName { get; set; }
        public string? DonorName { get; set; }
    }
}