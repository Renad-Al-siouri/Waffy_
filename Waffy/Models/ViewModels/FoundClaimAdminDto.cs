namespace Waffy.Models.ViewModels
{
    public class FoundClaimAdminDto
    {
        public int IdClaim { get; set; }
        public int IdLostReport { get; set; }
        public int? IdUser { get; set; }
        public string PetName { get; set; } = string.Empty;
        public string AnimalType { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public string OwnerPhone { get; set; } = string.Empty;
        public string FinderName { get; set; } = string.Empty;
        public string FinderPhone { get; set; } = string.Empty;
        public string? FinderEmail { get; set; }
        public bool IsVerified { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal? RewardAmount { get; set; }
        public string? RewardStatus { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string City { get; set; } = string.Empty;
    }
}