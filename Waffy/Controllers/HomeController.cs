using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Waffy.Interfaces;
using Waffy.Models;
using Waffy.Models.ViewModels;
using Waffy.Repositories.Interfaces;

namespace Waffy.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IComplaintRepository _complaintRepo;
        private readonly IClinicRepository _clinicRepo;
        private readonly IStoreRepository _storeRepo;
        private readonly IPetLawRepository _lawRepo;
        private readonly INotificationRepository _notifRepo;
        private readonly IProductRepository _productRepo;
        private readonly IProductImageRepository _productImageRepo;
        private readonly ILostPetRepository _lostPetRepo;
        private readonly IAdoptionRepository _adoptionRepo; 


        public HomeController(
     ILogger<HomeController> logger,
     IComplaintRepository complaintRepo,
     IClinicRepository clinicRepo,
     IStoreRepository storeRepo,
     IPetLawRepository lawRepo,
     INotificationRepository notifRepo,
     IProductRepository productRepo,
     IProductImageRepository productImageRepo,
     IAdoptionRepository adoptionRepo,
     ILostPetRepository lostPetRepo)
        {
            _logger = logger;
            _complaintRepo = complaintRepo;
            _clinicRepo = clinicRepo;
            _storeRepo = storeRepo;
            _lawRepo = lawRepo;
            _notifRepo = notifRepo;
            _productRepo = productRepo;
            _productImageRepo = productImageRepo;
            _adoptionRepo = adoptionRepo;
            _lostPetRepo = lostPetRepo;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
        public async Task<IActionResult> Home_Before()
        {
            var allStores = (await _storeRepo.GetAllAsync())
     .Where(s => s.VerificationStatus == "Approved")
     .ToList();
            var featuredProducts = new List<object>();

            foreach (var store in allStores.Take(10))
            {
                var products = (await _productRepo.GetByStoreIdAsync(store.IdStore))
                    .Where(p => p.IsVisible && p.StockQuantity > 0)
                    .Take(3).ToList();

                foreach (var p in products)
                {
                    var imgs = (await _productImageRepo.GetByProductIdAsync(p.Id)).ToList();
                    var img = imgs.FirstOrDefault(i => i.IsMain)?.ImageUrl
                           ?? imgs.FirstOrDefault()?.ImageUrl ?? "";

                    featuredProducts.Add(new
                    {
                        id = p.Id,
                        name = p.Name,
                        desc = p.Description?.Length > 80
                                    ? p.Description[..80] + "…" : p.Description ?? "",
                        price = p.Price,
                        salePrice = p.SalePrice,
                        imageUrl = img,
                        storeName = store.Name,
                        storeId = store.IdStore,
                        petType = p.PetType ?? ""
                    });
                }
            }

            ViewBag.FeaturedProductsJson = System.Text.Json.JsonSerializer.Serialize(
                featuredProducts.OrderBy(_ => Guid.NewGuid()).Take(20));

            // ← أضف هذا الجزء الجديد فقط
            var lostReports = (await _lostPetRepo.GetActivePagedAsync(1, 6)).ToList();
            var lostDtos = new List<object>();
            foreach (var r in lostReports)
            {
                var images = (await _lostPetRepo.GetAllImagesAsync(r.IdLostReport)).ToList();
                var firstImage = images.FirstOrDefault() ?? "";
                lostDtos.Add(new
                {
                    id = r.IdLostReport,
                    name = r.PetName,
                    animalType = r.AnimalType,
                    breed = r.Breed ?? "",
                    color = r.Color ?? "",
                    city = r.City ?? "",
                    area = r.Area ?? "",
                    description = r.Description ?? "",
                    contactPhone = r.ContactPhone ?? "",
                    dateLost = r.DateLost?.ToString("yyyy-MM-dd") ?? "",
                    hasReward = r.RewardAmount.HasValue && r.RewardAmount > 0,
                    reward = r.RewardAmount?.ToString("F2") ?? "0",
                    image = firstImage,
                    images = images
                });
            }
            ViewBag.LostReportsJson = System.Text.Json.JsonSerializer.Serialize(lostDtos);
            // ← انتهى الجزء الجديد
            // Adoption data
            var adoptionPosts = (await _adoptionRepo.GetApprovedAsync()).Take(4).ToList();
            var adoptionDtos = new List<object>();
            foreach (var p in adoptionPosts)
            {
                var images = (await _adoptionRepo.GetAllImagesAsync(p.IdPost)).ToList();
                adoptionDtos.Add(new
                {
                    id = p.IdPost,
                    name = p.PetName ?? "",
                    type = p.AnimalType ?? "",
                    breed = p.Breed ?? "",
                    age = p.Age.HasValue ? p.Age + " months" : "",
                    gender = p.Gender ?? "",
                    city = p.City ?? "",
                    area = p.Area ?? "",
                    color = p.Color ?? "",
                    temperament = p.EnergyLevel ?? "",
                    isVaccinated = p.IsVaccinated,
                    isNeutered = p.IsNeutered,
                    goodWithChildren = p.GoodWithChildren,
                    goodWithPets = p.GoodWithPets,
                    image = images.FirstOrDefault() ?? "",
                    images = images
                });
            }
            ViewBag.AdoptionJson = System.Text.Json.JsonSerializer.Serialize(adoptionDtos);

           
            return View();
        }

        public async Task<IActionResult> Home_After()
        {


            var allStores = (await _storeRepo.GetAllAsync())
                .Where(s => s.VerificationStatus == "Approved").ToList();

            var featuredProducts = new List<object>();
            foreach (var store in allStores.Take(10))
            {
                var products = (await _productRepo.GetByStoreIdAsync(store.IdStore))
                    .Where(p => p.IsVisible && p.StockQuantity > 0)
                    .Take(3).ToList();

                foreach (var p in products)
                {
                    var imgs = (await _productImageRepo.GetByProductIdAsync(p.Id)).ToList();
                    var img = imgs.FirstOrDefault(i => i.IsMain)?.ImageUrl
                            ?? imgs.FirstOrDefault()?.ImageUrl ?? "";

                    featuredProducts.Add(new
                    {
                        id = p.Id,
                        name = p.Name,
                        desc = p.Description?.Length > 80
                                    ? p.Description[..80] + "…" : p.Description ?? "",
                        price = p.Price,
                        salePrice = p.SalePrice,
                        imageUrl = img,
                        storeName = store.Name,
                        storeId = store.IdStore,
                        petType = p.PetType ?? ""
                    });
                }
            }

            ViewBag.FeaturedProductsJson = System.Text.Json.JsonSerializer.Serialize(
                featuredProducts.OrderBy(_ => Guid.NewGuid()).Take(20));
            // ← أضف هذا الجزء الجديد فقط
            var lostReports = (await _lostPetRepo.GetActivePagedAsync(1, 6)).ToList();
            var lostDtos = new List<object>();
            foreach (var r in lostReports)
            {
                var images = (await _lostPetRepo.GetAllImagesAsync(r.IdLostReport)).ToList();
                var firstImage = images.FirstOrDefault() ?? "";
                lostDtos.Add(new
                {
                    id = r.IdLostReport,
                    name = r.PetName,
                    animalType = r.AnimalType,
                    breed = r.Breed ?? "",
                    color = r.Color ?? "",
                    city = r.City ?? "",
                    area = r.Area ?? "",
                    description = r.Description ?? "",
                    contactPhone = r.ContactPhone ?? "",
                    dateLost = r.DateLost?.ToString("yyyy-MM-dd") ?? "",
                    hasReward = r.RewardAmount.HasValue && r.RewardAmount > 0,
                    reward = r.RewardAmount?.ToString("F2") ?? "0",
                    image = firstImage,
                    images = images
                });
            }
            ViewBag.LostReportsJson = System.Text.Json.JsonSerializer.Serialize(lostDtos);
            // ← انتهى الجزء الجديد
            // Adoption data
            var adoptionPosts = (await _adoptionRepo.GetApprovedAsync()).Take(4).ToList();
            var adoptionDtos = new List<object>();
            foreach (var p in adoptionPosts)
            {
                var images = (await _adoptionRepo.GetAllImagesAsync(p.IdPost)).ToList();
                adoptionDtos.Add(new
                {
                    id = p.IdPost,
                    name = p.PetName ?? "",
                    type = p.AnimalType ?? "",
                    breed = p.Breed ?? "",
                    age = p.Age.HasValue ? p.Age + " months" : "",
                    gender = p.Gender ?? "",
                    city = p.City ?? "",
                    area = p.Area ?? "",
                    color = p.Color ?? "",
                    temperament = p.EnergyLevel ?? "",
                    isVaccinated = p.IsVaccinated,
                    isNeutered = p.IsNeutered,
                    goodWithChildren = p.GoodWithChildren,
                    goodWithPets = p.GoodWithPets,
                    image = images.FirstOrDefault() ?? "",
                    images = images
                });
            }
            ViewBag.AdoptionJson = System.Text.Json.JsonSerializer.Serialize(adoptionDtos);

            return View();
        }
        public IActionResult About()
        {
            return View();
        }
        public IActionResult Faq()
        {
            return View();
        }

        // ── GET: /Home/Travel ──────────────────────────────────────────────

        
        public async Task<IActionResult> Travel(string? category, string? search)
        {
            IEnumerable<Waffy.Models.PetLaw> laws;

            if (!string.IsNullOrWhiteSpace(search))
                laws = await _lawRepo.SearchAsync(search);
            else if (!string.IsNullOrWhiteSpace(category) && category != "all")
                laws = await _lawRepo.GetByCategoryAsync(category);
            else
                laws = await _lawRepo.GetAllAsync();

            ViewBag.CurrentCategory = category ?? "all";
            ViewBag.SearchTerm = search ?? string.Empty;

            return View(laws);
        }


        public async Task<IActionResult> Suport()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            var clinics = (await _clinicRepo.GetAllAsync())
                .Where(c => c.VerificationStatus == "Approved").ToList();
            var stores = (await _storeRepo.GetAllAsync())
                .Where(s => s.VerificationStatus == "Approved").ToList();

            ViewBag.Clinics = clinics;
            ViewBag.Stores = stores;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitSupport(
            string targetType,
            int? targetId,
            string title,
            string description,
            string priority)
        {

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");
            if (string.IsNullOrWhiteSpace(title) ||
                string.IsNullOrWhiteSpace(description))
            {
                TempData["SupportError"] = "Please fill in all required fields.";
                return RedirectToAction("Suport");
            }

            await _complaintRepo.CreateAsync(new Complaint
            {
                IdUser = userId,
                TargetType = targetType,
                TargetId = targetType == "System" ? null : targetId,
                Title = title,
                Description = description,
                Status = "Open",
                Priority = priority ?? "Medium",
                CreatedAt = DateTime.UtcNow
            });
            // إشعار لصاحب المتجر لو الشكوى عن متجر
            if (targetType == "Store" && targetId.HasValue)
            {
                var targetStore = await _storeRepo.GetByIdAsync(targetId.Value);
                if (targetStore != null)
                {
                    await _notifRepo.AddAsync(new Notification
                    {
                        IdUser = targetStore.OwnerUserId,
                        Type = "NewComplaint",
                        Title = "📢 New Complaint Received",
                        Message = $"A customer submitted a complaint via support: \"{title}\"",
                        IsRead = false,
                        RelatedEntityType = "Store",
                        RelatedEntityId = targetId.Value,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
            TempData["SupportSuccess"] = "✅ Your report was received successfully!";
            return RedirectToAction("Suport");
        }
        public async Task<IActionResult> Notifications()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            var notifications = (await _notifRepo.GetByUserIdAsync(userId.Value)).ToList();
            var unreadCount = await _notifRepo.GetUnreadCountAsync(userId.Value);

            return View(new NotificationViewModel
            {
                Notifications = notifications,
                UnreadCount = unreadCount,
                TotalCount = notifications.Count
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkNotificationRead(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");
            await _notifRepo.MarkAsReadAsync(id);
            return RedirectToAction("Notifications");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllNotificationsRead()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");
            await _notifRepo.MarkAllAsReadAsync(userId.Value);
            return RedirectToAction("Notifications");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
