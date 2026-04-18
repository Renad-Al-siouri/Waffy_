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

        // ══════════════════════════════════════════════════════════
        // HELPERS
        // ══════════════════════════════════════════════════════════

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
                    : parts.Length > 0
                        ? parts[0][0].ToString().ToUpper()
                        : "AD";
            }
        }

        private void SetAdminBag()
        {
            ViewBag.AdminName = AdminName;
            ViewBag.AdminInitials = AdminInitials;
            if (TempData["SuccessMessage"] != null) ViewBag.Success = TempData["SuccessMessage"];
            if (TempData["ErrorMessage"] != null) ViewBag.Error = TempData["ErrorMessage"];
        }

        private static string MapStoreStatus(string? v) => v switch
        {
            "Approved" => "active",
            "Pending" => "pending",
            "Suspended" => "suspended",
            "Rejected" => "rejected",
            _ => "pending"
        };

        /// <summary>اسم مقروء للوثيقة حسب نوعها</summary>
        private static string MapDocLabel(string? docType) => docType switch
        {
            "CommercialRegister" => "📜 Commercial Registration",
            "BusinessLicense" => "🏢 Business License",
            "TrademarkCertificate" => "™️ Trademark Certificate",
            "AdditionalFile" => "💼 Additional Document",
            _ => docType ?? "Document"
        };

        /// <summary>هل الملف صورة أم PDF؟</summary>
        private static bool IsImageFile(string? url)
        {
            if (string.IsNullOrEmpty(url)) return false;
            var ext = Path.GetExtension(url).ToLowerInvariant();
            return ext is ".jpg" or ".jpeg" or ".png" or ".gif" or ".webp";
        }

        // ══════════════════════════════════════════════════════════
        // DASHBOARD
        // ══════════════════════════════════════════════════════════

        public IActionResult Dashboard()
        {
            if (!IsAdmin()) return NotAdmin();
            SetAdminBag();
            return View();
        }

        // ══════════════════════════════════════════════════════════
        // STORES
        // ══════════════════════════════════════════════════════════

        public async Task<IActionResult> Stores()
        {
            if (!IsAdmin()) return NotAdmin();
            SetAdminBag();

            var stores = (await _storeRepo.GetAllAsync()).ToList();

            ViewBag.TotalStores = stores.Count;
            ViewBag.ActiveStores = stores.Count(s => s.VerificationStatus == "Approved");
            ViewBag.PendingStores = stores.Count(s => s.VerificationStatus == "Pending");
            ViewBag.SuspendedStores = stores.Count(s => s.VerificationStatus == "Suspended");

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
                owner = owners.TryGetValue(s.OwnerUserId, out var on) ? on : "",
                email = s.Email ?? "",
                phone = s.Phone ?? "",
                city = s.City ?? "",
                status = MapStoreStatus(s.VerificationStatus),
                rating = s.Rating ?? 0,
                joined = s.CreatedAt.ToString("yyyy-MM-dd"),
                logoUrl = s.LogoUrl ?? "",
                products = 0,
                orders = 0,
                revenue = 0
            });

            ViewBag.StoresJson = JsonSerializer.Serialize(storesJs);
            return View();
        }

        // ══════════════════════════════════════════════════════════
        // STORE DETAILS
        // ══════════════════════════════════════════════════════════

        public async Task<IActionResult> Details_S(int id)
        {
            if (!IsAdmin()) return NotAdmin();
            SetAdminBag();

            var store = await _storeRepo.GetByIdAsync(id);
            if (store == null) return NotFound();

            var owner = await _userRepo.GetByIdForAdminAsync(store.OwnerUserId);
            var products = (await _productRepo.GetByStoreIdAsync(id)).ToList();
            var orders = (await _orderRepo.GetByStoreIdAsync(id)).ToList();
            var docs = (await _docRepo.GetByStoreIdAsync(id)).ToList();
            var revenue = await _orderRepo.GetTotalRevenueByStoreAsync(id);

            // ── storeData JSON ──
            var storeData = new
            {
                id = store.IdStore,
                name = store.Name ?? "",
                owner = owner?.FullName ?? "—",
                email = store.Email ?? "",
                phone = store.Phone ?? "",
                city = store.City ?? "",
                description = store.Description ?? "",
                status = MapStoreStatus(store.VerificationStatus),
                joined = store.CreatedAt.ToString("yyyy-MM-dd"),
                deliveryAvailable = store.DeliveryAvailable,
                products = products.Count,
                orders = orders.Count,
                revenue,
                rating = store.Rating ?? 0
            };

            // ── products JSON ──
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

            // ── orders JSON ──
            var ordersJs = orders.Select(o => new
            {
                id = o.IdOrder.ToString(),
                orderCode = o.OrderCode ?? "",
                cust = o.RecipientName ?? "—",
                phone = o.RecipientPhone ?? "",
                address = o.DeliveryAddress ?? "",
                deliveryFee = o.DeliveryFee,
                date = o.CreatedAt.ToString("dd MMM yyyy"),
                status = (o.Status ?? "pending").ToLower(),
                items = Array.Empty<string>()
            });

            // ── documents JSON — بيانات كاملة للعرض والتحقق ──
            var docsJs = docs.Select(d => new
            {
                id = d.Id,
                type = d.DocumentType ?? "",
                label = MapDocLabel(d.DocumentType),
                fileUrl = d.FileUrl ?? "",
                status = (d.Status ?? "pending").ToLower(),
                uploadedAt = d.UploadedAt.ToString("dd MMM yyyy"),
                isImage = IsImageFile(d.FileUrl)
            });

            ViewBag.StoreId = id;
            ViewBag.StoreName = store.Name;
            ViewBag.StoreDataJson = JsonSerializer.Serialize(storeData);
            ViewBag.ProductsJson = JsonSerializer.Serialize(productsJs);
            ViewBag.OrdersJson = JsonSerializer.Serialize(ordersJs);
            ViewBag.DocsJson = JsonSerializer.Serialize(docsJs);   // ← جديد

            return View();
        }

        // ══════════════════════════════════════════════════════════
        // DOCUMENT ACTIONS — جديد
        // ══════════════════════════════════════════════════════════

        /// <summary>الأدمن يوافق على وثيقة واحدة</summary>
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveDocument(int docId, int storeId)
        {
            if (!IsAdmin()) return NotAdmin();
            await _docRepo.UpdateStatusAsync(docId, "Approved");
            TempData["SuccessMessage"] = "✅ Document approved.";
            return RedirectToAction("Details_S", new { id = storeId });
        }

        /// <summary>الأدمن يرفض وثيقة واحدة</summary>
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectDocument(int docId, int storeId)
        {
            if (!IsAdmin()) return NotAdmin();
            await _docRepo.UpdateStatusAsync(docId, "Rejected");
            TempData["SuccessMessage"] = "❌ Document rejected.";
            return RedirectToAction("Details_S", new { id = storeId });
        }

        // ══════════════════════════════════════════════════════════
        // STORE ACTIONS
        // ══════════════════════════════════════════════════════════

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveStore(int storeId, string? from = null)
        {
            if (!IsAdmin()) return NotAdmin();
            await _storeRepo.UpdateVerificationAsync(storeId, "Approved");
            var store = await _storeRepo.GetByIdAsync(storeId);
            if (store != null) await _userRepo.UpdateStatusAsync(store.OwnerUserId, "Active");
            TempData["SuccessMessage"] = "✅ Store approved successfully.";
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

        // ══════════════════════════════════════════════════════════
        // USERS
        // ══════════════════════════════════════════════════════════

        public async Task<IActionResult> Users(
            string? search, string? status, string? role,
            string? sort, int page = 1)
        {
            if (!IsAdmin()) return NotAdmin();
            SetAdminBag();
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
            vm.Users = (await _userRepo.GetAllForAdminAsync(search, status, role, sort, page, pageSize)).ToList();
            vm.TotalUsers = await _userRepo.CountTotalAsync();
            vm.NewThisMonth = await _userRepo.CountNewThisMonthAsync();
            vm.ActiveUsers = await _userRepo.CountByStatusAsync("Active");
            vm.SuspendedUsers = await _userRepo.CountByStatusAsync("Suspended");
            vm.PendingUsers = await _userRepo.CountByStatusAsync("Pending");
            vm.TotalFiltered = await _userRepo.CountFilteredAsync(search, status, role);
            vm.TotalPages = (int)Math.Ceiling((double)vm.TotalFiltered / pageSize);
            if (vm.CurrentPage > vm.TotalPages && vm.TotalPages > 0)
                vm.CurrentPage = vm.TotalPages;
            return View(vm);
        }

        public async Task<IActionResult> Details_U(
            int id, string? returnSearch, string? returnStatus,
            string? returnRole, int returnPage = 1)
        {
            if (!IsAdmin()) return NotAdmin();
            SetAdminBag();
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
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SuspendUser(
            int userId, string? returnSearch, string? returnStatus,
            string? returnRole, int returnPage = 1, string? from = null)
        {
            if (!IsAdmin()) return NotAdmin();
            await _userRepo.UpdateStatusAsync(userId, "Suspended");
            TempData["SuccessMessage"] = "🔒 User suspended.";
            return from == "details"
                ? RedirectToAction("Details_U", new { id = userId, returnSearch, returnStatus, returnRole, returnPage })
                : RedirectToAction("Users", new { search = returnSearch, status = returnStatus, role = returnRole, page = returnPage });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ActivateUser(
            int userId, string? returnSearch, string? returnStatus,
            string? returnRole, int returnPage = 1, string? from = null)
        {
            if (!IsAdmin()) return NotAdmin();
            await _userRepo.UpdateStatusAsync(userId, "Active");
            TempData["SuccessMessage"] = "✅ User activated.";
            return from == "details"
                ? RedirectToAction("Details_U", new { id = userId, returnSearch, returnStatus, returnRole, returnPage })
                : RedirectToAction("Users", new { search = returnSearch, status = returnStatus, role = returnRole, page = returnPage });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(
            int userId, string? returnSearch, string? returnStatus,
            string? returnRole, int returnPage = 1)
        {
            if (!IsAdmin()) return NotAdmin();
            await _userRepo.DeleteAsync(userId);
            TempData["SuccessMessage"] = "🗑 User deleted.";
            return RedirectToAction("Users", new { search = returnSearch, status = returnStatus, role = returnRole, page = returnPage });
        }

        // ══════════════════════════════════════════════════════════
        // STUBS
        // ══════════════════════════════════════════════════════════

        public IActionResult Clinics() { if (!IsAdmin()) return NotAdmin(); SetAdminBag(); return View(); }
        public IActionResult Details_C() { if (!IsAdmin()) return NotAdmin(); SetAdminBag(); return View(); }
        public IActionResult Analytics_S() { if (!IsAdmin()) return NotAdmin(); SetAdminBag(); return View(); }
        public IActionResult Reviews_Complaints_S() { if (!IsAdmin()) return NotAdmin(); SetAdminBag(); return View(); }
        public IActionResult Listings() { if (!IsAdmin()) return NotAdmin(); SetAdminBag(); return View(); }
        public IActionResult Flagged() { if (!IsAdmin()) return NotAdmin(); SetAdminBag(); return View(); }
        public IActionResult Notifications() { if (!IsAdmin()) return NotAdmin(); SetAdminBag(); return View(); }
    }
}