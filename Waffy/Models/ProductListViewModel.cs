// Models/ViewModels/ProductListViewModel.cs

namespace Waffy.Models.ViewModels
{
    public class ProductListViewModel
    {
        // بيانات المتجر
        public Store Store { get; set; } = null!;

        // المنتجات بعد الفلترة
        public List<Product> Products { get; set; } = new();

        // عدد الكل قبل الفلترة
        public int TotalCount { get; set; }

        // قيم الفلاتر الحالية (لإعادة عرضها في الـ UI)
        public string? Search { get; set; }
        public int? CategoryId { get; set; }
        public decimal? MaxPrice { get; set; }

        // خريطة الفئات (ID → اسم + أيقونة)
        // سيُستبدل بجلب من الـ DB بعد ربط CategoryRepository
        public static readonly List<(int Id, string Name, string Icon)> CategoryMap = new()
        {
            (1, "Food",        "🍖"),
            (2, "Accessories", "🎀"),
            (3, "Health Care", "💊"),
            (4, "Toys",        "🧸"),
        };

        // اسم الفئة الحالية للعرض
        public string ActiveCategoryName =>
            CategoryId.HasValue
                ? CategoryMap.FirstOrDefault(c => c.Id == CategoryId).Name ?? "All"
                : "All";
    }
}