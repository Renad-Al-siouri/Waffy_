namespace Waffy.Models
{
    using System;

    namespace Waffy.Models
    {
        public class Order
        {
            public int Id { get; set; }
            public int UserId { get; set; }
            public int StoreId { get; set; }
            public string? OrderCode { get; set; }
            public string Status { get; set; } = "Pending";
            public string? PaymentMethod { get; set; }
            public string PaymentStatus { get; set; } = "Pending";
            public string? RecipientName { get; set; }
            public string? RecipientPhone { get; set; }
            public string? DeliveryAddress { get; set; }
            public decimal? DeliveryFee { get; set; }
            public string? Notes { get; set; }
            public string? CancellationReason { get; set; }
            public DateTime CreatedAt { get; set; }
        }
    }
}
