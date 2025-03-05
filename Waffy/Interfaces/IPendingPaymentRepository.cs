using Waffy.Models;
using Waffy.Models.ViewModels;

namespace Waffy.Interfaces
{
    public interface IPendingPaymentRepository
    {
        Task<int> AddAsync(PendingPayment payment);
        Task<IEnumerable<PendingPaymentAdminDto>> GetAllPendingAsync();
        Task ConfirmAsync(int paymentId, int adminId);
        Task RejectAsync(int paymentId, int adminId, string note);
        Task<PendingPayment?> GetByIdAsync(int paymentId);
    }
}