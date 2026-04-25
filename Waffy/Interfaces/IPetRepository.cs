using Waffy.Models;
namespace Waffy.Interfaces
{
    public interface IPetRepository
    {
        Task<IEnumerable<Pet>> GetByUserIdAsync(int userId);
        Task<Pet?> GetByIdAsync(int petId);
        Task<int> AddAsync(Pet pet);
        Task UpdateAsync(Pet pet);
        Task DeleteAsync(int petId);
        Task UpdateImageAsync(int petId, string imageUrl);
        Task TransferOwnershipAsync(int petId, int newOwnerId);


    }
}
