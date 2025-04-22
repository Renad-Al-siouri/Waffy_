namespace Waffy.Models
{
    public class User
    {
        public int UserId { get; set; }

        public string? Email { get; set; }
        public string? PasswordHash { get; set; }
        public int RoleId { get; set; }
        public string? RoleName { get; set; }
        public string? Phone { get; set; }
        public bool IsPhoneVerified { get; set; }
        public bool IsEmailVerified { get; set; }

        public string? FullName { get; set; }
        public string? City { get; set; }
        public string? ProfileImage { get; set; }
        public string? Area { get; set; }
        public string? AddressDetail { get; set; }

        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        public string? WhatsAppPhone { get; set; }
        public DateTime? BirthDate { get; set; }

        public string? Gender { get; set; }
        public string? Bio { get; set; }
        public string? AccountStatus { get; set; }

        public int WarningCount { get; set; }
        public string? SuspensionReason { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? LastActiveAt { get; set; }

        public bool HasUsedRenewalDiscount { get; set; } = false;

    }
}