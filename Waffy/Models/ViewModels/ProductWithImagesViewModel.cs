namespace Waffy.Models.ViewModels
{
    public class ProductWithImagesViewModel
    {
        public Product Product { get; set; } = null!;
        public List<ProductImage> Images { get; set; } = new();
        public string? MainImageUrl => Images.FirstOrDefault(i => i.IsMain)?.ImageUrl
                                    ?? Images.FirstOrDefault()?.ImageUrl
                                    ?? Product.ImageUrl;
    }
}