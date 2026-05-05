using Waffy.Models;

namespace Waffy.Interfaces
{
    public interface IOrderRepository
    {
        Task<int> CreateAsync(Order order);
        Task AddItemAsync(OrderItem item);
        Task<IEnumerable<Order>> GetByUserIdAsync(int userId);
        Task<IEnumerable<OrderItem>> GetItemsByOrderIdAsync(int idOrder);

        Task<Order?> GetByIdAsync(int orderId);
        Task<IEnumerable<Order>> GetByStoreIdAsync(int storeId);
        Task<decimal> GetTotalRevenueByStoreAsync(int storeId);

        Task UpdateStatusAsync(int orderId, string status);
        Task<IEnumerable<CustomerSummary>> GetCustomersByStoreAsync(int storeId);


    }
}