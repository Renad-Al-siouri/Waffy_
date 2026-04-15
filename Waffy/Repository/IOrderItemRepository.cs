using Waffy.Models;

namespace Waffy.Interfaces
{
    public interface IOrderItemRepository
    {
        Task<IEnumerable<OrderItem>> GetByOrderIdAsync(int idOrder);
        Task AddRangeAsync(IEnumerable<OrderItem> items);
    }
}