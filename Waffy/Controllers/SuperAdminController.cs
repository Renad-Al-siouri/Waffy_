// Controllers/SuperAdminController.cs
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Waffy.Interfaces;
using Waffy.Models.ViewModels;

namespace Waffy.Controllers
{
    public class SuperAdminController : Controller
    {
        private readonly IUserRepository _userRepo;
        private readonly IStoreRepository _storeRepo;
        private readonly IProductRepository _productRepo;
        private readonly IOrderRepository _orderRepo;
        private readonly IStoreDocumentRepository _docRepo;

        public SuperAdminController(
            IUserRepository userRepo,
            IStoreRepository storeRepo,
            IProductRepository productRepo,
            IOrderRepository orderRepo,
            IStoreDocumentRepository docRepo)
        {
            _userRepo = userRepo;
            _storeRepo = storeRepo;
            _productRepo = productRepo;
            _orderRepo = orderRepo;
            _docRepo = docRepo;
        }

        // ── Guard ────────────────────────────────────────────
        private bool IsAdmin() =>
            HttpContext.Session.GetString("RoleName") == "Admin";

        private IActionResult NotAdmin() =>
            RedirectToAction("Login", "Account");

        private string AdminName =>
            HttpContext.Session.GetString("FullName") ?? "Admin";

        private string AdminInitials
        {
            get
            {
                var parts = AdminName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                return parts.Length >= 2
                    ? $"{parts[0][0]}{parts[1][0]}".ToUpper()
                    : parts.Length > 0 ? parts[0][0].ToString().ToUpper() : "AD";
            }
        }

        // ════════════════════════════════════════════════════
        // Dashboard
        // ════════════════════════════════════════════════════
        public IActionResult Dashboard()
        {
            if (!IsAdmin()) return NotAdmin();
            ViewBag.AdminName = AdminName;
            ViewBag.AdminInitials = AdminInitials;
            return View();
        }

        // ════════════════════════════════════════════════════
        // GET: /SuperAdmin/Stores  ← جلب الكل بدل Pending فقط
        // ════════════════════════════════════════════════════
        public async Task<IActionResult> Stores()
        {
            if (!IsAdmin()) return NotAdmin();
            ViewBag.AdminName = AdminName;
            ViewBag.AdminInitials = AdminInitials;

            // ✅ GetAllAsync بدل GetPendingStoresAsync
            var stores = (await _storeRepo.GetAllAsync()).ToList();

            ViewBag.TotalStores = stores.Count;
            ViewBag.ActiveStores = stores.Count(s => s.VerificationStatus == "Approved");
            ViewBag.PendingStores = stores.Count(s => s.VerificationStatus == "Pending");
            ViewBag.SuspendedStores = stores.Count(s => s.VerificationStatus == "Suspended");

            // جلب اسم المالك لكل متجر
            var ownerIds = stores.Select(s => s.OwnerUserId).Distinct().ToList();
            var owners = new Dictionary<int, string>();
            foreach (var oid in ownerIds)
            {
                var u = await _userRepo.GetByIdForAdminAsync(oid);
                if (u != null) owners[oid] = u.FullName ?? "";
            }

            var storesJs = stores.Select(s => new
            {
                id = s.IdStore,
                name = s.Name ?? "",
                emoji = "🏪",
                owner = owners.TryGetValue(s.OwnerUserId, out var on) ? on : "",
                email = s.Email ?? "",
                phone = s.Phone ?? "",
                city = s.City ?? "",
                model = "Subscription",
                status = s.VerificationStatus == "Approved" ? "active"
                         : s.VerificationStatus == "Pending" ? "pending"
                         : s.VerificationStatus == "Suspended" ? "suspended"
                         : "rejected",
                products = 0,
                orders = 0,
                revenue = 0,
                rating = s.Rating ?? 0,
                joined = s.CreatedAt.ToString("yyyy-MM-dd"),
                logoUrl = s.LogoUrl ?? ""
            });

            ViewBag.StoresJson = JsonSerializer.Serialize(storesJs);

            if (TempData["SuccessMessage"] != null) ViewBag.Success = TempData["SuccessMessage"];

            return View();
        }

        // ════════════════════════════════════════════════════
        // GET: /SuperAdmin/Details_S?id=5  ← تنفيذ حقيقي
        // ════════════════════════════════════════════════════
        public async Task<IActionResult> Details_S(int id)
        {
            if (!IsAdmin()) return NotAdmin();
            ViewBag.AdminName = AdminName;
            ViewBag.AdminInitials = AdminInitials;

            var store = await _storeRepo.GetByIdAsync(id);
            if (store == null) return NotFound();

            // اسم المالك
            var owner = await _userRepo.GetByIdForAdminAsync(store.OwnerUserId);
            var ownerName = owner?.FullName ?? "—";

            // المنتجات
            var products = (await _productRepo.GetByStoreIdAsync(id)).ToList();

            // الطلبات
            var orders = (await _orderRepo.GetByStoreIdAsync(id)).ToList();

            // الوثائق
            var docs = (await _docRepo.GetByStoreIdAsync(id)).ToList();

            // الإيرادات
            var revenue = await _orderRepo.GetTotalRevenueByStoreAsync(id);

            // ══ storeData JSON ══
            var storeData = new
            {
                id = store.IdStore,
                name = store.Name ?? "",
                owner = ownerName,
                email = store.Email ?? "",
                phone = store.Phone ?? "",
                city = store.City ?? "",
                description = store.Description ?? "",
                status = store.VerificationStatus == "Approved" ? "active"
                                 : store.VerificationStatus == "Pending" ? "pending"
                                 : store.VerificationStatus == "Suspended" ? "suspended"
                                 : "rejected",
                joined = store.CreatedAt.ToString("yyyy-MM-dd"),
                deliveryAvailable = store.DeliveryAvailable,
                products = products.Count,
                orders = orders.Count,
                revenue = revenue,
                rating = store.Rating ?? 0,
                documents = new
                {
                    registration = docs.Any(d => d.DocumentType == "CommercialRegister"),
                    license = docs.Any(d => d.DocumentType == "BusinessLicense"),
                    trademark = docs.Any(d => d.DocumentType == "TrademarkCertificate"),
                    tax = docs.Any(d => d.DocumentType == "AdditionalFile")
                }
            };

            // ══ products JSON ══
            var productsJs = products.Select(p => new
            {
                id = p.Id,
                name = p.Name ?? "",
                price = p.Price,
                stock = p.StockQuantity,
                image = p.ImageUrl ?? "",
                status = p.StockQuantity == 0 ? "out"
                       : p.IsVisible ? "active"
                       : "hidden"
            });

            // ══ orders JSON ══
            var ordersJs = orders.Select(o => new
            {
                id = o.IdOrder.ToString(),
                cust = o.CustomerName ?? "—",
                total = o.TotalAmount,
                date = o.CreatedAt.ToString("dd MMM yyyy"),
                status = (o.Status ?? "pending").ToLower(),
                items = new string[0]
            });

            ViewBag.StoreId = id;
            ViewBag.StoreName = store.Name;
            ViewBag.StoreDataJson = JsonSerializer.Serialize(storeData);
            ViewBag.ProductsJson = JsonSerializer.Serialize(productsJs);
            ViewBag.OrdersJson = JsonSerializer.Serialize(ordersJs);

            if (TempData["SuccessMessage"] != null) ViewBag.Success = TempData["SuccessMessage"];

            return View();
        }

        // ════════════════════════════════════════════════════
        // POST Actions
        // ════════════════════════════════════════════════════
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveStore(int storeId, string? from = null)
        {
            if (!IsAdmin()) return NotAdmin();
            await _storeRepo.UpdateVerificationAsync(storeId, "Approved");
            var store = await _storeRepo.GetByIdAsync(storeId);
            if (store != null) await _userRepo.UpdateStatusAsync(store.OwnerUserId, "Active");
            TempData["SuccessMessage"] = "✅ Store approved.";
            return from == "details"
                ? RedirectToAction("Details_S", new { id = storeId })
                : RedirectToAction("Stores");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectStore(int storeId, string? from = null)
        {
            if (!IsAdmin()) return NotAdmin();
            await _storeRepo.UpdateVerificationAsync(storeId, "Rejected");
            TempData["SuccessMessage"] = "❌ Store rejected.";
            return from == "details"
                ? RedirectToAction("Details_S", new { id = storeId })
                : RedirectToAction("Stores");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SuspendStore(int storeId, string? from = null)
        {
            if (!IsAdmin()) return NotAdmin();
            await _storeRepo.UpdateVerificationAsync(storeId, "Suspended");
            var store = await _storeRepo.GetByIdAsync(storeId);
            if (store != null) await _userRepo.UpdateStatusAsync(store.OwnerUserId, "Suspended");
            TempData["SuccessMessage"] = "🔒 Store suspended.";
            return from == "details"
                ? RedirectToAction("Details_S", new { id = storeId })
                : RedirectToAction("Stores");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ReactivateStore(int storeId, string? from = null)
        {
            if (!IsAdmin()) return NotAdmin();
            await _storeRepo.UpdateVerificationAsync(storeId, "Approved");
            var store = await _storeRepo.GetByIdAsync(storeId);
            if (store != null) await _userRepo.UpdateStatusAsync(store.OwnerUserId, "Active");
            TempData["SuccessMessage"] = "✅ Store reactivated.";
            return from == "details"
                ? RedirectToAction("Details_S", new { id = storeId })
                : RedirectToAction("Stores");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteStore(int storeId)
        {
            if (!IsAdmin()) return NotAdmin();
            await _storeRepo.DeleteAsync(storeId);
            TempData["SuccessMessage"] = "🗑 Store deleted.";
            return RedirectToAction("Stores");
        }

        // ════════════════════════════════════════════════════
        // Users Actions (كما كانت)
        // ════════════════════════════════════════════════════
        public async Task<IActionResult> Users(
            string? search, string? status, string? role,
            string? sort, int page = 1)
        {
            if (!IsAdmin()) return NotAdmin();
            const int pageSize = 8;
            sort ??= "newest";
            var vm = new UserManagementViewModel
            {
                SearchQuery = search,
                StatusFilter = status,
                RoleFilter = role,
                SortBy = sort,
                CurrentPage = page,
                PageSize = pageSize
            };
            var users = await _userRepo.GetAllForAdminAsync(search, status, role, sort, page, pageSize);
            vm.Users = users.ToList();
            vm.TotalUsers = await _userRepo.CountTotalAsync();
            vm.NewThisMonth = await _userRepo.CountNewThisMonthAsync();
            vm.ActiveUsers = await _userRepo.CountByStatusAsync("Active");
            vm.SuspendedUsers = await _userRepo.CountByStatusAsync("Suspended");
            vm.PendingUsers = await _userRepo.CountByStatusAsync("Pending");
            vm.TotalFiltered = await _userRepo.CountFilteredAsync(search, status, role);
            vm.TotalPages = (int)Math.Ceiling((double)vm.TotalFiltered / pageSize);
            if (vm.CurrentPage > vm.TotalPages && vm.TotalPages > 0) vm.CurrentPage = vm.TotalPages;
            ViewBag.AdminName = AdminName; ViewBag.AdminInitials = AdminInitials;
            if (TempData["SuccessMessage"] != null) ViewBag.Success = TempData["SuccessMessage"];
            if (TempData["ErrorMessage"] != null) ViewBag.Error = TempData["ErrorMessage"];
            return View(vm);
        }

        public async Task<IActionResult> Details_U(int id,
            string? returnSearch, string? returnStatus, string? returnRole, int returnPage = 1)
        {
            if (!IsAdmin()) return NotAdmin();
            var user = await _userRepo.GetByIdForAdminAsync(id);
            if (user == null) return NotFound();
            var vm = new UserDetailViewModel { User = user, ReturnSearch = returnSearch, ReturnStatus = returnStatus, ReturnRole = returnRole, ReturnPage = returnPage };
            ViewBag.AdminName = AdminName; ViewBag.AdminInitials = AdminInitials;
            if (TempData["SuccessMessage"] != null) ViewBag.Success = TempData["SuccessMessage"];
            if (TempData["ErrorMessage"] != null) ViewBag.Error = TempData["ErrorMessage"];
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SuspendUser(int userId, string? returnSearch, string? returnStatus, string? returnRole, int returnPage = 1, string? from = null)
        {
            if (!IsAdmin()) return NotAdmin();
            await _userRepo.UpdateStatusAsync(userId, "Suspended");
            TempData["SuccessMessage"] = "🔒 User suspended.";
            return from == "details"
                ? RedirectToAction("Details_U", new { id = userId, returnSearch, returnStatus, returnRole, returnPage })
                : RedirectToAction("Users", new { search = returnSearch, status = returnStatus, role = returnRole, page = returnPage });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ActivateUser(int userId, string? returnSearch, string? returnStatus, string? returnRole, int returnPage = 1, string? from = null)
        {
            if (!IsAdmin()) return NotAdmin();
            await _userRepo.UpdateStatusAsync(userId, "Active");
            TempData["SuccessMessage"] = "✅ User activated.";
            return from == "details"
                ? RedirectToAction("Details_U", new { id = userId, returnSearch, returnStatus, returnRole, returnPage })
                : RedirectToAction("Users", new { search = returnSearch, status = returnStatus, role = returnRole, page = returnPage });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(int userId, string? returnSearch, string? returnStatus, string? returnRole, int returnPage = 1)
        {
            if (!IsAdmin()) return NotAdmin();
            await _userRepo.DeleteAsync(userId);
            TempData["SuccessMessage"] = "🗑 User deleted.";
            return RedirectToAction("Users", new { search = returnSearch, status = returnStatus, role = returnRole, page = returnPage });
        }

        // ── Stubs ─────────────────────────────────────────────
        public IActionResult Clinics() { if (!IsAdmin()) return NotAdmin(); return View(); }
        public IActionResult Details_C() { if (!IsAdmin()) return NotAdmin(); return View(); }
        public IActionResult Analytics_S() { if (!IsAdmin()) return NotAdmin(); return View(); }
        public IActionResult Reviews_Complaints_S() { if (!IsAdmin()) return NotAdmin(); return View(); }
        public IActionResult Listings() { if (!IsAdmin()) return NotAdmin(); return View(); }
        public IActionResult Flagged() { if (!IsAdmin()) return NotAdmin(); return View(); }
        public IActionResult Notifications() { if (!IsAdmin()) return NotAdmin(); return View(); }
    }
}