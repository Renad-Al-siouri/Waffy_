namespace Waffy.Models
{
    using System;

    namespace Waffy.Models
    {
        public class Subscription
        {
            public int Id { get; set; }
            public int PlanId { get; set; }
            public string EntityType { get; set; } = null!;
            public int EntityId { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public decimal? AmountPaid { get; set; }
            public string PaymentStatus { get; set; } = "Pending";
            public string Status { get; set; } = "Active";
            public DateTime CreatedAt { get; set; }
        }
    }
}
