using Waffy.Models;
namespace Waffy.Interfaces
{
    public interface ICartRepository
    {
        // جلب عربة المستخدم (أو إنشاء واحدة إذا لم تكن موجودة)
        Task<Cart> GetOrCreateCartAsync(int userId);

        // جلب عناصر العربة مع بيانات المنتج
        Task<IEnumerable<CartItem>> GetCartItemsAsync(int idCart);

        // إضافة منتج أو زيادة الكمية إذا كان موجوداً
        Task AddOrUpdateItemAsync(int idCart, int idProduct, int idStore, int quantity);

        // تعديل الكمية
        Task UpdateQuantityAsync(int itemId, int quantity);

        // حذف عنصر
        Task RemoveItemAsync(int itemId);

        // تفريغ العربة بعد الطلب
        Task ClearCartAsync(int idCart);

        // عدد العناصر في العربة (للـ badge في الـ navbar)
        Task<int> GetCartCountAsync(int idCart);
        Task RemoveOutOfStockItemsAsync(int idCart);
        // في نهاية الـ interface
        Task ClearCartByStoreAsync(int idCart, int idStore);
        Task<int> GetItemQuantityAsync(int cartId, int productId);
        Task<CartItem?> GetItemByIdAsync(int itemId);
    }
}