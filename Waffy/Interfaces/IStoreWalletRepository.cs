using Waffy.Models;

namespace Waffy.Interfaces
{
    public interface IStoreWalletRepository
    {
        Task<StoreWallet?> GetByStoreIdAsync(int storeId);
        Task<StoreWallet> GetOrCreateAsync(int storeId);
        Task AddBalanceAsync(int idStoreWallet, decimal amount);
        Task DeductBalanceAsync(int idStoreWallet, decimal amount);
        Task AddTransactionAsync(StoreWalletTransaction tx);
        Task<IEnumerable<StoreWalletTransaction>> GetTransactionsAsync(int idStoreWallet);
    }
}