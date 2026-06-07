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
        private readonly IOrderItemRepository _orderItemRepo;     
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
        private readonly ISurpriseRewardRepository _surpriseRepo;
        private readonly IPendingPaymentRepository _paymentRepo;
        private readonly ILostPetRepository _lostRepo;
        private readonly IFoundPetClaimRepository _foundClaimRepo;
        private readonly IRewardLogRepository _rewardLogRepo;
        private readonly IWalletRepository _walletRepo;
        private readonly IClinicRepository _clinicRepo;
        private readonly IPetRepository _petRepo;
        private readonly IProductImageRepository _productImageRepo;
        private readonly IClinicDocumentRepository _clinicDocumentRepo;
        private readonly IDoctorRepository _doctorAdminRepo;
        private readonly IAppointmentRepository _apptRepo;
        private readonly IReviewRepository _reviewRepo;
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
            INotificationRepository notifRepo,
            ISurpriseRewardRepository surpriseRepo,
            ILostPetRepository lostRepo,
            IFoundPetClaimRepository foundClaimRepo,
            IRewardLogRepository rewardLogRepo,
            IWalletRepository walletRepo,
            IClinicRepository clinicRepo,
            IPetRepository petRepo,
            IProductImageRepository productImageRepo,
            IClinicDocumentRepository clinicDocumentRepo,
            IDoctorRepository doctorAdminRepo,
            IPendingPaymentRepository paymentRepo,
            IReviewRepository reviewRepo,
            IAppointmentRepository apptRepo)  
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
            _surpriseRepo = surpriseRepo;
            _paymentRepo = paymentRepo;
            _lostRepo = lostRepo;
            _foundClaimRepo = foundClaimRepo;
            _rewardLogRepo = rewardLogRepo;
            _walletRepo = walletRepo;
            _clinicRepo = clinicRepo;
            _petRepo = petRepo;
            _productImageRepo = productImageRepo;
            _clinicDocumentRepo = clinicDocumentRepo;
            _doctorAdminRepo = doctorAdminRepo;
            _apptRepo = apptRepo;
            _reviewRepo = reviewRepo;
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

            var pets = (await _petRepo.GetByUserIdAsync(id)).ToList();
            var lostReports = (await _lostRepo.GetByUserIdAsync(id)).ToList();
            var adoptionPosts = (await _adoptionRepo.GetByUserIdAsync(id)).ToList();
            var surpriseRewards = (await _rewardLogRepo.GetByUserIdAsync(id)).ToList();
            var wallet = await _walletRepo.GetByUserIdAsync(id);
            var transactions = wallet != null
                                  ? (await _walletRepo.GetTransactionsByUserIdAsync(id)).ToList()
                                  : new List<WalletTransaction>();
            var orders = (await _orderRepo.GetByUserIdAsync(id)).ToList();
            var postIds = adoptionPosts.Select(p => p.IdPost);
            var flagCounts = await _flagRepo.GetFlagCountsForPostsAsync(postIds);
            var totalFlags = flagCounts.Values.Sum();
            var complaints = (await _complaintRepo.GetByUserIdAsync(id)).ToList();
            var messageHistory = (await _notifRepo.GetByUserIdAsync(id))
    .Where(n => n.RelatedEntityType == "AdminMessage"
             || n.RelatedEntityType == "AdminAction")
    .OrderByDescending(n => n.CreatedAt)
    .ToList();

            // ── أسماء المتاجر ─────────────────────────────────
            var storeNames = new Dictionary<int, string>();
            foreach (var o in orders)
            {
                if (!storeNames.ContainsKey(o.IdStore))
                {
                    var store = await _storeRepo.GetByIdAsync(o.IdStore);
                    storeNames[o.IdStore] = store?.Name ?? "—";
                }
            }
            ViewBag.StoreNames = storeNames;

            var vm = new UserDetailViewModel
            {
                User = user,
                Pets = pets,
                LostReports = lostReports,
                AdoptionPosts = adoptionPosts,
                SurpriseRewards = surpriseRewards,
                Wallet = wallet,
                Transactions = transactions,
                Orders = orders,
                PetsCount = pets.Count,
                LostReportsCount = lostReports.Count,
                AdoptionCount = adoptionPosts.Count,
                CouponsCount = surpriseRewards.Count,
                OrdersCount = orders.Count,
                ReturnSearch = returnSearch,
                ReturnStatus = returnStatus,
                ReturnRole = returnRole,
                TotalFlagsCount = totalFlags,
                Complaints = complaints,
                ComplaintsCount = complaints.Count,
                MessageHistory = messageHistory,
                ReturnPage = returnPage
            };

            SetAdminViewBag();
            if (TempData["SuccessMessage"] != null) ViewBag.Success = TempData["SuccessMessage"];
            if (TempData["ErrorMessage"] != null) ViewBag.Error = TempData["ErrorMessage"];

            return View(vm);
        }






        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> WarnUser(
    int userId, string warningType, string message,
    string? returnSearch, string? returnStatus,
    string? returnRole, int returnPage = 1)
        {
            if (!IsAdmin()) return NotAdmin();

            // ── إرسال إشعار حقيقي للمستخدم ──────────────────
            await _notifRepo.AddAsync(new Notification
            {
                IdUser = userId,
                Type = "Warning",
                Title = $"⚠️ {warningType}",
                Message = message,
                IsRead = false,
                RelatedEntityType = "AdminAction",
                CreatedAt = DateTime.UtcNow
            });

            // ── زيادة عداد التحذيرات ──────────────────────────
            await _userRepo.IncrementWarningCountAsync(userId);

            TempData["SuccessMessage"] = "⚠️ Warning sent successfully.";
            return RedirectToAction("Details_U", new
            {
                id = userId,
                returnSearch,
                returnStatus,
                returnRole,
                returnPage
            });
        }



        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMessage(
    int userId, string msgType, string title,
    string message, string priority,
    string? returnSearch, string? returnStatus,
    string? returnRole, int returnPage = 1)
        {
            if (!IsAdmin()) return NotAdmin();

            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(message))
            {
                TempData["ErrorMessage"] = "⚠️ Please fill in title and message.";
                return RedirectToAction("Details_U", new { id = userId, returnSearch, returnStatus, returnRole, returnPage });
            }

            await _notifRepo.AddAsync(new Notification
            {
                IdUser = userId,
                Type = msgType,
                Title = title,
                Message = message,
                IsRead = false,
                RelatedEntityType = "AdminMessage",
                CreatedAt = DateTime.UtcNow
            });

            TempData["SuccessMessage"] = $"✅ Message sent successfully.";
            return RedirectToAction("Details_U", new
            {
                id = userId,
                returnSearch,
                returnStatus,
                returnRole,
                returnPage
            });
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
            var allStores = (await _storeRepo.GetAllAsync()).ToList();

            int total = allStores.Count;
            int active = allStores.Count(s => s.VerificationStatus == "Approved");
            int pending = allStores.Count(s => s.VerificationStatus == "Pending");
            int suspended = allStores.Count(s => s.VerificationStatus == "Suspended");

            var list = new List<object>();
            foreach (var s in allStores)
            {
                var owner = await _userRepo.GetByIdAsync(s.OwnerUserId);
                var products = (await _productRepo.GetByStoreIdAsync(s.IdStore)).ToList();
                var orders = (await _orderRepo.GetByStoreIdAsync(s.IdStore)).ToList();
                var revenue = await _orderRepo.GetTotalRevenueByStoreAsync(s.IdStore);

                var status = s.VerificationStatus switch
                {
                    "Approved" => "active",
                    "Pending" => "pending",
                    "Suspended" => "suspended",
                    "Rejected" => "rejected",
                    _ => "pending"
                };

                list.Add(new
                {
                    id = s.IdStore,
                    name = s.Name ?? "—",
                    owner = owner?.FullName ?? "—",
                    email = owner?.Email ?? "—",
                    phone = owner?.Phone ?? "—",
                    city = s.City ?? "—",
                    logoUrl = s.LogoUrl ?? "",
                    products = products.Count,
                    orders = orders.Count,
                    revenue = revenue.Items + revenue.Delivery,
                    rating = 0,
                    status,
                    joined = s.CreatedAt.ToString("MMM dd, yyyy")
                });
            }

            ViewBag.TotalStores = total;
            ViewBag.ActiveStores = active;
            ViewBag.PendingStores = pending;
            ViewBag.SuspendedStores = suspended;
            ViewBag.StoresJson = JsonSerializer.Serialize(list);

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

            var owner = await _userRepo.GetByIdAsync(store.OwnerUserId);
            var products = (await _productRepo.GetByStoreIdAsync(id)).ToList();
            var orders = (await _orderRepo.GetByStoreIdAsync(id)).ToList();
            var revenue = await _orderRepo.GetTotalRevenueByStoreAsync(id);
            var docs = (await _docRepo.GetByStoreIdAsync(id)).ToList();

            // ── Store Data ──
            var status = store.VerificationStatus switch
            {
                "Approved" => "active",
                "Pending" => "pending",
                "Suspended" => "suspended",
                "Rejected" => "rejected",
                _ => "pending"
            };

            var storeData = new
            {
                id = store.IdStore,
                name = store.Name,
                logoUrl = store.LogoUrl ?? "",
                owner = owner?.FullName ?? "—",
                email = owner?.Email ?? "—",
                phone = owner?.Phone ?? "—",
                city = store.City ?? "—",
                status,
                joined = store.CreatedAt.ToString("MMM dd, yyyy"),
                products = products.Count,
                orders = orders.Count,
                revenue = revenue.Items + revenue.Delivery,
                rating = 0,
                deliveryAvailable = store.DeliveryFee > 0,
                description = store.Description ?? ""
            };

            // ── Products ──
            var productsList = new List<object>();
            foreach (var p in products)
            {
                var imgs = (await _productImageRepo.GetByProductIdAsync(p.Id)).ToList();
                var img = imgs.FirstOrDefault(i => i.IsMain)?.ImageUrl
                        ?? imgs.FirstOrDefault()?.ImageUrl ?? "";

                productsList.Add(new
                {
                    id = p.Id,
                    name = p.Name,
                    price = p.SalePrice ?? p.Price,
                    stock = p.StockQuantity,
                    status = !p.IsVisible ? "hidden" : p.StockQuantity == 0 ? "out" : "active",
                    image = img,
                    images = imgs.Select(i => i.ImageUrl).Where(u => !string.IsNullOrEmpty(u)).ToList()
                });
            }

            // ── Orders ──
            var ordersList = new List<object>();
            foreach (var o in orders)
            {
                var customer = await _userRepo.GetByIdAsync(o.IdUser);
                var orderItems = (await _orderItemRepo.GetByOrderIdAsync(o.IdOrder)).ToList();
                var items = orderItems.Select(i => i.ProductNameSnapshot).ToList();
                var itemsTotal = orderItems.Sum(i => i.LineTotal);

                ordersList.Add(new
                {
                    id = o.IdOrder,
                    orderCode = o.OrderCode ?? $"ORD-{o.IdOrder}",
                    cust = customer?.FullName ?? "—",
                    phone = customer?.Phone ?? "—",
                    items,
                    itemsTotal,
                    deliveryFee = o.DeliveryFee,
                    total = itemsTotal + o.DeliveryFee,
                    date = o.CreatedAt.ToString("MMM dd, yyyy"),
                    status = o.Status?.ToLower() ?? "pending"
                });
            }

            // ── Documents ──
            var docsList = docs.Select(d => new {
                id = d.Id,
                label = d.DocumentType ?? "Document",
                fileUrl = d.FileUrl ?? "",
                isImage = (d.FileUrl ?? "").Contains(".jpg") || (d.FileUrl ?? "").Contains(".png") || (d.FileUrl ?? "").Contains(".jpeg") || (d.FileUrl ?? "").Contains(".webp"),
                status = d.Status?.ToLower() ?? "pending",
                uploadedAt = d.UploadedAt.ToString("MMM dd, yyyy")
            });

            // ── Customers ──
            var customerMap = orders
                .Where(o => o.IdUser > 0)
                .GroupBy(o => o.IdUser);

            var customersList = new List<object>();
            foreach (var grp in customerMap)
            {
                var cust = await _userRepo.GetByIdAsync(grp.Key);
                var spent = grp.Sum(o => o.DeliveryFee);
                var last = grp.Max(o => o.CreatedAt).ToString("MMM dd, yyyy");
                customersList.Add(new
                {
                    name = cust?.FullName ?? "—",
                    email = cust?.Email ?? "—",
                    phone = cust?.Phone ?? "—",
                    orders = grp.Count(),
                    spent,
                    last
                });
            }

            ViewBag.StoreId = id;
            ViewBag.StoreName = store.Name;
            ViewBag.StoreDataJson = JsonSerializer.Serialize(storeData);
            ViewBag.ProductsJson = JsonSerializer.Serialize(productsList);
            ViewBag.OrdersJson = JsonSerializer.Serialize(ordersList);
            ViewBag.DocsJson = JsonSerializer.Serialize(docsList);
            ViewBag.CustomersJson = JsonSerializer.Serialize(customersList);

            return View();
        }

        // ════════════════════════════════════════════════════════
        // ApproveStore — POST
        // ════════════════════════════════════════════════════════
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveStore(int storeId, string? from)
        {
            var store = await _storeRepo.GetByIdAsync(storeId);
            if (store == null) return NotFound();

            store.VerificationStatus = "Approved";
            await _storeRepo.UpdateAsync(store);

            await _notifRepo.AddAsync(new Notification
            {
                IdUser = store.OwnerUserId,
                Type = "StoreApproved",
                Title = "✅ Your Store Has Been Approved!",
                Message = $"Congratulations! {store.Name} is now live on Waffy.",
                IsRead = false,
                RelatedEntityType = "Store",
                RelatedEntityId = storeId,
                CreatedAt = DateTime.UtcNow
            });

            ViewBag.Success = $"✅ {store.Name} approved successfully.";
            return from == "details"
                ? RedirectToAction("Details_S", new { id = storeId })
                : RedirectToAction("Stores");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectStore(int storeId, string? reason, string? message, string? from)
        {
            var store = await _storeRepo.GetByIdAsync(storeId);
            if (store == null) return NotFound();

            store.VerificationStatus = "Rejected";
            await _storeRepo.UpdateAsync(store);

            await _notifRepo.AddAsync(new Notification
            {
                IdUser = store.OwnerUserId,
                Type = "StoreRejected",
                Title = "❌ Store Application Rejected",
                Message = $"Your store \"{store.Name}\" was rejected. Reason: {reason}. {message}",
                IsRead = false,
                RelatedEntityType = "Store",
                RelatedEntityId = storeId,
                CreatedAt = DateTime.UtcNow
            });

            ViewBag.Success = $"Store rejected.";
            return from == "details"
                ? RedirectToAction("Details_S", new { id = storeId })
                : RedirectToAction("Stores");
        }


        // ════════════════════════════════════════════════════════
        // SuspendStore — POST
        // ════════════════════════════════════════════════════════
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SuspendStore(int storeId, string? reason, string? duration, string? message, string? from)
        {
            var store = await _storeRepo.GetByIdAsync(storeId);
            if (store == null) return NotFound();

            store.VerificationStatus = "Suspended";
            await _storeRepo.UpdateAsync(store);

            await _notifRepo.AddAsync(new Notification
            {
                IdUser = store.OwnerUserId,
                Type = "StoreSuspended",
                Title = "🔒 Your Store Has Been Suspended",
                Message = $"Store \"{store.Name}\" suspended for {duration}. Reason: {reason}. {message}",
                IsRead = false,
                RelatedEntityType = "Store",
                RelatedEntityId = storeId,
                CreatedAt = DateTime.UtcNow
            });

            return from == "details"
                ? RedirectToAction("Details_S", new { id = storeId })
                : RedirectToAction("Stores");
        }

        // ════════════════════════════════════════════════════════
        // ReactivateStore — POST
        // ════════════════════════════════════════════════════════
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ReactivateStore(int storeId, string? from)
        {
            var store = await _storeRepo.GetByIdAsync(storeId);
            if (store == null) return NotFound();

            store.VerificationStatus = "Approved";
            await _storeRepo.UpdateAsync(store);

            await _notifRepo.AddAsync(new Notification
            {
                IdUser = store.OwnerUserId,
                Type = "StoreReactivated",
                Title = "✅ Your Store Is Active Again",
                Message = $"Your store \"{store.Name}\" has been reactivated.",
                IsRead = false,
                RelatedEntityType = "Store",
                RelatedEntityId = storeId,
                CreatedAt = DateTime.UtcNow
            });

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
            await _storeRepo.DeleteAsync(storeId);
            return RedirectToAction("Stores");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> WarnStore(int storeId, string warnType, string warnMessage)
        {
            var store = await _storeRepo.GetByIdAsync(storeId);
            if (store == null) return Json(new { success = false });

            await _notifRepo.AddAsync(new Notification
            {
                IdUser = store.OwnerUserId,
                Type = "StoreWarning",
                Title = $"⚠️ Warning: {warnType}",
                Message = warnMessage,
                IsRead = false,
                RelatedEntityType = "Store",
                RelatedEntityId = storeId,
                CreatedAt = DateTime.UtcNow
            });

            return Json(new { success = true });
        }



        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveDocument(int docId, int storeId)
        {
            var adminId = HttpContext.Session.GetInt32("UserId") ?? 0;
            await _docRepo.UpdateStatusAsync(docId, "approved", null, adminId);
            return RedirectToAction("Details_S", new { id = storeId });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectDocument(int docId, int storeId)
        {
            var adminId = HttpContext.Session.GetInt32("UserId") ?? 0;
            await _docRepo.UpdateStatusAsync(docId, "rejected", null, adminId);
            return RedirectToAction("Details_S", new { id = storeId });
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
                var revenue = await _orderRepo.GetTotalRevenueByStoreAsync(s.IdStore);
                var storeRevenue = revenue.Items + revenue.Delivery;
               

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
                    rating = 0m,
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

            // ── Complaints (موجود من قبل) ──
            var complaints = (await _complaintRepo.GetAllByTargetTypeAsync("Clinic")).ToList();
            ViewBag.Complaints = complaints;
            ViewBag.TotalComplaints = complaints.Count;
            ViewBag.OpenComplaints = complaints.Count(c => c.Status == "Open" || c.Status == "Pending");
            ViewBag.ResolvedComplaints = complaints.Count(c => c.Status == "Resolved");

            // ── Reviews (جديد) ──
            var clinicReviews = (await _reviewRepo.GetAllAsync("Clinic")).ToList();
            var doctorReviews = (await _reviewRepo.GetAllAsync("Doctor")).ToList();
            var allReviews = clinicReviews.Concat(doctorReviews)
                                              .OrderByDescending(r => r.CreatedAt)
                                              .ToList();

            ViewBag.AllReviews = allReviews;
            ViewBag.ClinicReviews = clinicReviews;
            ViewBag.DoctorReviews = doctorReviews;
            ViewBag.TotalReviews = allReviews.Count;
            ViewBag.AvgRating = allReviews.Any()
                ? Math.Round(allReviews.Average(r => r.Rating), 1) : 0.0;

            if (TempData["ComplaintSuccess"] != null) ViewBag.Success = TempData["ComplaintSuccess"];
            if (TempData["ReviewSuccess"] != null) ViewBag.ReviewSuccess = TempData["ReviewSuccess"];

            return View();
        }

        // ── POST: حذف review ──────────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteReview(int id)
        {
            if (!IsAdmin()) return NotAdmin();
            await _reviewRepo.DeleteAsync(id);
            TempData["ReviewSuccess"] = "✅ Review deleted successfully.";
            return RedirectToAction("Reviews_Complaints_C");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> WarnTarget(
    string targetType, int targetId, string warnType, string warnMessage)
        {
            if (!IsAdmin()) return NotAdmin();

            // حفظ في DB
            await _complaintRepo.WarnAsync(targetType, targetId, warnType, warnMessage);

            // إشعار لصاحب العيادة
            var clinic = await _clinicRepo.GetByIdAsync(targetId);
            if (clinic != null)
            {
                await _notifRepo.AddAsync(new Notification
                {
                    IdUser = clinic.OwnerUserId,
                    Type = "AdminWarning",
                    Title = "⚠️ Official Warning Received",
                    Message = $"You have received an official warning from Waffy Admin. " +
                                        $"Type: {warnType}. {warnMessage}",
                    IsRead = false,
                    RelatedEntityType = "Clinic",
                    RelatedEntityId = targetId,
                    CreatedAt = DateTime.UtcNow
                });
            }

            TempData["ReviewSuccess"] = $"⚠️ Warning sent to {targetType} #{targetId}.";
            return RedirectToAction("Reviews_Complaints_C");
        }

        // ── POST: إيقاف ───────────────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SuspendTarget(
     string targetType, int targetId,
     string suspendReason, string suspendDuration, string suspendMessage)
        {
            if (!IsAdmin()) return NotAdmin();

            await _complaintRepo.SuspendAsync(
                targetType, targetId, suspendReason, suspendDuration, suspendMessage);

            // ✅ أضف هذا — نغيّر VerificationStatus أيضاً
            if (targetType == "Clinic")
            {
                var clinic = await _clinicRepo.GetByIdAsync(targetId);
                if (clinic != null)
                {
                    clinic.VerificationStatus = "Suspended";
                    await _clinicRepo.UpdateAsync(clinic);

                    await _notifRepo.AddAsync(new Notification
                    {
                        IdUser = clinic.OwnerUserId,
                        Type = "AdminSuspension",
                        Title = "🔒 Account Suspended",
                        Message = $"Your clinic has been suspended. Reason: {suspendReason}. Duration: {suspendDuration}. {suspendMessage}",
                        IsRead = false,
                        RelatedEntityType = "Clinic",
                        RelatedEntityId = targetId,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            TempData["ReviewSuccess"] = $"🔒 {targetType} #{targetId} suspended for {suspendDuration}.";
            return RedirectToAction("Reviews_Complaints_C");
        }


        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> LiftSuspension(int clinicId)
        {
            if (!IsAdmin()) return NotAdmin();

            var clinic = await _clinicRepo.GetByIdAsync(clinicId);
            if (clinic == null) return NotFound();

            // رفع الإيقاف
            clinic.IsSuspended = false;
            await _clinicRepo.UpdateAsync(clinic);

            // إشعار لصاحب العيادة
            await _notifRepo.AddAsync(new Notification
            {
                IdUser = clinic.OwnerUserId,
                Type = "AdminAction",
                Title = "✅ Suspension Lifted",
                Message = $"The suspension on your clinic \"{clinic.Name}\" has been lifted by Waffy Admin.",
                IsRead = false,
                RelatedEntityType = "Clinic",
                RelatedEntityId = clinicId,
                CreatedAt = DateTime.UtcNow
            });

            TempData["ClinicSuccess"] = $"✅ Suspension lifted for {clinic.Name}.";
            return RedirectToAction("Details_C", new { id = clinicId });
        }
        // ════════════════════════════════════════════════════════
        // Reviews_Complaints_S — GET  (from v1: real data)
        // ════════════════════════════════════════════════════════
        public async Task<IActionResult> Reviews_Complaints_S()
        {
            if (!IsAdmin()) return NotAdmin();
            SetAdminViewBag();

            var complaints = (await _complaintRepo.GetAllByTargetTypeAsync("Store")).ToList();

            // إضافة اسم المتجر واسم المشتكي من DB
            foreach (var c in complaints)
            {
                if (c.TargetId.HasValue && string.IsNullOrEmpty(c.TargetName))
                {
                    var store = await _storeRepo.GetByIdAsync(c.TargetId.Value);
                    c.TargetName = store?.Name;
                }
                if (c.IdUser.HasValue && c.IdUser > 0 && string.IsNullOrEmpty(c.ComplainantName))
                {
                    var user = await _userRepo.GetByIdAsync(c.IdUser.Value);
                    c.ComplainantName = user?.FullName;
                }
            }

            ViewBag.Complaints = complaints;
            ViewBag.TotalComplaints = complaints.Count;
            ViewBag.OpenComplaints = complaints.Count(c => c.Status == "Open" || c.Status == "Pending");
            ViewBag.InvestigatingCount = complaints.Count(c => c.Status == "Investigating");
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
            // ✅ جلب كل صور كل بوست
            var postImages = new Dictionary<int, List<string>>();
            foreach (var post in allPosts)
            {
                var imgs = (await _adoptionRepo.GetAllImagesAsync(post.IdPost)).ToList();
                postImages[post.IdPost] = imgs;
            }
            ViewBag.PostImages = postImages;
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
        // Lost_Pets — GET
        // ════════════════════════════════════════════════════════
        public async Task<IActionResult> Lost_Pets(string statusFilter = "All")
        {
            if (!IsAdmin()) return NotAdmin();
            SetAdminViewBag();

            // Auto-expire أولاً
            await _lostRepo.ExpireOldReportsAsync();

            // إرسال إشعارات الانتهاء القريب
            var expiringSoon = (await _lostRepo.GetExpiringSoonAsync()).ToList();
            foreach (var rep in expiringSoon)
            {
                await _notifRepo.AddAsync(new Notification
                {
                    IdUser = rep.IdUser,
                    Type = "LostPetExpiry",
                    Title = "⚠️ Your lost pet report is expiring soon",
                    Message = $"Your report for {rep.PetName} expires in 3 days. Close it if found, or it will expire automatically.",
                    IsRead = false,
                    RelatedEntityType = "LostPetReport",
                    RelatedEntityId = rep.IdLostReport,
                    CreatedAt = DateTime.UtcNow
                });
                await _lostRepo.MarkExpiryNotifiedAsync(rep.IdLostReport);
            }

            var allReports = (await _lostRepo.GetAllAsync()).ToList();
            var pendingPayments = (await _paymentRepo.GetAllPendingAsync()).ToList();

            // ── فلتر "No Reward + Confirmed Finder" ──────────────────
            IEnumerable<LostPetAdminDto> filtered;
            if (statusFilter == "NoRewardConfirmed")
            {
                // بوستات Found + بدون مكافأة + يوجد Finder مؤكد
                filtered = allReports.Where(r =>
                    r.Status == "Found" &&
                    (r.RewardAmount == null || r.RewardAmount == 0) &&
                    r.ConfirmedClaimCount > 0);
            }
            else if (statusFilter == "All")
            {
                filtered = allReports;
            }
            else
            {
                filtered = allReports.Where(r => r.Status == statusFilter);
            }

            var vm = new SuperAdminLostViewModel
            {
                Reports = filtered.ToList(),
                TotalCount = allReports.Count,
                ActiveCount = allReports.Count(r => r.Status == "Active"),
                FoundCount = allReports.Count(r => r.Status == "Found"),
                ExpiredCount = allReports.Count(r => r.Status == "Expired"),
                PendingPaymentCount = allReports.Count(r => !r.IsPaid),
                NoRewardConfirmedCount = allReports.Count(r =>          
                    r.Status == "Found" &&
                    (r.RewardAmount == null || r.RewardAmount == 0) &&
                    r.ConfirmedClaimCount > 0),
                StatusFilter = statusFilter
            };

            var approvedStores = (await _storeRepo.GetAllAsync())
    .Where(s => s.VerificationStatus == "Approved")
    .Select(s => new { id = s.IdStore, name = s.Name ?? "" })
    .ToList();

            var approvedClinics = (await _clinicRepo.GetAllAsync())
                .Where(c => c.VerificationStatus == "Approved")
                .Select(c => new { id = c.IdClinic, name = c.Name ?? "" })
                .ToList();

            ViewBag.ApprovedStores = approvedStores;
            ViewBag.ApprovedClinics = approvedClinics;

            ViewBag.PendingPayments = pendingPayments;
            ViewBag.SurprisePool = (await _surpriseRepo.GetAllAsync()).ToList();
            // ✅ جلب كل صور كل تقرير
            var reportImages = new Dictionary<int, List<string>>();
            foreach (var r in allReports)
            {
                var imgs = (await _lostRepo.GetAllImagesAsync(r.IdLostReport)).ToList();
                reportImages[r.IdLostReport] = imgs;
            }
            ViewBag.ReportImages = reportImages;
            if (TempData["LostSuccess"] != null) ViewBag.Success = TempData["LostSuccess"];
            if (TempData["LostError"] != null) ViewBag.Error = TempData["LostError"];

            return View(vm);
        }

        // ══════════════════════════════════════════════════════════
        // ── 2. DeliverRewardAsync — تسليم تلقائي حسب النوع ────────
        // ══════════════════════════════════════════════════════════
        private async Task DeliverRewardAsync(
            SurpriseRewardPool reward,
            int recipientId,
            string recipientName,
            string reasonCategory)
        {
            string title, message;

            switch (reward.RewardType)
            {
                case "WalletCredit":
                    // ✅ أضف للمحفظة مباشرة
                    var wallet = await _walletRepo.GetByUserIdAsync(recipientId);
                    if (wallet == null)
                    {
                        // أنشئ محفظة إذا لم تكن موجودة
                        await _walletRepo.CreateWalletAsync(recipientId);
                        wallet = await _walletRepo.GetByUserIdAsync(recipientId);
                    }
                    if (wallet != null && reward.Value.HasValue)
                    {
                        await _walletRepo.AddBalanceAsync(wallet.IdWallet, reward.Value.Value);
                        await _walletRepo.AddTransactionAsync(new WalletTransaction
                        {
                            IdWallet = wallet.IdWallet,
                            TransactionCode = $"GIFT-{Guid.NewGuid().ToString()[..8].ToUpper()}",
                            Type = "SurpriseGift",
                            Amount = reward.Value.Value,
                            PaymentMethod = "Waffy Gift",
                            Status = "Completed",
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                    title = "🎁 You received a Waffy gift!";
                    message = $"{reward.Value:F2} JOD has been added to your wallet. " +
                              $"Reason: {reasonCategory}";
                    break;

                case "StoreDiscount":
                    title = "🏪 You received a store discount!";
                    message = $"{reward.Title}" +
                              (reward.PartnerName != null ? $" at {reward.PartnerName}" : "") +
                              (reward.DiscountPercent.HasValue ? $" — {reward.DiscountPercent}% off" : "") +
                              (reward.CouponCode != null ? $" | Code: {reward.CouponCode}" : "") +
                              $" | Reason: {reasonCategory}";
                    break;

                case "DeliveryVoucher":
                    title = "🚚 You received a delivery voucher!";
                    message = $"{reward.Title}" +
                              (reward.CouponCode != null ? $" | Code: {reward.CouponCode}" : "") +
                              $" | Reason: {reasonCategory}";
                    break;

                case "ClinicSession":
                    title = "🏥 You received a free clinic session!";
                    message = $"{reward.Title}" +
                              (reward.PartnerName != null ? $" | Contact {reward.PartnerName} to book." : "") +
                              $" | Reason: {reasonCategory}";
                    break;

                default:
                    title = "🎁 You received a special reward!";
                    message = $"{reward.Title} | Reason: {reasonCategory}";
                    break;
            }

            await _notifRepo.AddAsync(new Notification
            {
                IdUser = recipientId,
                Type = "SurpriseReward",
                Title = title,
                Message = message,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            });
        }

        // ══════════════════════════════════════════════════════════
        // ── 3. AssignRewardModal — GET ─────────────────────────────
        // ══════════════════════════════════════════════════════════
        public async Task<IActionResult> AssignRewardModal(int reportId)
        {
            if (!IsAdmin()) return NotAdmin();
            SetAdminViewBag();

            var report = await _lostRepo.GetByIdAsync(reportId);
            if (report == null) return NotFound();

            // البوست يجب أن يكون بدون مكافأة
            if (report.RewardAmount > 0)
            {
                TempData["LostError"] = "This report already has a reward.";
                return RedirectToAction("Lost_Pets");
            }

            // جلب الـ Claim المؤكد فقط
            var allClaims = (await _foundClaimRepo.GetByReportIdAsync(reportId)).ToList();
            var confirmedClaim = allClaims.FirstOrDefault(c => c.Status == "Confirmed");

            if (confirmedClaim == null)
            {
                TempData["LostError"] = "No confirmed finder for this report yet.";
                return RedirectToAction("Lost_Pets");
            }

            var finder = confirmedClaim.IdUser.HasValue
                          ? await _userRepo.GetByIdAsync(confirmedClaim.IdUser.Value)
                          : null;
            var owner = await _userRepo.GetByIdAsync(report.IdUser);
            var pool = (await _surpriseRepo.GetAllAsync())
                          .Where(r => !r.IsUsed)
                          .ToList();

            bool hasRecentReward = finder != null &&
                await _rewardLogRepo.HasRecentRewardAsync(finder.UserId);

            ViewBag.Report = report;
            ViewBag.OwnerName = owner?.FullName ?? "—";
            ViewBag.OwnerPhone = owner?.Phone ?? "—";
            ViewBag.Finder = finder;
            ViewBag.FoundClaim = confirmedClaim;
            ViewBag.Pool = pool;
            ViewBag.HasRecentReward = hasRecentReward;
            ViewBag.RewardHistory = finder != null
                ? (await _rewardLogRepo.GetByUserIdAsync(finder.UserId)).ToList()
                : new List<RewardLogDto>();

            if (TempData["RewardError"] != null) ViewBag.Error = TempData["RewardError"];
            return View();
        }

        // ── AssignReward — POST ────────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignReward(
            int reportId, int claimId, int recipientId,
            int rewardId, string reasonCategory, string? reasonNote)
        {
            if (!IsAdmin()) return NotAdmin();
            var adminId = HttpContext.Session.GetInt32("UserId") ?? 0;

            var reward = await _surpriseRepo.GetByIdAsync(rewardId);
            var recipient = await _userRepo.GetByIdAsync(recipientId);

            if (reward == null || reward.IsUsed)
            {
                TempData["RewardError"] = "⚠️ This reward is no longer available.";
                return RedirectToAction("AssignRewardModal", new { reportId });
            }

            // تسليم المكافأة تلقائياً
            await DeliverRewardAsync(reward, recipientId, recipient?.FullName ?? "", reasonCategory);

            // تعليم المكافأة كمستخدمة
            await _surpriseRepo.MarkAsUsedAsync(rewardId, recipientId);

            // سجّل في RewardLog
            await _rewardLogRepo.AddAsync(new RewardLog
            {
                IdReward = rewardId,
                IdRecipient = recipientId,
                IdAdminGiver = adminId,
                IdLostReport = reportId,
                IdFoundClaim = claimId,
                GiftType = "FinderReward",
                ReasonCategory = reasonCategory,
                ReasonNote = reasonNote,
                DeliveryStatus = "Delivered",
                CreatedAt = DateTime.UtcNow
            });

            TempData["LostSuccess"] =
                $"🎁 Reward '{reward.Title}' assigned to {recipient?.FullName} successfully!";
            return RedirectToAction("Lost_Pets");
        }

        // ══════════════════════════════════════════════════════════
        // ── 4. GiveRewardModal — GET ───────────────────────────────
        // ══════════════════════════════════════════════════════════
        public async Task<IActionResult> GiveRewardModal(int userId)
        {
            if (!IsAdmin()) return NotAdmin();
            SetAdminViewBag();

            var recipient = await _userRepo.GetByIdAsync(userId);
            if (recipient == null) return NotFound();

            var pool = (await _surpriseRepo.GetAllAsync())
                                  .Where(r => !r.IsUsed).ToList();
            var rewardHistory = (await _rewardLogRepo.GetByUserIdAsync(userId)).ToList();
            bool hasRecentReward = await _rewardLogRepo.HasRecentRewardAsync(userId);

            ViewBag.Recipient = recipient;
            ViewBag.Pool = pool;
            ViewBag.RewardHistory = rewardHistory;
            ViewBag.HasRecentReward = hasRecentReward;

            if (TempData["RewardError"] != null) ViewBag.Error = TempData["RewardError"];
            return View();
        }

        // ── GiveReward — POST ──────────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> GiveReward(
            int recipientId, int rewardId,
            string reasonCategory, string? reasonNote)
        {
            if (!IsAdmin()) return NotAdmin();
            var adminId = HttpContext.Session.GetInt32("UserId") ?? 0;

            var reward = await _surpriseRepo.GetByIdAsync(rewardId);
            var recipient = await _userRepo.GetByIdAsync(recipientId);

            if (reward == null || reward.IsUsed)
            {
                TempData["RewardError"] = "⚠️ This reward is no longer available.";
                return RedirectToAction("GiveRewardModal", new { userId = recipientId });
            }

            await DeliverRewardAsync(reward, recipientId, recipient?.FullName ?? "", reasonCategory);
            await _surpriseRepo.MarkAsUsedAsync(rewardId, recipientId);

            await _rewardLogRepo.AddAsync(new RewardLog
            {
                IdReward = rewardId,
                IdRecipient = recipientId,
                IdAdminGiver = adminId,
                GiftType = "ManualGift",
                ReasonCategory = reasonCategory,
                ReasonNote = reasonNote,
                DeliveryStatus = "Delivered",
                CreatedAt = DateTime.UtcNow
            });

            TempData["SuccessMessage"] =
                $"🎁 '{reward.Title}' given to {recipient?.FullName} successfully!";
            return RedirectToAction("Details_U", new { id = recipientId });
        }

        public async Task<IActionResult> RewardLog(
    string? search, string? typeFilter = "All")
        {
            if (!IsAdmin()) return NotAdmin();
            SetAdminViewBag();

            var allLogs = (await _rewardLogRepo.GetAllAsync()).ToList();

            // ── فلتر النوع ──
            if (typeFilter != "All")
                allLogs = allLogs.Where(l => l.GiftType == typeFilter).ToList();

            // ── فلتر البحث ──
            if (!string.IsNullOrWhiteSpace(search))
            {
                var q = search.ToLower();
                allLogs = allLogs.Where(l =>
                    (l.RecipientName?.ToLower().Contains(q) == true) ||
                    (l.RecipientEmail?.ToLower().Contains(q) == true) ||
                    (l.RewardTitle?.ToLower().Contains(q) == true) ||
                    (l.ReasonCategory?.ToLower().Contains(q) == true)
                ).ToList();
            }

            // ── KPI ──
            ViewBag.TotalCount = allLogs.Count;
            ViewBag.TotalWallet = allLogs
                .Where(l => l.RewardType == "WalletCredit")
                .Sum(l => l.RewardValue ?? 0);
            ViewBag.FinderCount = allLogs.Count(l => l.GiftType == "FinderReward");
            ViewBag.ManualCount = allLogs.Count(l => l.GiftType == "ManualGift");
            ViewBag.TypeFilter = typeFilter ?? "All";
            ViewBag.SearchQuery = search ?? "";
            ViewBag.Logs = allLogs;

            return View();
        }


        [HttpGet]
        public async Task<IActionResult> GetRewardForEdit(int rewardId)
        {
            if (!IsAdmin()) return NotAdmin();
            var reward = await _surpriseRepo.GetByIdAsync(rewardId);
            if (reward == null || reward.IsUsed)
                return Json(new { success = false });

            return Json(new
            {
                success = true,
                idReward = reward.IdReward,
                rewardType = reward.RewardType,
                title = reward.Title,
                value = reward.Value,
                discountPercent = reward.DiscountPercent,
                partnerName = reward.PartnerName,
                couponCode = reward.CouponCode,
                expiryDate = reward.ExpiryDate?.ToString("yyyy-MM-dd")
            });
        }

        // POST — يحفظ التعديل
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditSurpriseReward(
            int idReward,
            string rewardType,
            string title,
            decimal? value,
            int? discountPercent,
            string? partnerName,
            string? couponCode,
            DateTime? expiryDate)
        {
            if (!IsAdmin()) return NotAdmin();

            await _surpriseRepo.UpdateAsync(new SurpriseRewardPool
            {
                IdReward = idReward,
                RewardType = rewardType,
                Title = title,
                Value = value,
                DiscountPercent = discountPercent,
                PartnerName = partnerName,
                CouponCode = couponCode,
                ExpiryDate = expiryDate
            });

            TempData["LostSuccess"] = "✅ Reward updated successfully.";
            return RedirectToAction("Lost_Pets");
        }

        // ════════════════════════════════════════════════════════
        // Found_Pets — GET
        // ════════════════════════════════════════════════════════
        public async Task<IActionResult> Found_Pets(string statusFilter = "All")
        {
            if (!IsAdmin()) return NotAdmin();
            SetAdminViewBag();

            var allClaims = (await _foundClaimRepo.GetAllWithDetailsAsync()).ToList();
            var filtered = statusFilter == "All"
                ? allClaims
                : allClaims.Where(c => c.Status == statusFilter).ToList();
            var topFinders = (await _foundClaimRepo.GetTopFindersAsync()).ToList();

            var vm = new SuperAdminFoundViewModel
            {
                Claims = filtered,
                TopFinders = topFinders,
                TotalClaims = allClaims.Count,
                PendingCount = allClaims.Count(c => c.Status == "Pending"),
                ConfirmedCount = allClaims.Count(c => c.Status == "Confirmed"),
                TotalRewardsIssued = allClaims
                    .Where(c => c.Status == "Confirmed")
                    .Sum(c => c.RewardAmount ?? 0),
                StatusFilter = statusFilter
            };

            if (TempData["FoundSuccess"] != null) ViewBag.Success = TempData["FoundSuccess"];
            if (TempData["FoundError"] != null) ViewBag.Error = TempData["FoundError"];
            ViewBag.RewardedClaimIds = await _rewardLogRepo.GetRewardedClaimIdsAsync();

            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CloseSuspiciousReport(
            int reportId, string reason, string adminMessage)
        {
            if (!IsAdmin()) return NotAdmin();

            var report = await _lostRepo.GetByIdAsync(reportId);
            if (report != null)
            {
                await _lostRepo.UpdateStatusAsync(reportId, "Closed");
                await _notifRepo.AddAsync(new Notification
                {
                    IdUser = report.IdUser,
                    Type = "Warning",
                    Title = "⚠️ Your lost pet report was closed by admin",
                    Message = $"Reason: {reason}. {adminMessage}",
                    IsRead = false,
                    RelatedEntityType = "LostPetReport",
                    RelatedEntityId = reportId,
                    CreatedAt = DateTime.UtcNow
                });
            }
            TempData["LostSuccess"] = "🔒 Report closed and owner notified.";
            return RedirectToAction("Lost_Pets");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SendEmergencyAlert(int reportId)
        {
            if (!IsAdmin()) return NotAdmin();

            var report = await _lostRepo.GetByIdAsync(reportId);
            if (report == null)
            {
                TempData["LostError"] = "Report not found.";
                return RedirectToAction("Lost_Pets");
            }

            var userIds = (await _userRepo.GetUserIdsByCityAsync(report.City))
                .Where(id => id != report.IdUser)
                .ToList();

            if (userIds.Any())
            {
                await _notifRepo.AddBulkAsync(
                    userIds,
                    type: "EmergencyAlert",
                    title: $"🚨 Emergency: Lost pet in {report.City}",
                    message: $"{report.PetName} ({report.AnimalType}) was lost near {report.Area}, {report.City}. Can you help?",
                    relatedEntityType: "LostPetReport",
                    relatedEntityId: reportId
                );
            }

            TempData["LostSuccess"] = $"🚨 Emergency alert sent to {userIds.Count} users in {report.City}.";
            return RedirectToAction("Lost_Pets");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmPayment(int paymentId)
        {
            if (!IsAdmin()) return NotAdmin();
            var adminId = HttpContext.Session.GetInt32("UserId") ?? 0;

            var payment = await _paymentRepo.GetByIdAsync(paymentId);
            if (payment != null)
            {
                await _paymentRepo.ConfirmAsync(paymentId, adminId);

                if (payment.RelatedId.HasValue)
                {
                    // فعّل البوست
                    await _lostRepo.UpdatePaymentStatusAsync(
                        payment.RelatedId.Value, true, paymentId);

                    // حدّث AmountPaid برسوم النشر فقط
                    var report = await _lostRepo.GetByIdAsync(payment.RelatedId.Value);
                    if (report != null)
                    {
                        decimal feeAmount = payment.Amount - (report.RewardAmount ?? 0);
                        await _lostRepo.UpdateAmountPaidAsync(
                            payment.RelatedId.Value, feeAmount > 0 ? feeAmount : payment.Amount);

                        // ✅ حجز المكافأة إذا كانت موجودة
                        if (report.RewardAmount > 0 && !report.RewardReserved)
                        {
                            await _lostRepo.MarkRewardReservedAsync(payment.RelatedId.Value);
                        }
                    }
                }

                await _notifRepo.AddAsync(new Notification
                {
                    IdUser = payment.IdUser,
                    Type = "Payment",
                    Title = "✅ Payment confirmed",
                    Message = $"Your payment of {payment.Amount:F2} JOD confirmed. " +
                                        "Your lost pet report is now live!",
                    IsRead = false,
                    RelatedEntityType = "LostPetReport",
                    RelatedEntityId = payment.RelatedId,
                    CreatedAt = DateTime.UtcNow
                });
            }

            TempData["LostSuccess"] = "✅ Payment confirmed and report activated.";
            return RedirectToAction("Lost_Pets");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectPayment(int paymentId, string reason)
        {
            if (!IsAdmin()) return NotAdmin();
            var adminId = HttpContext.Session.GetInt32("UserId") ?? 0;

            var payment = await _paymentRepo.GetByIdAsync(paymentId);
            if (payment != null)
            {
                await _paymentRepo.RejectAsync(paymentId, adminId, reason);

                // ✅ غيّري الـ Report Status لـ Rejected
                if (payment.RelatedId.HasValue)
                    await _lostRepo.UpdateStatusAsync(payment.RelatedId.Value, "Rejected");

                await _notifRepo.AddAsync(new Notification
                {
                    IdUser = payment.IdUser,
                    Type = "Payment",
                    Title = "❌ Payment rejected",
                    Message = $"Your CliQ payment was rejected. Reason: {reason}. Please contact support.",
                    IsRead = false,
                    RelatedEntityType = "LostPetReport",
                    RelatedEntityId = payment.RelatedId,
                    CreatedAt = DateTime.UtcNow
                });
            }

            TempData["LostError"] = "❌ Payment rejected and user notified.";
            return RedirectToAction("Lost_Pets");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelReward(int claimId, string reason)
        {
            if (!IsAdmin()) return NotAdmin();

            var claim = await _foundClaimRepo.GetByIdAsync(claimId);
            if (claim != null)
            {
                await _foundClaimRepo.CancelRewardAsync(claimId);

                if (claim.IdUser.HasValue)
                {
                    await _notifRepo.AddAsync(new Notification
                    {
                        IdUser = claim.IdUser.Value,
                        Type = "Warning",
                        Title = "🚫 Your reward has been cancelled",
                        Message = $"Reason: {reason}",
                        IsRead = false,
                        RelatedEntityType = "FoundPetClaim",
                        RelatedEntityId = claimId,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            TempData["FoundSuccess"] = "🚫 Reward cancelled and finder notified.";
            return RedirectToAction("Found_Pets");
        }

        // ── Surprise Reward Pool Management ──────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSurpriseReward(
            string rewardType, string title, string? description,
            decimal? value, int? discountPercent,
            string? partnerName, string? couponCode, DateTime? expiryDate)
        {
            if (!IsAdmin()) return NotAdmin();

            await _surpriseRepo.AddAsync(new SurpriseRewardPool
            {
                RewardType = rewardType,
                Title = title,
                Description = description,
                Value = value,
                DiscountPercent = discountPercent,
                PartnerName = partnerName,
                CouponCode = couponCode,
                ExpiryDate = expiryDate
            });

            TempData["LostSuccess"] = "🎁 Surprise reward added to pool.";
            return RedirectToAction("Lost_Pets");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSurpriseReward(int rewardId)
        {
            if (!IsAdmin()) return NotAdmin();
            await _surpriseRepo.DeleteAsync(rewardId);
            TempData["LostSuccess"] = "🗑 Reward removed from pool.";
            return RedirectToAction("Lost_Pets");
        }


        // ── GET ─────────────────────────────────────────────
        public async Task<IActionResult> AlertModal(int reportId)
        {
            if (!IsAdmin()) return NotAdmin();
            SetAdminViewBag();

            var report = await _lostRepo.GetByIdAsync(reportId);
            if (report == null) return NotFound();

            var owner = await _userRepo.GetByIdAsync(report.IdUser);

            // ✅ نمرر report.Area للحصول على ProximityRank
            var users = (await _userRepo.GetUsersByCityAsync(report.City, report.Area))
                        .Where(u => u.UserId != report.IdUser)
                        .ToList();

            ViewBag.Report = report;
            ViewBag.OwnerPhone = owner?.Phone ?? "—";
            ViewBag.OwnerName = owner?.FullName ?? "—";
            ViewBag.CityUsers = users;
            ViewBag.UserCount = users.Count;
            ViewBag.SameAreaCount = users.Count(u => u.ProximityRank == 1);
            ViewBag.SameCityCount = users.Count(u => u.ProximityRank == 2);

            if (TempData["AlertError"] != null) ViewBag.Error = TempData["AlertError"];
            return View();
        }

        // ── POST ────────────────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SendAlert(
            int reportId,
            int[]? selectedUserIds,
            string message,
            string priority = "Normal")
        {
            if (!IsAdmin()) return NotAdmin();

            var report = await _lostRepo.GetByIdAsync(reportId);
            if (report == null) return NotFound();

            if (selectedUserIds == null || selectedUserIds.Length == 0)
            {
                TempData["AlertError"] = "Please select at least one user.";
                return RedirectToAction("AlertModal", new { reportId });
            }

            await _notifRepo.AddBulkAsync(
                selectedUserIds,
                type: "EmergencyAlert",
                title: priority == "Urgent"
                                       ? "🚨 Urgent Alert!"
                                       : "🔔 Lost Pet Alert",
                message: message,
                relatedEntityType: "LostPetReport",
                relatedEntityId: reportId
            );

            TempData["LostSuccess"] =
                $"🚨 Alert sent to {selectedUserIds.Length} user(s) in {report.City}.";
            return RedirectToAction("Lost_Pets");
        }

        // ════════════════════════════════════════════════════════
        // Stubs
        // ════════════════════════════════════════════════════════
        // ════════════════════════════════════════════════════════
        // Clinics — GET
        // ════════════════════════════════════════════════════════
        public async Task<IActionResult> Clinics(string? search, string? statusFilter)
        {
            if (!IsAdmin()) return NotAdmin();
            SetAdminViewBag();

            var allClinics = (await _clinicRepo.GetAllAsync()).ToList();

            var vm = new ClinicManagementViewModel
            {
                TotalClinics = allClinics.Count,
                ActiveClinics = allClinics.Count(c => c.VerificationStatus == "Approved"),
                PendingClinics = allClinics.Count(c => c.VerificationStatus == "Pending"),
                SuspendedClinics = allClinics.Count(c => c.VerificationStatus == "Suspended"),
                NewThisMonth = await _clinicRepo.CountNewThisMonthAsync(),
                StatusFilter = statusFilter,
                SearchQuery = search
            };

            var rows = new List<ClinicRowDto>();
            foreach (var c in allClinics)
            {
                if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "All"
                    && c.VerificationStatus != statusFilter) continue;

                var owner = await _userRepo.GetByIdAsync(c.OwnerUserId);
                var docs = (await _clinicDocumentRepo.GetByClinicIdAsync(c.IdClinic)).ToList();

                var ownerName = owner?.FullName ?? "—";
                var ownerEmail = owner?.Email ?? "—";
                var ownerPhone = owner?.Phone ?? "—";

                if (!string.IsNullOrWhiteSpace(search))
                {
                    var q = search.ToLower();
                    if (!c.Name.ToLower().Contains(q) &&
                        !ownerName.ToLower().Contains(q) &&
                        !ownerEmail.ToLower().Contains(q) &&
                        !(c.City ?? "").ToLower().Contains(q)) continue;
                }

                rows.Add(new ClinicRowDto
                {
                    IdClinic = c.IdClinic,
                    Name = c.Name,
                    OwnerName = ownerName,
                    OwnerEmail = ownerEmail,
                    OwnerPhone = ownerPhone,
                    City = c.City,
                    LogoUrl = c.LogoUrl,
                    VerificationStatus = c.VerificationStatus,
                    CreatedAt = c.CreatedAt,
                    DocumentsCount = docs.Count(d => d.DocumentType != "Logo"),
                    PendingDocsCount = docs.Count(d =>
                        d.DocumentType != "Logo" &&
                        (d.Status == "Pending" || d.Status == "pending"))
                });
            }

            vm.Clinics = rows;

            if (TempData["ClinicSuccess"] != null) ViewBag.Success = TempData["ClinicSuccess"];
            if (TempData["ClinicError"] != null) ViewBag.Error = TempData["ClinicError"];

            return View(vm);
        }

        // ════════════════════════════════════════════════════════
        // Details_C — GET
        // ════════════════════════════════════════════════════════
        public async Task<IActionResult> Details_C(int id)
        {
            if (!IsAdmin()) return NotAdmin();
            SetAdminViewBag();

            var clinic = await _clinicRepo.GetByIdAsync(id);
            if (clinic == null) return NotFound();

            var owner = await _userRepo.GetByIdAsync(clinic.OwnerUserId);
            var docs = (await _clinicDocumentRepo.GetByClinicIdAsync(id)).ToList();

            var vm = new ClinicDetailAdminViewModel
            {
                Clinic = clinic,
                OwnerName = owner?.FullName ?? "—",
                OwnerEmail = owner?.Email ?? "—",
                OwnerPhone = owner?.Phone ?? "—",
                OwnerId = clinic.OwnerUserId,
                Documents = docs
            };

            // ── Doctors for this clinic ──────────────────────────
            // GetAllForAdminAsync تقبل string? statusFilter فقط
            // نجلب الكل ثم نفلتر بالـ IdClinic
            var allDoctors = (await _doctorAdminRepo.GetAllForAdminAsync(null))
                                .Where(d => d.IdClinic == id)
                                .ToList();
            ViewBag.Doctors = allDoctors;

            // ── Appointments for this clinic ─────────────────────
            // GetAllAsync تقبل clinicId مباشرة كـ parameter ثاني
            var allAppointments = (await _apptRepo.GetAllAsync(null, id)).ToList();
            ViewBag.Appointments = allAppointments;

            if (TempData["ClinicSuccess"] != null) ViewBag.Success = TempData["ClinicSuccess"];
            if (TempData["ClinicError"] != null) ViewBag.Error = TempData["ClinicError"];

            return View(vm);
        }

        // ════════════════════════════════════════════════════════
        // ApproveClinicDocument — POST
        // ════════════════════════════════════════════════════════
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveClinicDocument(int docId, int clinicId)
        {
            if (!IsAdmin()) return NotAdmin();
            var adminId = HttpContext.Session.GetInt32("UserId") ?? 0;
            await _clinicDocumentRepo.UpdateStatusAsync(docId, "approved", null, adminId);
            TempData["ClinicSuccess"] = "✅ Document approved.";
            return RedirectToAction("Details_C", new { id = clinicId });
        }

        // ════════════════════════════════════════════════════════
        // RejectClinicDocument — POST
        // ════════════════════════════════════════════════════════
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectClinicDocument(int docId, int clinicId, string? adminNote)
        {
            if (!IsAdmin()) return NotAdmin();
            var adminId = HttpContext.Session.GetInt32("UserId") ?? 0;
            await _clinicDocumentRepo.UpdateStatusAsync(docId, "rejected", adminNote, adminId);
            TempData["ClinicError"] = "❌ Document rejected.";
            return RedirectToAction("Details_C", new { id = clinicId });
        }

        // ════════════════════════════════════════════════════════
        // ApproveClinic — POST
        // ════════════════════════════════════════════════════════
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveClinic(int clinicId, string? from)
        {
            if (!IsAdmin()) return NotAdmin();

            var clinic = await _clinicRepo.GetByIdAsync(clinicId);
            if (clinic == null) return NotFound();

            clinic.VerificationStatus = "Approved";
            await _clinicRepo.UpdateAsync(clinic);

            await _userRepo.UpdateStatusAsync(clinic.OwnerUserId, "Active");

            await _notifRepo.AddAsync(new Notification
            {
                IdUser = clinic.OwnerUserId,
                Type = "ClinicApproved",
                Title = "✅ Your Clinic Has Been Approved!",
                Message = $"Congratulations! {clinic.Name} is now live on Waffy. You can log in and start managing your clinic.",
                IsRead = false,
                RelatedEntityType = "Clinic",
                RelatedEntityId = clinicId,
                CreatedAt = DateTime.UtcNow
            });

            TempData["ClinicSuccess"] = $"✅ {clinic.Name} approved successfully.";
            return from == "details"
                ? RedirectToAction("Details_C", new { id = clinicId })
                : RedirectToAction("Clinics");
        }

        // ════════════════════════════════════════════════════════
        // RejectClinic — POST
        // ════════════════════════════════════════════════════════
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectClinic(int clinicId, string? reason, string? message, string? from)
        {
            if (!IsAdmin()) return NotAdmin();

            var clinic = await _clinicRepo.GetByIdAsync(clinicId);
            if (clinic == null) return NotFound();

            clinic.VerificationStatus = "Rejected";
            await _clinicRepo.UpdateAsync(clinic);

            await _notifRepo.AddAsync(new Notification
            {
                IdUser = clinic.OwnerUserId,
                Type = "ClinicRejected",
                Title = "❌ Clinic Application Rejected",
                Message = $"Your clinic \"{clinic.Name}\" was rejected. Reason: {reason}. {message}",
                IsRead = false,
                RelatedEntityType = "Clinic",
                RelatedEntityId = clinicId,
                CreatedAt = DateTime.UtcNow
            });

            TempData["ClinicError"] = $"❌ {clinic.Name} rejected.";
            return from == "details"
                ? RedirectToAction("Details_C", new { id = clinicId })
                : RedirectToAction("Clinics");
        }

        // ════════════════════════════════════════════════════════
        // SuspendClinic — POST
        // ════════════════════════════════════════════════════════
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SuspendClinic(
            int clinicId, string? reason, string? duration, string? message, string? from)
        {
            if (!IsAdmin()) return NotAdmin();

            var clinic = await _clinicRepo.GetByIdAsync(clinicId);
            if (clinic == null) return NotFound();

            clinic.VerificationStatus = "Suspended";
            await _clinicRepo.UpdateAsync(clinic);

            await _notifRepo.AddAsync(new Notification
            {
                IdUser = clinic.OwnerUserId,
                Type = "ClinicSuspended",
                Title = "🔒 Your Clinic Has Been Suspended",
                Message = $"Clinic \"{clinic.Name}\" was suspended{(duration != null ? $" for {duration}" : "")}. Reason: {reason}. {message}",
                IsRead = false,
                RelatedEntityType = "Clinic",
                RelatedEntityId = clinicId,
                CreatedAt = DateTime.UtcNow
            });

            TempData["ClinicError"] = $"🔒 {clinic.Name} suspended.";
            return from == "details"
                ? RedirectToAction("Details_C", new { id = clinicId })
                : RedirectToAction("Clinics");
        }

        // ════════════════════════════════════════════════════════
        // ReactivateClinic — POST
        // ════════════════════════════════════════════════════════
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ReactivateClinic(int clinicId, string? from)
        {
            if (!IsAdmin()) return NotAdmin();

            var clinic = await _clinicRepo.GetByIdAsync(clinicId);
            if (clinic == null) return NotFound();

            clinic.VerificationStatus = "Approved";
            await _clinicRepo.UpdateAsync(clinic);

            await _notifRepo.AddAsync(new Notification
            {
                IdUser = clinic.OwnerUserId,
                Type = "ClinicReactivated",
                Title = "✅ Your Clinic Is Active Again",
                Message = $"Your clinic \"{clinic.Name}\" has been reactivated.",
                IsRead = false,
                RelatedEntityType = "Clinic",
                RelatedEntityId = clinicId,
                CreatedAt = DateTime.UtcNow
            });

            TempData["ClinicSuccess"] = $"✅ {clinic.Name} reactivated.";
            return from == "details"
                ? RedirectToAction("Details_C", new { id = clinicId })
                : RedirectToAction("Clinics");
        }

        // ════════════════════════════════════════════════════════
        // DeleteClinic — POST
        // ════════════════════════════════════════════════════════
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteClinic(int clinicId)
        {
            if (!IsAdmin()) return NotAdmin();
            await _clinicRepo.DeleteAsync(clinicId);
            TempData["ClinicSuccess"] = "🗑 Clinic deleted.";
            return RedirectToAction("Clinics");
        }

        // ════════════════════════════════════════════════════════
        // WarnClinic — POST (JSON)
        // ════════════════════════════════════════════════════════
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> WarnClinic(int clinicId, string warnType, string warnMessage)
        {
            if (!IsAdmin()) return Json(new { success = false });

            var clinic = await _clinicRepo.GetByIdAsync(clinicId);
            if (clinic == null) return Json(new { success = false });

            await _notifRepo.AddAsync(new Notification
            {
                IdUser = clinic.OwnerUserId,
                Type = "ClinicWarning",
                Title = $"⚠️ Warning: {warnType}",
                Message = warnMessage,
                IsRead = false,
                RelatedEntityType = "Clinic",
                RelatedEntityId = clinicId,
                CreatedAt = DateTime.UtcNow
            });

            return Json(new { success = true });
        }

        // ════════════════════════════════════════════════════════
        // Doctors_C — GET
        // ════════════════════════════════════════════════════════
        public async Task<IActionResult> Doctors_C(string? statusFilter)
        {
            if (!IsAdmin()) return NotAdmin();
            SetAdminViewBag();

            var allDoctors = (await _doctorAdminRepo
                .GetAllForAdminAsync(null)).ToList();

            var filtered = string.IsNullOrEmpty(statusFilter) || statusFilter == "All"
                ? allDoctors
                : allDoctors.Where(d => d.VerificationStatus == statusFilter).ToList();

            // عدد العيادات التي لها مستندات معلقة
            var allClinics = (await _clinicRepo.GetAllAsync()).ToList();
            int clinicsWithPendingDocs = 0;
            foreach (var c in allClinics.Where(c => c.VerificationStatus == "Pending"))
            {
                var docs = (await _clinicDocumentRepo.GetByClinicIdAsync(c.IdClinic)).ToList();
                if (docs.Any(d => d.Status?.ToLower() == "pending" && d.DocumentType != "Logo"))
                    clinicsWithPendingDocs++;
            }

            var vm = new DoctorsAdminViewModel
            {
                Doctors = filtered,
                TotalDoctors = allDoctors.Count,
                PendingDoctors = allDoctors.Count(d => d.VerificationStatus == "Pending"),
                ApprovedDoctors = allDoctors.Count(d => d.VerificationStatus == "Approved"),
                RejectedDoctors = allDoctors.Count(d => d.VerificationStatus == "Rejected"),
                ClinicsWithPendingDocs = clinicsWithPendingDocs,
                StatusFilter = statusFilter ?? "All"
            };

            if (TempData["DoctorSuccess"] != null) ViewBag.Success = TempData["DoctorSuccess"];
            if (TempData["DoctorError"] != null) ViewBag.Error = TempData["DoctorError"];

            return View(vm);
        }

        // ════════════════════════════════════════════════════════
        // ApproveDoctor — POST
        // ════════════════════════════════════════════════════════
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveDoctor(int idDoctor, int idUser, string? clinicName)
        {
            if (!IsAdmin()) return NotAdmin();

            await _doctorAdminRepo.UpdateVerificationStatusAsync(idDoctor, "Approved");

            // إشعار للدكتور
            await _notifRepo.AddAsync(new Notification
            {
                IdUser = idUser,
                Type = "DoctorApproved",
                Title = "✅ Your account has been approved!",
                Message = $"Your doctor profile at {clinicName ?? "the clinic"} has been approved. You can now log in and complete your information.",
                IsRead = false,
                RelatedEntityType = "Doctor",
                RelatedEntityId = idDoctor,
                CreatedAt = DateTime.UtcNow
            });

            TempData["DoctorSuccess"] = "✅ Doctor approved successfully.";
            return RedirectToAction("Doctors_C");
        }

        // ════════════════════════════════════════════════════════
        // RejectDoctor — POST
        // ════════════════════════════════════════════════════════
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectDoctor(int idDoctor, int idUser,
            string? reason, string? clinicName)
        {
            if (!IsAdmin()) return NotAdmin();

            await _doctorAdminRepo.UpdateVerificationStatusAsync(idDoctor, "Rejected");

            await _notifRepo.AddAsync(new Notification
            {
                IdUser = idUser,
                Type = "DoctorRejected",
                Title = "❌ Doctor application rejected",
                Message = $"Your application at {clinicName ?? "the clinic"} was rejected. Reason: {reason}. Please contact the clinic admin.",
                IsRead = false,
                RelatedEntityType = "Doctor",
                RelatedEntityId = idDoctor,
                CreatedAt = DateTime.UtcNow
            });

            TempData["DoctorError"] = "❌ Doctor rejected.";
            return RedirectToAction("Doctors_C");
        }


        [HttpGet]
        public async Task<IActionResult> Details_D(int id)
        {
            if (!IsAdmin()) return NotAdmin();
            SetAdminViewBag();

            var doctor = await _doctorAdminRepo.GetFullInfoByDoctorIdAsync(id);
            if (doctor == null) return NotFound();

            var docs = (await _doctorAdminRepo.GetDocumentsByDoctorIdAsync(id)).ToList();

            ViewBag.AllDocs = docs;
            ViewBag.CertDocs = docs.Where(d => d.DocumentType == "Certification").ToList();
            ViewBag.OtherDocs = docs.Where(d => d.DocumentType != "Certification").ToList();
            ViewBag.DoctorId = id;

            if (TempData["DoctorSuccess"] != null) ViewBag.Success = TempData["DoctorSuccess"];
            if (TempData["DoctorError"] != null) ViewBag.Error = TempData["DoctorError"];

            return View(doctor);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveDoctorDocument(int docId, int doctorId)
        {
            if (!IsAdmin()) return NotAdmin();
            await _doctorAdminRepo.UpdateDocumentStatusAsync(docId, "Approved");
            TempData["DoctorSuccess"] = "✅ Document approved.";
            return RedirectToAction("Details_D", new { id = doctorId });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectDoctorDocument(int docId, int doctorId)
        {
            if (!IsAdmin()) return NotAdmin();
            await _doctorAdminRepo.UpdateDocumentStatusAsync(docId, "Rejected");
            TempData["DoctorError"] = "❌ Document rejected.";
            return RedirectToAction("Details_D", new { id = doctorId });
        }

        // ════════════════════════════════════════════════════════
        // GET DANGER REPORT IMAGES — AJAX
        // GET /SuperAdmin/GetDangerImages?reportId=5
        // ════════════════════════════════════════════════════════
        [HttpGet]
        public async Task<IActionResult> GetDangerImages(int reportId)
        {
            var images = await _dangerRepo.GetImagesByReportIdAsync(reportId);
            return Json(images.Select(i => new { i.ImageUrl, i.SortOrder }));
        }

        public async Task<IActionResult> Appointments_C()
        {
            if (!IsAdmin()) return NotAdmin();
            SetAdminViewBag();

            var appointments = (await _apptRepo.GetAllAsync()).ToList();
            var clinics = (await _clinicRepo.GetAllAsync())
                               .Where(c => c.VerificationStatus == "Approved")
                               .ToList();

            var vm = new SuperAdminAppointmentsViewModel
            {
                Appointments = appointments,
                AvailableClinics = clinics,
                TotalCount = appointments.Count,
                PendingCount = appointments.Count(a => a.Status is
                                        "PendingDoctorApproval" or "PendingClinicApproval" or
                                        "PendingPayment" or "PendingPaymentVerification"),
                ConfirmedCount = appointments.Count(a => a.Status == "Confirmed"),
                CompletedCount = appointments.Count(a => a.Status == "Completed"),
                CancelledCount = appointments.Count(a => a.Status == "Cancelled"),
                NoShowCount = appointments.Count(a => a.Status == "NoShow"),
                TotalDepositsCollected = appointments
                    .Where(a => a.DepositPaid).Sum(a => a.DepositAmount),
                TotalDepositsReturned = appointments
                    .Where(a => a.DepositReturned).Sum(a => a.DepositAmount),
                TotalDepositsRetained = appointments
                    .Where(a => a.Status == "NoShow" && a.DepositPaid && !a.DepositReturned)
                    .Sum(a => a.DepositAmount)
            };

            return View(vm);
        }
        public IActionResult Analytics_A() { if (!IsAdmin()) return NotAdmin(); SetAdminViewBag(); return View(); }
        public IActionResult Map() { if (!IsAdmin()) return NotAdmin(); SetAdminViewBag(); return View(); }
        public IActionResult Notifications() { if (!IsAdmin()) return NotAdmin(); SetAdminViewBag(); return View(); }
    }
}