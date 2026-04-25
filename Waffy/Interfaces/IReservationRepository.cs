using Waffy.Models;

namespace Waffy.Interfaces
{
    public interface IReservationRepository
    {
        Task<int> AddAsync(AdoptionReservation reservation);
        Task<IEnumerable<AdoptionReservation>> GetByPostIdAsync(int postId);
        Task<IEnumerable<AdoptionReservation>> GetByRequesterIdAsync(int userId);
        Task<AdoptionReservation?> GetByIdAsync(int id);
        Task<bool> HasPendingAsync(int postId, int userId);
        Task ApproveAsync(int reservationId);
        Task RejectAsync(int reservationId);

        Task CancelAsync(int reservationId);
    }
}