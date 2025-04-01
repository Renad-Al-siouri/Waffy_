namespace Waffy.Models.ViewModels
{
    public class LostPetAdminDto
    {
        public int IdLostReport { get; set; }
        public int IdUser { get; set; }
        public string OwnerName { get; set; } = string.Empty;
        public string OwnerPhone { get; set; } = string.Empty;
        public string PetName { get; set; } = string.Empty;
        public string AnimalType { get; set; } = string.Empty;
        public string? Breed { get; set; }
        public string? Color { get; set; }
        public string City { get; set; } = string.Empty;
        public string? Area { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal? RewardAmount { get; set; }
        public string? ReportCode { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string? SecretQuestion { get; set; }
        public bool IsPaid { get; set; }
        public bool HasSurpriseReward { get; set; }
        public int ClaimCount { get; set; }
        public int ConfirmedClaimCount { get; set; }
        public bool RewardReserved { get; set; }
        public decimal AmountPaid { get; set; }
        public bool IsFreePost { get; set; }
    }
}