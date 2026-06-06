// ══════════════════════════════════════════════════════════
// Models/Review.cs — تحقق أن الـ model مطابق للجدول
// ══════════════════════════════════════════════════════════
namespace Waffy.Models
{
    public class Review
    {
        public int IdReview { get; set; }
        public int IdUser { get; set; }
        public string TargetType { get; set; } = "";  // "Clinic" | "Doctor"
        public int TargetId { get; set; }
        public int Rating { get; set; }         // 1–5
        public string? Title { get; set; }
        public string? Comment { get; set; }
        public string Status { get; set; } = "Active"; // "Active" | "Deleted"
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // ── Joined (not in DB — للعرض فقط) ──
        public string? UserFullName { get; set; }
        public string? UserProfileImage { get; set; }
    }
}