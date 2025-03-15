namespace Waffy.Models.ViewModels
{
    public class RewardLogDto
    {
        public int IdRewardLog { get; set; }
        public int IdReward { get; set; }
        public int IdRecipient { get; set; }
        public int IdAdminGiver { get; set; }
        public int? IdLostReport { get; set; }
        public int? IdFoundClaim { get; set; }
        public string GiftType { get; set; } = "";
        public string ReasonCategory { get; set; } = "";
        public string? ReasonNote { get; set; }
        public string DeliveryStatus { get; set; } = "Delivered";
        public DateTime CreatedAt { get; set; }

        // ── Joined from Users (Recipient) ──
        public string RecipientName { get; set; } = "";
        public string RecipientEmail { get; set; } = "";

        // ── Joined from Users (Admin) ──
        public string AdminName { get; set; } = "";

        // ── Joined from SurpriseRewardPool ──
        public string? RewardTitle { get; set; }
        public string? RewardType { get; set; }
        public decimal? RewardValue { get; set; }
        public int? DiscountPercent { get; set; }
        public string? CouponCode { get; set; }
        public string? PartnerName { get; set; }

        // ── Computed Properties ──────────────────────────────
        public string GiftTypeLabel => GiftType switch
        {
            "FinderReward" => "Finder Reward",
            "ManualGift" => "Manual Gift",
            _ => GiftType
        };

        public string RewardDisplay
        {
            get
            {
                if (RewardValue.HasValue)
                    return $"{RewardValue.Value:F2} JOD";
                if (DiscountPercent.HasValue)
                    return $"{DiscountPercent}% off";
                if (CouponCode != null)
                    return CouponCode;
                return "—";
            }
        }
    }
}