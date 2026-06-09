using Waffy.Models;
using Waffy.Models.ViewModels;

namespace Waffy.Interfaces
{
    public interface IAppointmentRepository
    {
        // ── Booking ──────────────────────────────────────
        Task<int> BookAsync(Appointment appt);

        // ── Slot availability ─────────────────────────────
        Task<IEnumerable<TimeSpan>> GetBookedSlotsAsync(int idDoctor, DateTime date);
        Task<bool> IsSlotAvailableAsync(int idDoctor, DateTime date, TimeSpan startTime);

        // ── Queries ───────────────────────────────────────
        Task<AppointmentDto?> GetByIdAsync(int id);
        Task<IEnumerable<AppointmentDto>> GetByUserIdAsync(int userId);
        Task<IEnumerable<AppointmentDto>> GetByDoctorIdAsync(int idDoctor, string? statusFilter = null);
        Task<IEnumerable<AppointmentDto>> GetByClinicIdAsync(int idClinic, string? statusFilter = null);
        Task<IEnumerable<AppointmentDto>> GetTodayByDoctorIdAsync(int idDoctor);

        // ── Status ────────────────────────────────────────
        Task CancelAsync(int id, string reason);
        Task CompleteAsync(int id);

        // ── Stats ─────────────────────────────────────────
        Task<int> CountByDoctorAndDateAsync(int idDoctor, DateTime date);

        Task<IEnumerable<AppointmentDto>> GetTomorrowConfirmedAsync();
        Task MarkReminderSentAsync(int appointmentId);
        // أضيفي هذا الـ method:
        Task UpdateMedicalNoteAsync(int appointmentId, string? note);
        // ── Approval Flow ─────────────────────────────────────
        // للعيادة: توافق + تعين طبيب
        Task ApproveAndAssignAsync(int appointmentId, int doctorId);

        // للطبيب: يوافق على موعد مخصص له
        Task ApproveByDoctorAsync(int appointmentId);

        // تأكيد حضور المريض (يُرجع الإيداع)
        Task ConfirmAttendanceAsync(int appointmentId);

        // تسجيل غياب (يُحتجز الإيداع)
        Task MarkNoShowAsync(int appointmentId);

        // ── Deposit ───────────────────────────────────────────
        Task MarkDepositPaidAsync(int appointmentId, string payMethod);
        Task MarkDepositReturnedAsync(int appointmentId);
        Task MarkCliqSubmittedAsync(int appointmentId);

        Task<IEnumerable<TimeSpan>> GetClinicBookedSlotsAsync(int clinicId, DateTime date);
        Task MarkCliqRefundPendingAsync(int appointmentId);
        Task AssignDoctorAsync(int appointmentId, int doctorId);
        Task SetStatusAsync(int appointmentId, string status);

        // IAppointmentRepository
        Task<IEnumerable<AppointmentDto>> GetAllAsync(
            string? statusFilter = null,
            int? clinicId = null);

        Task MarkDepositPaidBySuperAdminAsync(
    int appointmentId,
    string payMethod,
    int? adminId,
    int? holderWalletId,
    string? note);

        Task MarkDepositReturnedBySuperAdminAsync(
            int appointmentId,
            int? adminId);

        Task CancelWithMetadataAsync(
            int appointmentId,
            string reason,
            string cancelledBy);

        Task RejectWithMetadataAsync(
            int appointmentId,
            string reason,
            string rejectedBy);

        Task<bool> IsAppointmentCancellableBefore24hAsync(int appointmentId);

        Task<bool> IsDoctorValidForAppointmentAsync(
            int appointmentId,
            int doctorId);


    }
}