using Waffy.Models;
namespace Waffy.Interfaces
{
    public interface IDonationAnimalRepository
    {
        Task<IEnumerable<DonationAnimal>> GetAllAsync();
        Task<IEnumerable<DonationAnimal>> GetActiveAsync();
        Task<int> CreateAsync(DonationAnimal animal);
        Task UpdateStatusAsync(int id, string status);
        Task DeleteAsync(int id);
        Task<DonationAnimal?> GetByIdAsync(int id);
        Task AddCollectedAmountAsync(int animalId, decimal amount);

    }
}