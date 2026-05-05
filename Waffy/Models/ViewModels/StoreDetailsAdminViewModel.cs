namespace Waffy.Models.ViewModels
{
    public class StoreDetailsAdminViewModel
    {
        public Store Store { get; set; } = null!;
        public string OwnerName { get; set; } = "";
        public string OwnerEmail { get; set; } = "";
        public List<StoreDocumentViewModel> Documents { get; set; } = new();
        public List<ProductWithImagesViewModel> Products { get; set; } = new();
        public List<Order> RecentOrders { get; set; } = new();
        public decimal TotalRevenue { get; set; }
        public List<StoreImage> GalleryImages { get; set; } = new();
    }
}
