using Waffy.Models;

namespace Waffy.Interfaces
{
    public interface IWalletRepository
    {
        Task<Wallet?> GetByUserIdAsync(int userId);
        Task<IEnumerable<WalletTransaction>> GetTransactionsByUserIdAsync(int userId);
        Task<int> CreateWalletAsync(int userId);

        Task AddBalanceAsync(int walletId, decimal amount);
        Task AddTransactionAsync(WalletTransaction transaction);
        Task DeductBalanceAsync(int walletId, decimal amount);

        // New
        Task<Wallet?> GetOrCreateByUserIdAsync(int userId);
        Task<Wallet?> GetSuperAdminWalletAsync();
        Task<bool> TransferAsync(
            int fromWalletId,
            int toWalletId,
            decimal amount,
            string type,
            string paymentMethod,
            int? referenceId,
            string? referenceType,
            string? description);

        Task<bool> RefundFromSuperAdminToUserAsync(
            int userId,
            decimal amount,
            int appointmentId,
            int? adminId,
            string description);

        Task<SystemPaymentSettings?> GetPaymentSettingsAsync();
        Task UpdateSuperAdminCliqNumberAsync(string cliqNumber, int adminId);
        Task<IEnumerable<WalletTransaction>> GetTransactionsByWalletIdAsync(int walletId);

    }
}