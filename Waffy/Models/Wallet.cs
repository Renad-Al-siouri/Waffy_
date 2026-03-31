namespace Waffy.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class Wallet
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Range(0, 99999999.99)]
        public decimal Balance { get; set; } = 0.00m;

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Active";

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
