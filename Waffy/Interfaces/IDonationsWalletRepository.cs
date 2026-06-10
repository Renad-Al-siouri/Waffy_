using Waffy.Models;

namespace Waffy.Interfaces
{
    public interface IDonationsWalletRepository
    {
        Task<DonationsWallet?> GetAsync();
        Task AddIncomingAsync(int idWallet, decimal amount, int idDonation, string note);
        Task AddDistributionAsync(int idWallet, decimal amount, int idAnimal, string note);
        Task<List<DonationsWalletTransaction>> GetTransactionsAsync();
    }
}