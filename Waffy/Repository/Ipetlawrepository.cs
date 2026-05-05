using Waffy.Models;

namespace Waffy.Repositories.Interfaces
{
    
    public interface IPetLawRepository
    {
        // ── Read ────────────────────────────────────────────────────────────

       
        Task<IEnumerable<PetLaw>> GetAllAsync();

       
        Task<IEnumerable<PetLaw>> GetByCategoryAsync(string category);

        
        Task<IEnumerable<PetLaw>> SearchAsync(string keyword);

        
        Task<PetLaw?> GetByIdAsync(int id);

        
        Task<int> GetTotalCountAsync();

        // ── Write (Admin only) ───────────────────────────────────────────────

       
        Task<int> AddAsync(PetLaw law);

        
        Task<bool> UpdateAsync(PetLaw law);

        
        Task<bool> DeleteAsync(int id);
    }
}