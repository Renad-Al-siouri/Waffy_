using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Waffy.Interfaces;
using Waffy.Models.ViewModels;

namespace Waffy.Controllers
{
    public class SuperAdminController : Controller
    {
        // ── Dependencies — دمج من النسختين ───────────────────────
        private readonly IUserRepository _userRepo;
        private readonly IStoreRepository _storeRepo;
        private readonly IProductRepository _productRepo;
        private readonly IOrderRepository _orderRepo;
        private readonly IStoreDocumentRepository _docRepo;
        private readonly IDangerousAnimalReportRepository _dangerRepo;

        public SuperAdminController(
            IUserRepository userRepo,
            IStoreRepository storeRepo,
            IProductRepository productRepo,
            IOrderRepository orderRepo,
            IStoreDocumentRepository docRepo,
    IDangerousAnimalReportRepository dangerRepo)
        {
            _userRepo = userRepo;
            _storeRepo = storeRepo;
            _productRepo = productRepo;
            _orderRepo = orderRepo;
            _docRepo = docRepo;
            _dangerRepo = dangerRepo;
        }

        // ── Guards ───────────────────────────────────────────────
        private bool IsAdmin() =>
            HttpContext.Session.GetString("RoleName") == "Admin";

        private IActionResult NotAdmin() =>
            RedirectToAction("Login", "Account");

        // ── Admin Info ───────────────────────────────────────────
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

        private void SetAdminViewBag()
        {
            ViewBag.AdminName = AdminName;
            ViewBag.AdminInitials = AdminInitials;
        }

        // ════════════════════════════════════════════════════════
        // Dashboard
        // ════════════════════════════════════════════════════════
        public IActionResult Dashboard()
        {
            if (!IsAdmin()) return NotAdmin();
            SetAdminViewBag();
            return View();
        }

        // ════════════════════════════════════════════════════════
        // Users — GET
        // من النسخة الأولى (الأكمل)
        // ════════════════════════════════════════════════════════
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
            if (vm.CurrentPage > vm.TotalPages && vm.TotalPages > 0)
                vm.CurrentPage = vm.TotalPages;

            SetAdminViewBag();
            if (TempData["SuccessMessage"] != null) ViewBag.Success = TempData["SuccessMessage"];
            if (TempData["ErrorMessage"] != null) ViewBag.Error = TempData["ErrorMessage"];

            return View(vm);
        }

        // ════════════════════════════════════════════════════════
        // User Details — GET
        // ════════════════════════════════════════════════════════
        public async Task<IActionResult> Details_U(
            int id,
            string? returnSearch, string? returnStatus,
            string? returnRole, int returnPage = 1)
        {
            if (!IsAdmin()) return NotAdmin();

            var user = await _userRepo.GetByIdForAdminAsync(id);
            if (user == null) return NotFound();

            var vm = new UserDetailViewModel
            {
                User = user,
                ReturnSearch = returnSearch,
                ReturnStatus = returnStatus,
                ReturnRole = returnRole,
                ReturnPage = returnPage
            };

            SetAdminViewBag();
            if (TempData["SuccessMessage"] != null) ViewBag.Success = TempData["SuccessMessage"];
            if (TempData["ErrorMessage"] != null) ViewBag.Error = TempData["ErrorMessage"];

            return View(vm);
        }

        // ════════════════════════════════════════════════════════
        // SuspendUser — POST
        // ════════════════════════════════════════════════════════
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SuspendUser(
            int userId, string? returnSearch, string? returnStatus,
            string? returnRole, int returnPage = 1, string? from = null)
        {
            if (!IsAdmin()) return NotAdmin();
            await _userRepo.UpdateStatusAsync(userId, "Suspended");
            TempData["SuccessMessage"] = "🔒 User has been suspended.";
            return from == "details"
                ? RedirectToAction("Details_U", new { id = userId, returnSearch, returnStatus, returnRole, returnPage })
                : RedirectToAction("Users", new { search = returnSearch, status = returnStatus, role = returnRole, page = returnPage });
        }

        // ════════════════════════════════════════════════════════
        // ActivateUser — POST
        // ════════════════════════════════════════════════════════
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ActivateUser(
            int userId, string? returnSearch, string? returnStatus,
            string? returnRole, int returnPage = 1, string? from = null)
        {
            if (!IsAdmin()) return NotAdmin();
            await _userRepo.UpdateStatusAsync(userId, "Active");
            TempData["SuccessMessage"] = "✅ User account has been activated.";
            return from == "details"
                ? RedirectToAction("Details_U", new { id = userId, returnSearch, returnStatus, returnRole, returnPage })
                : RedirectToAction("Users", new { search = returnSearch, status = returnStatus, role = returnRole, page = returnPage });
        }

        // ════════════════════════════════════════════════════════
        // DeleteUser — POST
        // ════════════════════════════════════════════════════════
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(
            int userId, string? returnSearch, string? returnStatus,
            string? returnRole, int returnPage = 1)
        {
            if (!IsAdmin()) return NotAdmin();
            await _userRepo.DeleteAsync(userId);
            TempData["SuccessMessage"] = "🗑 User deleted successfully.";
            return RedirectToAction("Users", new { search = returnSearch, status = returnStatus, role = returnRole, page = returnPage });
        }

        // ════════════════════════════════════════════════════════
        // Stores — GET
        // من النسخة الثانية: GetAllAsync + اسم المالك
        // ════════════════════════════════════════════════════════
        public async Task<IActionResult> Stores()
        {
            if (!IsAdmin()) return NotAdmin();
            SetAdminViewBag();

            // ✅ GetAllAsync — يجلب جميع المتاجر (ليس Pending فقط)
            var stores = (await _storeRepo.GetAllAsync()).ToList();

            ViewBag.TotalStores = stores.Count;
            ViewBag.ActiveStores = stores.Count(s => s.VerificationStatus == "Approved");
            ViewBag.PendingStores = stores.Count(s => s.VerificationStatus == "Pending");
            ViewBag.SuspendedStores = stores.Count(s => s.VerificationStatus == "Suspended");

            // ✅ جلب اسم المالك لكل متجر
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
            if (TempData["ErrorMessage"] != null) ViewBag.Error = TempData["ErrorMessage"];

            return View();
        }

        // ════════════════════════════════════════════════════════
        // Details_S — GET
        // من النسخة الثانية: بيانات حقيقية (Products+Orders+Docs)
        // ════════════════════════════════════════════════════════
        public async Task<IActionResult> Details_S(int id)
        {
            if (!IsAdmin()) return NotAdmin();
            SetAdminViewBag();

            var store = await _storeRepo.GetByIdAsync(id);
            if (store == null) return NotFound();

            var owner = await _userRepo.GetByIdForAdminAsync(store.OwnerUserId);
            var ownerName = owner?.FullName ?? "—";

            var products = (await _productRepo.GetByStoreIdAsync(id)).ToList();
            var orders = (await _orderRepo.GetByStoreIdAsync(id)).ToList();
            var docs = (await _docRepo.GetByStoreIdAsync(id)).ToList();
            var revenue = await _orderRepo.GetTotalRevenueByStoreAsync(id);

            var storeData = new
            {
                id = store.IdStore,
                name = store.Name ?? "",
                emoji = "🏪",
                owner = ownerName,
                email = store.Email ?? "",
                phone = store.Phone ?? "",
                city = store.City ?? "",
                description = store.Description ?? "",
                model = "Subscription",
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
                },
                recentProducts = products.Take(3).Select(p => new
                {
                    name = p.Name ?? "",
                    price = p.Price.ToString("F2") + " JD",
                    status = p.StockQuantity == 0 ? "out"
                           : p.IsVisible ? "active"
                           : "hidden"
                }),
                recentOrders = orders.Take(3).Select(o => new
                {
                    id = o.OrderCode ?? o.IdOrder.ToString(),
                    user = o.RecipientName ?? "—",
                    amount = o.DeliveryFee.ToString("F2") + " JD",
                    status = (o.Status ?? "pending").ToLower()
                })
            };

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

            var ordersJs = orders.Select(o => new
            {
                id = o.OrderCode ?? o.IdOrder.ToString(),
                cust = o.RecipientName ?? "—",
                total = o.DeliveryFee,
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

        // ════════════════════════════════════════════════════════
        // ApproveStore — POST
        // ════════════════════════════════════════════════════════
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveStore(int storeId, string? from = null)
        {
            if (!IsAdmin()) return NotAdmin();
            await _storeRepo.UpdateVerificationAsync(storeId, "Approved");
            var store = await _storeRepo.GetByIdAsync(storeId);
            if (store != null) await _userRepo.UpdateStatusAsync(store.OwnerUserId, "Active");
            TempData["SuccessMessage"] = "✅ Store approved. The owner can now log in.";
            return from == "details"
                ? RedirectToAction("Details_S", new { id = storeId })
                : RedirectToAction("Stores");
        }

        // ════════════════════════════════════════════════════════
        // RejectStore — POST
        // ════════════════════════════════════════════════════════
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectStore(
            int storeId, string? reason = null, string? message = null, string? from = null)
        {
            if (!IsAdmin()) return NotAdmin();
            await _storeRepo.UpdateVerificationAsync(storeId, "Rejected");
            TempData["SuccessMessage"] = "❌ Store registration rejected.";
            return from == "details"
                ? RedirectToAction("Details_S", new { id = storeId })
                : RedirectToAction("Stores");
        }

        // ════════════════════════════════════════════════════════
        // SuspendStore — POST
        // ════════════════════════════════════════════════════════
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SuspendStore(
            int storeId, string? reason = null, string? duration = null,
            string? message = null, string? from = null)
        {
            if (!IsAdmin()) return NotAdmin();
            await _storeRepo.UpdateVerificationAsync(storeId, "Suspended");
            var store = await _storeRepo.GetByIdAsync(storeId);
            if (store != null) await _userRepo.UpdateStatusAsync(store.OwnerUserId, "Suspended");
            TempData["SuccessMessage"] = "🔒 Store has been suspended.";
            return from == "details"
                ? RedirectToAction("Details_S", new { id = storeId })
                : RedirectToAction("Stores");
        }

        // ════════════════════════════════════════════════════════
        // ReactivateStore — POST
        // ════════════════════════════════════════════════════════
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ReactivateStore(int storeId, string? from = null)
        {
            if (!IsAdmin()) return NotAdmin();
            await _storeRepo.UpdateVerificationAsync(storeId, "Approved");
            var store = await _storeRepo.GetByIdAsync(storeId);
            if (store != null) await _userRepo.UpdateStatusAsync(store.OwnerUserId, "Active");
            TempData["SuccessMessage"] = "✅ Store reactivated successfully.";
            return from == "details"
                ? RedirectToAction("Details_S", new { id = storeId })
                : RedirectToAction("Stores");
        }

        // ════════════════════════════════════════════════════════
        // DeleteStore — POST
        // ════════════════════════════════════════════════════════
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteStore(int storeId)
        {
            if (!IsAdmin()) return NotAdmin();
            await _storeRepo.DeleteAsync(storeId);
            TempData["SuccessMessage"] = "🗑 Store deleted successfully.";
            return RedirectToAction("Stores");
        }


        // ════════════════════════════════════════════════════════
        // Dangerous_Animals — GET
        // ════════════════════════════════════════════════════════

        public async Task<IActionResult> Dangerous_Animals()
        {
            if (!IsAdmin()) return NotAdmin();
            SetAdminViewBag();

            var reports = (await _dangerRepo.GetAllAsync()).ToList();

            ViewBag.TotalReports = reports.Count;
            ViewBag.OpenReports = reports.Count(r => r.Status == "Pending" || r.Status == "Under Review");
            ViewBag.HighRisk = reports.Count(r => r.Severity == "High");
            ViewBag.ResolvedReports = reports.Count(r => r.Status == "Resolved");

            if (TempData["DangerSuccess"] != null) ViewBag.Success = TempData["DangerSuccess"];
            if (TempData["DangerError"] != null) ViewBag.Error = TempData["DangerError"];

            return View(reports);
        }

        // ════════════════════════════════════════════════════════
        // Dangerous_Animals — POST
        // ════════════════════════════════════════════════════════

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateDangerStatus(int reportId, string status)
        {
            if (!IsAdmin()) return NotAdmin();
            await _dangerRepo.UpdateStatusAsync(reportId, status);
            TempData["DangerSuccess"] = $"✅ Report status updated to {status}.";
            return RedirectToAction("Dangerous_Animals");
        }

        // ════════════════════════════════════════════════════════
        // Stubs — من النسخة الأولى (الأكمل)
        // ════════════════════════════════════════════════════════
        public IActionResult Clinics() { if (!IsAdmin()) return NotAdmin(); SetAdminViewBag(); return View(); }
        public IActionResult Details_C() { if (!IsAdmin()) return NotAdmin(); SetAdminViewBag(); return View(); }
        public IActionResult Reviews_Complaints_C() { if (!IsAdmin()) return NotAdmin(); SetAdminViewBag(); return View(); }
        public IActionResult Analytics_C() { if (!IsAdmin()) return NotAdmin(); SetAdminViewBag(); return View(); }
        public IActionResult Reviews_Complaints_S() { if (!IsAdmin()) return NotAdmin(); SetAdminViewBag(); return View(); }
        public IActionResult Analytics_S() { if (!IsAdmin()) return NotAdmin(); SetAdminViewBag(); return View(); }
        public IActionResult Listings() { if (!IsAdmin()) return NotAdmin(); SetAdminViewBag(); return View(); }
        public IActionResult Flagged() { if (!IsAdmin()) return NotAdmin(); SetAdminViewBag(); return View(); }
        public IActionResult Adopted_Pets() { if (!IsAdmin()) return NotAdmin(); SetAdminViewBag(); return View(); }
        public IActionResult Analytics_A() { if (!IsAdmin()) return NotAdmin(); SetAdminViewBag(); return View(); }
        public IActionResult Lost_Pets() { if (!IsAdmin()) return NotAdmin(); SetAdminViewBag(); return View(); }
        public IActionResult Found_Pets() { if (!IsAdmin()) return NotAdmin(); SetAdminViewBag(); return View(); }
       
        public IActionResult Donations() { if (!IsAdmin()) return NotAdmin(); SetAdminViewBag(); return View(); }
        public IActionResult Laws_Regulations() { if (!IsAdmin()) return NotAdmin(); SetAdminViewBag(); return View(); }
        public IActionResult Map() { if (!IsAdmin()) return NotAdmin(); SetAdminViewBag(); return View(); }
        public IActionResult Notifications() { if (!IsAdmin()) return NotAdmin(); SetAdminViewBag(); return View(); }
    }
}