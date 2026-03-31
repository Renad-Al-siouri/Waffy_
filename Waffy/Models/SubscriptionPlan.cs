namespace Waffy.Models
{
    using System;

    namespace Waffy.Models
    {
        public class SubscriptionPlan
        {
            public int Id { get; set; }
            public string Name { get; set; } = null!;
            public decimal Price { get; set; }
            public int DurationInDays { get; set; }
            public int? MaxProducts { get; set; }
            public int? MaxDoctors { get; set; }
            public int? MaxAppointments { get; set; }
            public decimal? CommissionRate { get; set; }
            public DateTime CreatedAt { get; set; }
        }
    }
}
