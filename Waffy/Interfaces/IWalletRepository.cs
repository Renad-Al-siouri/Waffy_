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
    }
}