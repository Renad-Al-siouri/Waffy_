namespace Waffy.Models
{
    public class SurpriseRewardPool
    {
        public int IdReward { get; set; }
        public string RewardType { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal? Value { get; set; }
        public int? DiscountPercent { get; set; }
        public string? PartnerName { get; set; }
        public string? CouponCode { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public bool IsUsed { get; set; }
        public int? UsedByUserId { get; set; }
        public DateTime? UsedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}