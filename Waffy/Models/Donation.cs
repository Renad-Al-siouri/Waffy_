namespace Waffy.Models
{
    using System;

    namespace Waffy.Models
    {
        public class Donation
        {
            public int Id { get; set; }
            public int DonorId { get; set; }
            public string DonationType { get; set; } = null!;
            public decimal? Amount { get; set; }
            public int? Quantity { get; set; }
            public string? Unit { get; set; }
            public string? Description { get; set; }
            public string? ProofImageUrl { get; set; }
            public string? TargetType { get; set; }
            public int? TargetId { get; set; }
            public string? ReferenceCode { get; set; }
            public string Status { get; set; } = "Pending";
            public DateTime CreatedAt { get; set; }
        }
    }
}
