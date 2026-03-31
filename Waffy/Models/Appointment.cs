namespace Waffy.Models
{
    using System;

    namespace Waffy.Models
    {
        public class Appointment
        {
            public int Id { get; set; }
            public int UserId { get; set; }
            public int PetId { get; set; }
            public int DoctorId { get; set; }
            public int ClinicId { get; set; }
            public DateTime AppointmentDate { get; set; }
            public TimeSpan StartTime { get; set; }
            public TimeSpan EndTime { get; set; }
            public string? ServiceName { get; set; }
            public string Status { get; set; } = "Pending";
            public string? AppointmentType { get; set; }
            public string UrgencyLevel { get; set; } = "Normal";
            public string? Reason { get; set; }
            public string? Notes { get; set; }
            public decimal? ConsultationFee { get; set; }
            public string? PaymentMethod { get; set; }
            public string PaymentStatus { get; set; } = "Pending";
            public decimal BookingDeposit { get; set; } = 5.00m;
            public string DepositStatus { get; set; } = "Held";
            public string? CancellationReason { get; set; }
            public DateTime CreatedAt { get; set; }
        }
    }
}
