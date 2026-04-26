using Microsoft.AspNetCore.Mvc;
using Waffy.Interfaces;
using Waffy.Models.ViewModels;

namespace Waffy.Controllers
{
    public class ProductController : Controller
    {
        private readonly IStoreRepository _storeRepo;
        private readonly IProductRepository _productRepo;

        public ProductController(IStoreRepository storeRepo, IProductRepository productRepo)
        {
            _storeRepo = storeRepo;
            _productRepo = productRepo;
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
            {
                allStores = allStores
                    .Where(s => (s.StoreType ?? "")
                    .Equals(storeType, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

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
        // PRODUCTS — منتجات متجر معين مع فلترة server-side
        // ══════════════════════════════════════════════════════════

        [HttpGet]
        public async Task<IActionResult> Products(
    int storeId, string? search, int? categoryId, decimal? maxPrice)
        {
            var store = await _storeRepo.GetByIdAsync(storeId);
            if (store == null) return NotFound();

            var products = (await _productRepo.GetByStoreIdAsync(storeId))
                .Where(p => p.IsVisible)
                .ToList();

            var totalBeforeFilter = products.Count;

            if (!string.IsNullOrWhiteSpace(search))
            {
                var q = search.Trim().ToLower();
                products = products.Where(p =>
                    p.Name.ToLower().Contains(q) ||
                    (p.Description ?? "").ToLower().Contains(q)
                ).ToList();
            }

            if (categoryId.HasValue)
                products = products.Where(p => p.IdCategory == categoryId.Value).ToList();

            if (maxPrice.HasValue)
                products = products.Where(p => (p.SalePrice ?? p.Price) <= maxPrice.Value).ToList();

            var vm = new ProductListViewModel
            {
                Store = store,
                Products = products,
                TotalCount = totalBeforeFilter,
                Search = search,
                CategoryId = categoryId,
                MaxPrice = maxPrice
            };

            return View(vm);
        }
    }
}