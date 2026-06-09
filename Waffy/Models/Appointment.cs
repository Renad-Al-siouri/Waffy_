namespace Waffy.Models
{
    public class Appointment
    {
        public int Id { get; set; }
        public int IdUser { get; set; }
        public int? IdPet { get; set; }
        public int? IdDoctor { get; set; }
        public int IdClinic { get; set; }
        public DateTime AppointmentDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public string? ServiceName { get; set; }
        public string Status { get; set; } = "PendingPaymentVerification";
        public string? AppointmentType { get; set; }
        public string? UrgencyLevel { get; set; }
        public string? Reason { get; set; }
        public string? Notes { get; set; }
       
        public string? CancellationReason { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? MedicalRecordNote { get; set; }
        public bool ShareMedicalRecord { get; set; }

        // ── Deposit ───────────────────────────────────────────
        public decimal DepositAmount { get; set; } = 5.00m;
        public bool DepositPaid { get; set; }
        public bool DepositReturned { get; set; }
        public bool AttendanceConfirmed { get; set; }
        public bool AssignedByClinic { get; set; }
        public bool DepositCliqSubmitted { get; set; }
        public string? DepositPayMethod { get; set; }
        public string? CliqReference { get; set; }
        public decimal ConsultationFee { get; set; } = 0;
        public string? PaymentMethod { get; set; }
        public string? PaymentStatus { get; set; }

        public string? RefundStatus { get; set; }
        public string? DoctorNotes { get; set; }

        public int? PaymentVerifiedByAdminId { get; set; }
        public DateTime? PaymentVerifiedAt { get; set; }
        public string? PaymentVerificationNote { get; set; }

        public int? DepositHolderWalletId { get; set; }

        public DateTime? RefundedAt { get; set; }
        public int? RefundedByAdminId { get; set; }

        public string? CancelledBy { get; set; }
        public DateTime? CancelledAt { get; set; }

        public string? RejectionReason { get; set; }
    }
}