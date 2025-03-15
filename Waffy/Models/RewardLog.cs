namespace Waffy.Models
{
    public class RewardLog
    {
        public int IdRewardLog { get; set; }
        public int IdReward { get; set; }
        public int IdRecipient { get; set; }
        public int IdAdminGiver { get; set; }
        public int? IdLostReport { get; set; }
        public int? IdFoundClaim { get; set; }
        public string GiftType { get; set; } = "";   // FinderReward | ManualGift
        public string ReasonCategory { get; set; } = "";
        public string? ReasonNote { get; set; }
        public string DeliveryStatus { get; set; } = "Delivered";
        public DateTime CreatedAt { get; set; }
    }
}