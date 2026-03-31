namespace Waffy.Models
{
    using System;

    namespace Waffy.Models
    {
        public class WalletTransaction
        {
            public int Id { get; set; }
            public int WalletId { get; set; }

            public string? TransactionCode { get; set; }
            public string Type { get; set; } = null!;
            public decimal Amount { get; set; }

            public string? PaymentMethod { get; set; }
            public string? Description { get; set; }

            public string? ReferenceType { get; set; }
            public int? ReferenceId { get; set; }

            public string Status { get; set; } = "Completed";
            public DateTime CreatedAt { get; set; }
        }
    }
}
