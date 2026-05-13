using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Waffy.Interfaces;
using Waffy.Models;
using Waffy.Models.ViewModels;
using Waffy.Repositories.Interfaces;

namespace Waffy.Controllers
{
    public class SuperAdminController : Controller
    {
        // ── Dependencies (merged from both versions) ─────────────
        private readonly IUserRepository _userRepo;
        private readonly IStoreRepository _storeRepo;
        private readonly IProductRepository _productRepo;
        private readonly IOrderRepository _orderRepo;
        private readonly IOrderItemRepository _orderItemRepo;       // ← from v2
        private readonly IStoreDocumentRepository _docRepo;
        private readonly IDangerousAnimalReportRepository _dangerRepo;
        private readonly IDonationRepository _donationRepo;
        private readonly IDonationAnimalRepository _donationAnimalRepo;
        private readonly IComplaintRepository _complaintRepo;       
        private readonly IPetLawRepository _lawRepo;
        private readonly IAdoptionRepository _adoptionRepo;
        private readonly IReservationRepository _reservationRepo;
        private readonly IPostFlagRepository _flagRepo;
        private readonly INotificationRepository _notifRepo;

        public SuperAdminController(
            IUserRepository userRepo,
            IStoreRepository storeRepo,
            IProductRepository productRepo,
            IOrderRepository orderRepo,
            IOrderItemRepository orderItemRepo,
            IStoreDocumentRepository docRepo,
            IDangerousAnimalReportRepository dangerRepo,
            IDonationRepository donationRepo,
            IDonationAnimalRepository donationAnimalRepo,
            IComplaintRepository complaintRepo,
            IPetLawRepository lawRepo,
            IAdoptionRepository adoptionRepo,
            IReservationRepository reservationRepo,
            IPostFlagRepository flagRepo,
            INotificationRepository notifRepo)
        {
            _userRepo = userRepo;
            _storeRepo = storeRepo;
            _productRepo = productRepo;
            _orderRepo = orderRepo;
            _orderItemRepo = orderItemRepo;
            _docRepo = docRepo;
            _dangerRepo = dangerRepo;
            _donationRepo = donationRepo;
            _donationAnimalRepo = donationAnimalRepo;
            _complaintRepo = complaintRepo;
            _lawRepo = lawRepo;
            _adoptionRepo = adoptionRepo;
            _reservationRepo = reservationRepo;
            _flagRepo = flagRepo;
            _notifRepo = notifRepo;
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

        // ── Helpers (from v2) ────────────────────────────────────
        private static string MapDocLabel(string? docType) => docType switch
        {
            "CommercialRegister" => "📋 Commercial Registration",
            "BusinessLicense" => "🏢 Business License",
            "TrademarkCertificate" => "™️ Trademark Certificate",
            "AdditionalFile" => "📎 Additional Document",
            _ => docType ?? "Document"
        };

        private static bool IsImageFile(string? fileUrl)
        {
            if (string.IsNullOrEmpty(fileUrl)) return false;
            var ext = System.IO.Path.GetExtension(fileUrl).ToLowerInvariant();
            return ext is ".jpg" or ".jpeg" or ".png" or ".gif" or ".webp";
        }

        private static string GetCategoryLabel(string category) => category switch
        {
            "travel" => "✈️ Pet Travel",
            "adoption" => "🐾 Adoption",
            "protection" => "🛡 Animal Protection",
            "vaccination" => "💉 Vaccinations",
            "breeding" => "🧬 Breeding & Import",
            "stray" => "🏚 Stray Animals",
            _ => category
        };

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
        // Users — GET (from v1: complete with return params)
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
        // ════════════════════════════════════════════════════════
        public async Task<IActionResult> Stores()
        {
            if (!IsAdmin()) return NotAdmin();
            SetAdminViewBag();

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
        // Details_S — GET (merged: real data from v2 + v1 fields)
        // ════════════════════════════════════════════════════════
        public async Task<IActionResult> Details_S(int id)
        {
            if (!IsAdmin()) return NotAdmin();
            SetAdminViewBag();

            var store = await _storeRepo.GetByIdAsync(id);
            if (store == null) return NotFound();

            var owner = await _userRepo.GetByIdForAdminAsync(store.OwnerUserId);
            var products = (await _productRepo.GetByStoreIdAsync(id)).ToList();
            var orders = (await _orderRepo.GetByStoreIdAsync(id)).ToList();
            var docs = (await _docRepo.GetByStoreIdAsync(id)).ToList();
            var revenue = await _orderRepo.GetTotalRevenueByStoreAsync(id);
            var customers = (await _orderRepo.GetCustomersByStoreAsync(id)).ToList();

            // Orders with real totals from OrderItems (from v2)
            var ordersJs = new List<object>();
            foreach (var o in orders)
            {
                var items = (await _orderItemRepo.GetByOrderIdAsync(o.IdOrder)).ToList();
                var orderTotal = items.Sum(i => i.Quantity * (i.SalePrice ?? i.UnitPrice));
                ordersJs.Add(new
                {
                    id = o.OrderCode ?? o.IdOrder.ToString(),
                    idOrder = o.IdOrder,
                    cust = o.RecipientName ?? "—",
                    phone = o.RecipientPhone ?? "",
                    address = o.DeliveryAddress ?? "",
                    total = orderTotal + o.DeliveryFee,
                    deliveryFee = o.DeliveryFee,
                    date = o.CreatedAt.ToString("dd MMM yyyy"),
                    status = (o.Status ?? "pending").ToLower(),
                    items = items.Select(i =>
                        $"{i.ProductNameSnapshot ?? "Product"} × {i.Quantity}").ToList()
                });
            }

            var storeData = new
            {
                id = store.IdStore,
                name = store.Name ?? "",
                emoji = "🏪",
                owner = owner?.FullName ?? "—",
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
                revenue,
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
                    status = p.StockQuantity == 0 ? "out" : p.IsVisible ? "active" : "hidden"
                }),
                recentOrders = orders.Take(3).Select(o => new
                {
                    id = o.OrderCode ?? o.IdOrder.ToString(),
                    user = o.RecipientName ?? "—",
                    amount = o.DeliveryFee.ToString("F2") + " JD",
                    status = (o.Status ?? "pending").ToLower()
                })
            };

            ViewBag.StoreId = id;
            ViewBag.StoreName = store.Name;
            ViewBag.StoreDataJson = JsonSerializer.Serialize(storeData);

            ViewBag.ProductsJson = JsonSerializer.Serialize(products.Select(p => new
            {
                id = p.Id,
                name = p.Name ?? "",
                price = p.Price,
                stock = p.StockQuantity,
                image = p.ImageUrl ?? "",
                status = p.StockQuantity == 0 ? "out" : p.IsVisible ? "active" : "hidden"
            }));

            ViewBag.OrdersJson = JsonSerializer.Serialize(ordersJs);

            // Docs with labels and image detection (from v2)
            ViewBag.DocsJson = JsonSerializer.Serialize(docs.Select(d => new
            {
                id = d.Id,
                type = d.DocumentType ?? "",
                label = MapDocLabel(d.DocumentType),
                fileUrl = d.FileUrl ?? "",
                status = (d.Status ?? "pending").ToLower(),
                adminNote = d.AdminNote ?? "",
                uploadedAt = d.UploadedAt.ToString("dd MMM yyyy"),
                isImage = IsImageFile(d.FileUrl)
            }));

            // Customers (from v2)
            ViewBag.CustomersJson = JsonSerializer.Serialize(customers.Select(c => new
            {
                name = c.FullName ?? "—",
                email = c.Email ?? "—",
                phone = c.Phone ?? "—",
                orders = c.OrderCount,
                spent = c.TotalSpent,
                last = c.LastOrderDate?.ToString("dd MMM yyyy") ?? "—"
            }));

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
        // Analytics_S — GET (from v2: real data per store)
        // ════════════════════════════════════════════════════════
        public async Task<IActionResult> Analytics_S()
        {
            if (!IsAdmin()) return NotAdmin();
            SetAdminViewBag();

            var stores = (await _storeRepo.GetAllAsync()).ToList();
            decimal totalRevenue = 0;
            int totalOrders = 0;

            var storesWithStats = new List<object>();
            var monthlyOrders = new int[12];

            foreach (var s in stores)
            {
                var storeOrders = (await _orderRepo.GetByStoreIdAsync(s.IdStore)).ToList();
                var storeRevenue = await _orderRepo.GetTotalRevenueByStoreAsync(s.IdStore);
                var storeProducts = (await _productRepo.GetByStoreIdAsync(s.IdStore)).ToList();

                totalRevenue += storeRevenue;
                totalOrders += storeOrders.Count;

                foreach (var o in storeOrders.Where(o => o.CreatedAt.Year == DateTime.Now.Year))
                    monthlyOrders[o.CreatedAt.Month - 1]++;

                storesWithStats.Add(new
                {
                    id = s.IdStore,
                    name = s.Name ?? "",
                    city = s.City ?? "",
                    model = "Subscription",
                    products = storeProducts.Count,
                    orders = storeOrders.Count,
                    revenue = storeRevenue,
                    rating = s.Rating ?? 0m,
                    status = s.VerificationStatus == "Approved" ? "active"
                             : s.VerificationStatus == "Pending" ? "pending"
                             : s.VerificationStatus == "Suspended" ? "suspended"
                             : "rejected"
                });
            }

            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.TotalOrders = totalOrders;
            ViewBag.TotalStores = stores.Count;
            ViewBag.ActiveStores = stores.Count(s => s.VerificationStatus == "Approved");
            ViewBag.PendingStores = stores.Count(s => s.VerificationStatus == "Pending");
            ViewBag.SuspendedStores = stores.Count(s => s.VerificationStatus == "Suspended");
            ViewBag.StoresJson = JsonSerializer.Serialize(storesWithStats);
            ViewBag.MonthlyOrdersJson = JsonSerializer.Serialize(monthlyOrders);

            return View();
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
        // UpdateDangerStatus — POST
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
        // Donations — GET
        // ════════════════════════════════════════════════════════
        public async Task<IActionResult> Donations()
        {
            if (!IsAdmin()) return NotAdmin();
            SetAdminViewBag();

            var donations = (await _donationRepo.GetAllWithDonorAsync()).ToList();
            var animals = (await _donationAnimalRepo.GetAllAsync()).ToList();

            ViewBag.Donations = donations;
            ViewBag.Animals = animals;
            ViewBag.TotalDonations = donations.Count;
            ViewBag.PendingDonations = donations.Count(d => d.Status == "Pending");
            ViewBag.CompletedDonations = donations.Count(d => d.Status == "Completed");
            ViewBag.ActiveAnimals = animals.Count(a => a.Status == "Active");

            if (TempData["DonationSuccess"] != null) ViewBag.Success = TempData["DonationSuccess"];
            if (TempData["DonationError"] != null) ViewBag.Error = TempData["DonationError"];

            return View();
        }

        // ════════════════════════════════════════════════════════
        // AddDonationAnimal — POST
        // ════════════════════════════════════════════════════════
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddDonationAnimal(
            string name, string animalType, string condition,
            string? location, string story, decimal? goalAmount)
        {
            if (!IsAdmin()) return NotAdmin();

            await _donationAnimalRepo.CreateAsync(new DonationAnimal
            {
                Name = name,
                AnimalType = animalType,
                Condition = condition,
                Location = location,
                Story = story,
                GoalAmount = goalAmount,
                Status = "Active",
                CreatedAt = DateTime.UtcNow
            });

            TempData["DonationSuccess"] = $"✅ {name} added successfully.";
            return RedirectToAction("Donations");
        }

        // ════════════════════════════════════════════════════════
        // UpdateDonationAnimalStatus — POST
        // ════════════════════════════════════════════════════════
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateDonationAnimalStatus(int animalId, string status)
        {
            if (!IsAdmin()) return NotAdmin();
            await _donationAnimalRepo.UpdateStatusAsync(animalId, status);
            TempData["DonationSuccess"] = $"✅ Animal status updated to {status}.";
            return RedirectToAction("Donations");
        }

        // ════════════════════════════════════════════════════════
        // DeleteDonationAnimal — POST
        // ════════════════════════════════════════════════════════
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteDonationAnimal(int animalId)
        {
            if (!IsAdmin()) return NotAdmin();
            await _donationAnimalRepo.DeleteAsync(animalId);
            TempData["DonationSuccess"] = "🗑 Animal removed.";
            return RedirectToAction("Donations");
        }

        // ════════════════════════════════════════════════════════
        // UpdateDonationStatus — POST
        // ════════════════════════════════════════════════════════
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateDonationStatus(int donationId, string status)
        {
            if (!IsAdmin()) return NotAdmin();
            await _donationRepo.UpdateStatusAsync(donationId, status);
            TempData["DonationSuccess"] = $"✅ Donation marked as {status}.";
            return RedirectToAction("Donations");
        }

        // ════════════════════════════════════════════════════════
        // Reviews_Complaints_C — GET  (from v1: real data)
        // ════════════════════════════════════════════════════════
        public async Task<IActionResult> Reviews_Complaints_C()
        {
            if (!IsAdmin()) return NotAdmin();
            SetAdminViewBag();

            var complaints = (await _complaintRepo.GetAllByTargetTypeAsync("Clinic")).ToList();

            ViewBag.Complaints = complaints;
            ViewBag.TotalComplaints = complaints.Count;
            ViewBag.OpenComplaints = complaints.Count(c => c.Status == "Open" || c.Status == "Pending");
            ViewBag.ResolvedComplaints = complaints.Count(c => c.Status == "Resolved");

            if (TempData["ComplaintSuccess"] != null) ViewBag.Success = TempData["ComplaintSuccess"];

            return View();
        }

        // ════════════════════════════════════════════════════════
        // Reviews_Complaints_S — GET  (from v1: real data)
        // ════════════════════════════════════════════════════════
        public async Task<IActionResult> Reviews_Complaints_S()
        {
            if (!IsAdmin()) return NotAdmin();
            SetAdminViewBag();

            var complaints = (await _complaintRepo.GetAllByTargetTypeAsync("Store")).ToList();

            ViewBag.Complaints = complaints;
            ViewBag.TotalComplaints = complaints.Count;
            ViewBag.OpenComplaints = complaints.Count(c => c.Status == "Open" || c.Status == "Pending");
            ViewBag.ResolvedComplaints = complaints.Count(c => c.Status == "Resolved");

            if (TempData["ComplaintSuccess"] != null) ViewBag.Success = TempData["ComplaintSuccess"];

            return View();
        }

        // ════════════════════════════════════════════════════════
        // UpdateComplaintStatus — POST (shared for both)
        // ════════════════════════════════════════════════════════
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateComplaintStatus(
            int complaintId, string status, string returnPage = "Reviews_Complaints_S")
        {
            if (!IsAdmin()) return NotAdmin();
            await _complaintRepo.UpdateStatusAsync(complaintId, status);
            TempData["ComplaintSuccess"] = $"✅ Complaint status updated to {status}.";
            return RedirectToAction(returnPage);
        }

        // ════════════════════════════════════════════════════════
        // Laws_Regulations — GET  (from v2)
        // ════════════════════════════════════════════════════════
        public async Task<IActionResult> Laws_Regulations()
        {
            if (!IsAdmin()) return NotAdmin();
            SetAdminViewBag();

            var laws = (await _lawRepo.GetAllAsync()).ToList();

            ViewBag.TotalLaws = laws.Count;
            ViewBag.UpdatedThisYear = laws.Count(l => l.LastUpdated.Year == DateTime.Now.Year);
            ViewBag.TotalCategories = laws.Select(l => l.Category).Distinct().Count();
            ViewBag.LatestUpdate = laws.Any()
                ? laws.Max(l => l.LastUpdated).ToString("MMM yyyy")
                : "—";

            ViewBag.LawsJson = JsonSerializer.Serialize(laws.Select(l => new
            {
                id = l.Id,
                cat = l.Category,
                catLabel = l.CategoryLabel,
                title = l.Title,
                body = l.Summary,
                source = l.Source ?? "—",
                updated = l.LastUpdated.ToString("yyyy-MM-dd")
            }));

            if (TempData["Toast"] != null) ViewBag.Toast = TempData["Toast"];
            return View();
        }

        // ════════════════════════════════════════════════════════
        // AddLaw — POST
        // ════════════════════════════════════════════════════════
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddLaw(
            string title, string category, string summary,
            string? source, string? lastUpdated)
        {
            if (!IsAdmin()) return NotAdmin();
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(category) || string.IsNullOrWhiteSpace(summary))
            {
                TempData["Toast"] = "⚠️ Title, Category and Summary are required.";
                return RedirectToAction(nameof(Laws_Regulations));
            }

            var law = new PetLaw
            {
                Title = title.Trim(),
                Category = category,
                CategoryLabel = GetCategoryLabel(category),
                Summary = summary.Trim(),
                Source = string.IsNullOrWhiteSpace(source) ? "—" : source.Trim(),
                LastUpdated = DateTime.TryParse(lastUpdated, out var d) ? d : DateTime.Today
            };

            await _lawRepo.AddAsync(law);
            TempData["Toast"] = "✅ Law added successfully!";
            return RedirectToAction(nameof(Laws_Regulations));
        }

        // ════════════════════════════════════════════════════════
        // EditLaw — POST
        // ════════════════════════════════════════════════════════
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditLaw(
            int id, string title, string category, string summary,
            string? source, string? lastUpdated)
        {
            if (!IsAdmin()) return NotAdmin();
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(category) || string.IsNullOrWhiteSpace(summary))
            {
                TempData["Toast"] = "⚠️ Title, Category and Summary are required.";
                return RedirectToAction(nameof(Laws_Regulations));
            }

            var law = new PetLaw
            {
                Id = id,
                Title = title.Trim(),
                Category = category,
                CategoryLabel = GetCategoryLabel(category),
                Summary = summary.Trim(),
                Source = string.IsNullOrWhiteSpace(source) ? "—" : source.Trim(),
                LastUpdated = DateTime.TryParse(lastUpdated, out var d) ? d : DateTime.Today
            };

            bool ok = await _lawRepo.UpdateAsync(law);
            TempData["Toast"] = ok ? "✅ Law updated!" : "⚠️ Law not found.";
            return RedirectToAction(nameof(Laws_Regulations));
        }

        // ════════════════════════════════════════════════════════
        // DeleteLaw — POST
        // ════════════════════════════════════════════════════════
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteLaw(int id)
        {
            if (!IsAdmin()) return NotAdmin();
            bool ok = await _lawRepo.DeleteAsync(id);
            TempData["Toast"] = ok ? "🗑 Law deleted." : "⚠️ Law not found.";
            return RedirectToAction(nameof(Laws_Regulations));
        }


        // ════════════════════════════════════════════════════
        // System_Complaints — GET
        // ════════════════════════════════════════════════════
        public async Task<IActionResult> System_Complaints()
        {
            if (!IsAdmin()) return NotAdmin();
            SetAdminViewBag();

            var complaints = (await _complaintRepo.GetAllByTargetTypeAsync("System")).ToList();

            ViewBag.Complaints = complaints;
            ViewBag.TotalComplaints = complaints.Count;
            ViewBag.OpenComplaints = complaints.Count(c => c.Status == "Open" || c.Status == "Pending");
            ViewBag.ResolvedComplaints = complaints.Count(c => c.Status == "Resolved");
            ViewBag.InvestigatingCount = complaints.Count(c => c.Status == "Investigating");

            if (TempData["ComplaintSuccess"] != null) ViewBag.Success = TempData["ComplaintSuccess"];

            return View();
        }

        // ════════════════════════════════════════════════════════
        // LISTINGS — GET
        // ════════════════════════════════════════════════════════
        public async Task<IActionResult> Listings(
            string statusFilter = "All", string sourceFilter = "All")
        {
            if (!IsAdmin()) return NotAdmin();
            SetAdminViewBag();

            var allPosts = (await _adoptionRepo.GetAllAsync()).ToList();

            var vm = new SuperAdminAdoptionViewModel
            {
                TotalCount = allPosts.Count,
                PendingCount = allPosts.Count(p => p.Status == "Pending"),
                ApprovedCount = allPosts.Count(p => p.Status == "Approved"),
                ClosedCount = allPosts.Count(p => p.Status == "Closed"),
                RejectedCount = allPosts.Count(p => p.Status == "Rejected"),
                StatusFilter = statusFilter,
                SourceFilter = sourceFilter
            };

            var filtered = allPosts.AsEnumerable();
            if (statusFilter != "All") filtered = filtered.Where(p => p.Status == statusFilter);
            if (sourceFilter != "All") filtered = filtered.Where(p => p.PostedByType == sourceFilter);
            vm.Posts = filtered.ToList();

            if (TempData["ListingSuccess"] != null) ViewBag.Success = TempData["ListingSuccess"];
            if (TempData["ListingError"] != null) ViewBag.Error = TempData["ListingError"];

            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveListing(
            int id, string statusFilter = "All", string sourceFilter = "All")
        {
            if (!IsAdmin()) return NotAdmin();
            await _adoptionRepo.ApproveAsync(id);
            TempData["ListingSuccess"] = "✅ Post approved and published.";
            return RedirectToAction("Listings", new { statusFilter, sourceFilter });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectListing(
            int id, string statusFilter = "All", string sourceFilter = "All")
        {
            if (!IsAdmin()) return NotAdmin();
            await _adoptionRepo.RejectAsync(id);
            TempData["ListingSuccess"] = "❌ Post rejected.";
            return RedirectToAction("Listings", new { statusFilter, sourceFilter });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CloseListing(
            int id, string statusFilter = "All", string sourceFilter = "All")
        {
            if (!IsAdmin()) return NotAdmin();
            await _adoptionRepo.ClosePostAsync(id);
            TempData["ListingSuccess"] = "🔒 Post closed.";
            return RedirectToAction("Listings", new { statusFilter, sourceFilter });
        }

        // ════════════════════════════════════════════════════════
        // ADOPTED PETS — GET (مراقبة فقط)
        // ════════════════════════════════════════════════════════
        public async Task<IActionResult> Adopted_Pets(string statusFilter = "All")
        {
            if (!IsAdmin()) return NotAdmin();
            SetAdminViewBag();

            var all = (await _reservationRepo.GetAllWithDetailsAsync()).ToList();

            var vm = new SuperAdminReservationViewModel
            {
                TotalCount = all.Count,
                PendingCount = all.Count(r => r.Status == "Pending"),
                ApprovedCount = all.Count(r => r.Status == "Approved"),
                RejectedCount = all.Count(r => r.Status == "Rejected"),
                CancelledCount = all.Count(r => r.Status == "Cancelled"),
                StatusFilter = statusFilter,
                Reservations = statusFilter == "All"
                    ? all
                    : all.Where(r => r.Status == statusFilter).ToList()
            };

            return View(vm);
        }

        // ════════════════════════════════════════════════════════
        // FLAGGED — GET
        // ════════════════════════════════════════════════════════
        public async Task<IActionResult> Flagged(string reasonFilter = "All")
        {
            if (!IsAdmin()) return NotAdmin();
            SetAdminViewBag();

            var allFlagged = (await _flagRepo.GetAllFlaggedPostsAsync()).ToList();
            var filtered = reasonFilter == "All"
                ? allFlagged
                : allFlagged.Where(f => f.TopReason == reasonFilter).ToList();

            var items = new List<FlaggedPostWithReporters>();
            foreach (var post in filtered)
            {
                var reporters = await _flagRepo.GetFlagsByPostIdAsync(post.IdPost);
                var actions = await _flagRepo.GetActionsByPostIdAsync(post.IdPost);
                items.Add(new FlaggedPostWithReporters
                {
                    Post = post,
                    Reporters = reporters,
                    Actions = actions
                });
            }

            // History مع التفاصيل الكاملة
            var historyPosts = (await _flagRepo.GetFlagHistoryAsync()).ToList();
            var historyItems = new List<FlaggedPostWithReporters>();
            foreach (var h in historyPosts)
            {
                var reporters = await _flagRepo.GetFlagsByPostIdAsync(h.IdPost);
                var actions = await _flagRepo.GetActionsByPostIdAsync(h.IdPost);
                historyItems.Add(new FlaggedPostWithReporters
                {
                    Post = h,
                    Reporters = reporters,
                    Actions = actions
                });
            }

            ViewBag.ReasonFilter = reasonFilter;
            ViewBag.HistoryItems = historyItems;

            if (TempData["FlagSuccess"] != null) ViewBag.Success = TempData["FlagSuccess"];

            return View(items);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ClearFlags(int postId)
        {
            if (!IsAdmin()) return NotAdmin();
            var adminId = HttpContext.Session.GetInt32("UserId") ?? 0;

            await _flagRepo.LogActionAsync(postId, adminId,
                "Cleared", null, null, "No violation found — flags cleared");
            await _flagRepo.MarkAsReviewedAsync(postId, adminId,
                "Cleared", "No violation found");

            TempData["FlagSuccess"] = "✅ Flags reviewed and cleared.";
            return RedirectToAction("Flagged");
        }
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> FlagClosePost(int postId)
        {
            if (!IsAdmin()) return NotAdmin();
            var adminId = HttpContext.Session.GetInt32("UserId") ?? 0;

            await _flagRepo.LogActionAsync(postId, adminId,
                "Closed", null, null, "Post closed due to policy violation");
            await _flagRepo.MarkAsReviewedAsync(postId, adminId,
                "Closed", "Post closed due to policy violation");
            await _adoptionRepo.ClosePostAsync(postId);

            TempData["FlagSuccess"] = "🔒 Post closed and flags reviewed.";
            return RedirectToAction("Flagged");
        }
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SendWarning(
    int postId, int targetUserId, string targetName,
    string warningMessage, string adminAction)
        {
            if (!IsAdmin()) return NotAdmin();
            var adminId = HttpContext.Session.GetInt32("UserId") ?? 0;

            // سجّل العملية فقط — لا تعلّم reviewed
            await _flagRepo.LogActionAsync(
                postId, adminId, adminAction,
                targetUserId, targetName, warningMessage);

            // أرسل الإشعار
            await _notifRepo.AddAsync(new Notification
            {
                IdUser = targetUserId,
                Type = "Warning",
                Title = "⚠️ Warning from Waffy Admin",
                Message = warningMessage,
                IsRead = false,
                RelatedEntityType = "AdoptionPost",
                RelatedEntityId = postId,
                CreatedAt = DateTime.UtcNow
            });

            TempData["FlagSuccess"] = $"✅ Warning sent to {targetName}.";
            return RedirectToAction("Flagged");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> RevertDecision(int postId)
        {
            if (!IsAdmin()) return NotAdmin();
            var adminId = HttpContext.Session.GetInt32("UserId") ?? 0;

            await _flagRepo.LogActionAsync(postId, adminId,
                "DecisionReverted", null, null, "Decision reverted by admin");
            await _flagRepo.RevertReviewAsync(postId);

            TempData["FlagSuccess"] = "↩️ Decision reverted. Post moved back to active flags.";
            return RedirectToAction("Flagged");
        }

        // ════════════════════════════════════════════════════════
        // Stubs
        // ════════════════════════════════════════════════════════
        public IActionResult Clinics() { if (!IsAdmin()) return NotAdmin(); SetAdminViewBag(); return View(); }
        public IActionResult Details_C() { if (!IsAdmin()) return NotAdmin(); SetAdminViewBag(); return View(); }
        public IActionResult Analytics_C() { if (!IsAdmin()) return NotAdmin(); SetAdminViewBag(); return View(); }
        public IActionResult Analytics_A() { if (!IsAdmin()) return NotAdmin(); SetAdminViewBag(); return View(); }
        public IActionResult Lost_Pets() { if (!IsAdmin()) return NotAdmin(); SetAdminViewBag(); return View(); }
        public IActionResult Found_Pets() { if (!IsAdmin()) return NotAdmin(); SetAdminViewBag(); return View(); }
        public IActionResult Map() { if (!IsAdmin()) return NotAdmin(); SetAdminViewBag(); return View(); }
        public IActionResult Notifications() { if (!IsAdmin()) return NotAdmin(); SetAdminViewBag(); return View(); }
    }
}