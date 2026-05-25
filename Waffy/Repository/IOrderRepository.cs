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
        Task<(decimal Items, decimal Delivery)> GetTotalRevenueByStoreAsync(int storeId);
        Task UpdateStatusAsync(int orderId, string status);
        Task<IEnumerable<CustomerSummary>> GetCustomersByStoreAsync(int storeId);

        /// <summary>
        /// Sets Status = 'Shipped' and stores the assigned delivery-person details.
        /// </summary>
        Task UpdateDeliveryPersonAsync(int orderId, string name, string phone);
       
    }
}