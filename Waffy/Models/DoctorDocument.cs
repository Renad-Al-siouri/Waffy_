namespace Waffy.Models
{
    using System;

    namespace Waffy.Models
    {
        public class DoctorDocument
        {
            public int Id { get; set; }
            public int DoctorId { get; set; }
            public string DocumentType { get; set; } = null!;
            public string FileUrl { get; set; } = null!;
            public string Status { get; set; } = "Pending";
            public DateTime? ExpiryDate { get; set; }
            public DateTime UploadedAt { get; set; }
        }
    }
}
