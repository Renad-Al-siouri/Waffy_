namespace Waffy.Models
{
    public class FoundPetClaim
    {
        public int IdClaim { get; set; }
        public int IdLostReport { get; set; }
        public int? IdUser { get; set; }
        public string? FinderName { get; set; }
        public string? FinderPhone { get; set; }
        public string? Description { get; set; }
        public bool IsVerified { get; set; }
        public string? Status { get; set; }
        public DateTime? CreatedAt { get; set; }

        // ← من JOIN — لا يُحفظ في DB
        public string? PetName { get; set; }
        public decimal? RewardAmount { get; set; }
        public int OwnerId { get; set; }
    }
}