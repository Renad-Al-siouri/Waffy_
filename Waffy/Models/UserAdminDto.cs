
namespace Waffy.Models.DTOs
{
    public class UserAdminDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Phone { get; set; } = "";
        public string? City { get; set; }
        public string? Area { get; set; }
        public string? Gender { get; set; }
        public string? Bio { get; set; }
        public string? ProfileImage { get; set; }
        public string AccountStatus { get; set; } = "";
        public string RoleName { get; set; } = "";
        public DateTime? CreatedAt { get; set; }
        public string? SuspensionReason { get; set; }

        // ── جديد ────────────────────────────────────────────
        public DateTime? LastActiveAt { get; set; }
        public int WarningCount { get; set; }

        // ── Display Helpers ──────────────────────────────────
        public string StatusBadgeClass => AccountStatus switch
        {
            "Active" => "badge-success",
            "Suspended" => "badge-danger",
            "Pending" => "badge-warning",
            _ => "badge-muted"
        };
        public string StatusIcon => AccountStatus switch
        {
            "Active" => "✅",
            "Suspended" => "🔒",
            "Pending" => "⏳",
            _ => "❓"
        };
        public string RoleBadgeClass => RoleName switch
        {
            "User" => "badge-purple",
            "Clinic" => "badge-info",
            "Store" => "badge-orange",
            "Doctor" => "badge-success",
            _ => "badge-muted"
        };
        public string RoleIcon => RoleName switch
        {
            "User" => "🐾",
            "Clinic" => "🏥",
            "Store" => "🏪",
            "Doctor" => "👨‍⚕️",
            _ => "👤"
        };
        public string JoinedLabel => CreatedAt.HasValue
            ? CreatedAt.Value.ToString("MMM yyyy")
            : "—";

        public string LastActiveLabel => LastActiveAt.HasValue
            ? LastActiveAt.Value.ToString("dd MMM yyyy")
            : "Never";

        public string AvatarInitials
        {
            get
            {
                var parts = FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                return parts.Length >= 2
                    ? $"{parts[0][0]}{parts[1][0]}".ToUpper()
                    : FullName.Length > 0 ? FullName[0].ToString().ToUpper() : "?";
            }
        }
    }
}