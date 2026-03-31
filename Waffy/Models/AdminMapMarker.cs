namespace Waffy.Models
{
    using System;

    namespace Waffy.Models
    {
        public class AdminMapMarker
        {
            public int Id { get; set; }
            public string MarkerType { get; set; } = null!;
            public string? ReferenceType { get; set; }
            public int? ReferenceId { get; set; }
            public decimal Latitude { get; set; }
            public decimal Longitude { get; set; }
            public string Status { get; set; } = "Active";
            public int CreatedByAdminId { get; set; }
            public DateTime CreatedAt { get; set; }
        }
    }
}
