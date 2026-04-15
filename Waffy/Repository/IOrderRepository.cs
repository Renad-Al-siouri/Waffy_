using Waffy.Models;
using Waffy.DTOs;

namespace Waffy.Interfaces
{
    public interface IOrderRepository
    {
        Task<IEnumerable<Order>> GetByStoreIdAsync(int idStore);
        Task<Order?> GetByIdAsync(int idOrder);
        Task<IEnumerable<Order>> GetByStoreIdAndStatusAsync(int idStore, string status);
        Task UpdateStatusAsync(int idOrder, string status);
        Task<int> GetOrdersCountByStoreAsync(int idStore);
        Task<decimal> GetTotalRevenueByStoreAsync(int idStore);

        // ── جديد ──
        Task<IEnumerable<CustomerStoreDto>> GetCustomersByStoreAsync(int idStore);
    }
}