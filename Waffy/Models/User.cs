namespace Waffy.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class User
    {
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(255)]
        public string PasswordHash { get; set; }

        [Required]
        [StringLength(20)]
        public string Phone { get; set; }

        public bool IsPhoneVerified { get; set; } = false;

        public bool IsEmailVerified { get; set; } = false;

        [Required]
        [StringLength(100)]
        public string FullName { get; set; }

        [Required]
        [StringLength(100)]
        public string City { get; set; }

        [StringLength(500)]
        public string? ProfileImage { get; set; }

        [StringLength(100)]
        public string? Area { get; set; }

        [StringLength(300)]
        public string? AddressDetail { get; set; }

        public decimal? Latitude { get; set; }

        public decimal? Longitude { get; set; }

        [StringLength(20)]
        public string? WhatsAppPhone { get; set; }

        public DateTime? BirthDate { get; set; }

        [StringLength(20)]
        public string? Gender { get; set; }

        [StringLength(500)]
        public string? Bio { get; set; }

        [Required]
        [StringLength(20)]
        public string AccountStatus { get; set; } = "Active";

        public int WarningCount { get; set; } = 0;

        [StringLength(500)]
        public string? SuspensionReason { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? LastActiveAt { get; set; }

        [Required]
        public int RoleId { get; set; }
    }
}
