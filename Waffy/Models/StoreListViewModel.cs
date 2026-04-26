namespace Waffy.Models.ViewModels
{
    public class StoreListViewModel
    {
        // قائمة المتاجر المعتمدة
        public List<Store> Stores { get; set; } = new();

        // المتجر المميّز (أول متجر أو الأحدث)
        public Store? FeaturedStore { get; set; }

        // قيمة البحث الحالية (تُحفظ لإعادة عرضها في input)
        public string? Search { get; set; }

        // فلتر النوع (اختياري)
        public string? StoreType { get; set; }
    }
}