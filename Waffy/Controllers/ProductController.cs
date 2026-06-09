using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Waffy.Interfaces;
using Waffy.Models.ViewModels;

namespace Waffy.Controllers
{
    public class ProductController : Controller
    {
        private readonly IStoreRepository _storeRepo;
        private readonly IProductRepository _productRepo;
        private readonly IProductImageRepository _productImageRepo;
        private readonly ICategoryRepository _categoryRepo;
        private readonly IStoreImageRepository _storeImageRepo;

        public ProductController(
            IStoreRepository storeRepo,
            IProductRepository productRepo,
            IProductImageRepository productImageRepo,
            ICategoryRepository categoryRepo,
            IStoreImageRepository storeImageRepo)
        {
            _storeRepo = storeRepo;
            _productRepo = productRepo;
            _productImageRepo = productImageRepo;
            _categoryRepo = categoryRepo;
            _storeImageRepo = storeImageRepo;
        }

        // ══════════════════════════════════════════════════════════
        // STORES — قائمة المتاجر المعتمدة 
        // ══════════════════════════════════════════════════════════
        [HttpGet]
        public async Task<IActionResult> Stores(string? search, string? storeType)
        {
            var allStores = (await _storeRepo.GetApprovedStoresAsync()).ToList();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var q = search.Trim().ToLower();
                allStores = allStores.Where(s =>
                    (s.Name ?? "").ToLower().Contains(q) ||
                    (s.Description ?? "").ToLower().Contains(q) ||
                    (s.City ?? "").ToLower().Contains(q) ||
                    (s.StoreType ?? "").ToLower().Contains(q)
                ).ToList();
            }

            if (!string.IsNullOrWhiteSpace(storeType) && storeType != "all")
                allStores = allStores
                    .Where(s => (s.StoreType ?? "")
                    .Equals(storeType, StringComparison.OrdinalIgnoreCase))
                    .ToList();

            // جلب صور Gallery لكل متجر
            var storeImagesMap = new Dictionary<int, List<string>>();
            foreach (var s in allStores)
            {
                var imgs = (await _storeImageRepo.GetByStoreIdAndTypeAsync(s.IdStore, "Gallery")).ToList();
                var urls = imgs.Select(i => i.ImageUrl).Where(u => !string.IsNullOrEmpty(u)).ToList();

                if (!urls.Any() && !string.IsNullOrEmpty(s.CoverImageUrl))
                    urls.Add(s.CoverImageUrl);
                if (!urls.Any() && !string.IsNullOrEmpty(s.LogoUrl))
                    urls.Add(s.LogoUrl);

                storeImagesMap[s.IdStore] = urls;
            }

            ViewBag.StoreImagesJson = JsonSerializer.Serialize(storeImagesMap);

            var vm = new StoreListViewModel
            {
                Stores = allStores,
                FeaturedStore = allStores.FirstOrDefault(),
                Search = search,
                StoreType = storeType
            };
            return View(vm);
        }

        // ══════════════════════════════════════════════════════════
        // PRODUCTS — منتجات متجر معين (تعرض فقط المنتجات المتاحة IsVisible = 1)
        // ══════════════════════════════════════════════════════════
        [HttpGet]
        public async Task<IActionResult> Products(
            int storeId, string? search, int? categoryId, decimal? maxPrice)
        {
            var store = await _storeRepo.GetByIdAsync(storeId);
            if (store == null) return NotFound();

            // الدالة المفلترة جلبناها وتأكدنا أنها تعرض فقط IsVisible = 1 للزبائن مسبقاً
            var products = (await _productRepo.GetFilteredAsync(
                storeId,
                string.IsNullOrWhiteSpace(search) ? null : search.Trim(),
                categoryId,
                maxPrice
            )).Where(p => p.IsVisible == true || p.IsVisible == null).ToList(); // تأكيد إضافي لضمان عدم ظهور أي منتج مخفي

            // جلب صور كل منتج وبناء JSON للـ slider 
            var productsWithImages = new List<object>();
            foreach (var p in products)
            {
                var imgs = (await _productImageRepo.GetByProductIdAsync(p.Id)).ToList();
                var allUrls = imgs.Select(i => i.ImageUrl).Where(u => !string.IsNullOrEmpty(u)).ToList();

                if (!allUrls.Any() && !string.IsNullOrEmpty(p.ImageUrl))
                    allUrls.Add(p.ImageUrl);

                productsWithImages.Add(new
                {
                    id = p.Id,
                    name = p.Name,
                    description = p.Description ?? "",
                    price = p.Price,
                    salePrice = p.SalePrice,
                    stock = p.StockQuantity,
                    images = allUrls
                });
            }

            // جلب فئات المتجر
            var categories = (await _categoryRepo.GetByStoreIdAsync(storeId)).ToList();

            ViewBag.ProductsJson = JsonSerializer.Serialize(productsWithImages);
            ViewBag.CategoriesJson = JsonSerializer.Serialize(categories.Select(c => new
            {
                id = c.IdCategory,
                name = c.Name,
                icon = c.Icon ?? "📦"
            }));

            var vm = new ProductListViewModel
            {
                Store = store,
                Products = products,
                TotalCount = products.Count,
                Search = search,
                CategoryId = categoryId,
                MaxPrice = maxPrice
            };
            return View(vm);
        }
    }
}