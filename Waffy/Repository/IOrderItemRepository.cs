using Waffy.Models;

namespace Waffy.Interfaces
{
    public interface IOrderItemRepository
    {
        
        Task<IEnumerable<OrderItem>> GetByOrderIdAsync(int orderId);
        Task AddRangeAsync(IEnumerable<OrderItem> items);
    }
}