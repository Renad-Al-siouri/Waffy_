using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Waffy.Interfaces;
using Waffy.Models;
using Waffy.Models.ViewModels;

namespace Waffy.Controllers
{
    public class ProfileController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly IPetRepository _petRepo;
        private readonly IAdoptionRepository _adoptionRepo;   
        private readonly ILostPetRepository _lostPetRepo;
        private readonly IReservationRepository _reservationRepo;
        private readonly IFoundPetClaimRepository _claimRepo;
        private readonly IWalletRepository _walletRepo;
        private readonly IDonationRepository _donationRepo;
        private readonly IDangerousAnimalReportRepository _dangerRepo;
        private readonly IComplaintRepository _complaintRepo;
        private readonly IClinicRepository _clinicRepo;
        private readonly IStoreRepository _storeRepo;
        private readonly IOrderRepository _orderRepo;
        private readonly IOrderItemRepository _orderItemRepo;
        private readonly IPostFlagRepository _flagRepo;
        private readonly ILocationRepository _locationRepo;
        private readonly IRewardLogRepository _rewardLogRepo;
        private readonly INotificationRepository _notifRepo;

        public ProfileController(
            IUserRepository userRepository,
            IPetRepository petRepo,
            IAdoptionRepository adoptionRepo,
            ILostPetRepository lostPetRepo,
            IReservationRepository reservationRepo,
            IFoundPetClaimRepository claimRepo,
            IDonationRepository donationRepo,
            IDangerousAnimalReportRepository dangerRepo,
            IComplaintRepository complaintRepo,
            IClinicRepository clinicRepo,
            IStoreRepository storeRepo,
            IWalletRepository walletRepo,
            IOrderRepository orderRepo,
            IOrderItemRepository orderItemRepo,
            IPostFlagRepository flagRepo,
                IRewardLogRepository rewardLogRepo,
                INotificationRepository notifRepo,
            ILocationRepository locationRepo)
        {
            _userRepository = userRepository;
            _petRepo = petRepo;
            _adoptionRepo = adoptionRepo;   
            _lostPetRepo = lostPetRepo;
            _reservationRepo = reservationRepo;
            _claimRepo = claimRepo;
            _walletRepo = walletRepo;
            _donationRepo = donationRepo;
            _dangerRepo = dangerRepo;
            _complaintRepo = complaintRepo;
            _clinicRepo = clinicRepo;
            _storeRepo = storeRepo;
            _orderRepo = orderRepo;
            _orderItemRepo = orderItemRepo;
            _flagRepo = flagRepo;
            _locationRepo = locationRepo;
            _rewardLogRepo = rewardLogRepo;
            _notifRepo = notifRepo;
        }

        // ── helper: تجلب المستخدم من Session أو ترجع null ──
        private async Task<User?> GetCurrentUserAsync()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return null;
            return await _userRepository.GetByIdAsync(userId.Value);
        }

        // =========================
        // Edit Profile — GET
        // =========================
        [HttpGet]
        public async Task<IActionResult> Edit_Profile()
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return RedirectToAction("Login", "Account");

            var wallet = await _walletRepo.GetByUserIdAsync(user.UserId);
            ViewBag.SidebarBalance = (wallet?.Balance ?? 0).ToString("F2");

            return View(user);
        }

        // =========================
        // Edit Profile — POST
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit_Profile(
            string fullName,
            string email,
            string? dateOfBirth,
            string? gender,
            string? bio,
            IFormFile? avatarFile)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            var user = await _userRepository.GetByIdAsync(userId.Value);
            if (user == null) return RedirectToAction("Login", "Account");

            // التحقق من الإيميل إذا تغيّر
            if (user.Email != email)
            {
                var existing = await _userRepository.GetByEmailAsync(email);
                if (existing != null)
                {
                    TempData["ProfileError"] = "This email is already in use.";
                    return View(user);
                }
            }

            // رفع الصورة إذا وُجدت
            if (avatarFile != null && avatarFile.Length > 0)
            {
                var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "avatars");
                Directory.CreateDirectory(folder);
                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(avatarFile.FileName)}";
                using var stream = new FileStream(Path.Combine(folder, fileName), FileMode.Create);
                await avatarFile.CopyToAsync(stream);
                user.ProfileImage = $"/uploads/avatars/{fileName}";
            }

            user.FullName = fullName;
            user.Email = email;
            user.Gender = gender;
            user.Bio = bio;

            await _userRepository.UpdateAsync(user);

            TempData["ProfileSuccess"] = "Profile updated successfully!";
            return RedirectToAction("Edit_Profile");
        }

        // =========================
        // Contact Info — GET
        // =========================
        [HttpGet]
        public async Task<IActionResult> Contact_Info()
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return RedirectToAction("Login", "Account");

            var wallet = await _walletRepo.GetByUserIdAsync(user.UserId);
            ViewBag.SidebarBalance = (wallet?.Balance ?? 0).ToString("F2");

            await LoadLocationsToViewBag();
            return View(user);
        }

        // =========================
        // Contact Info — POST
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Contact_Info(
            string phone,
            string? whatsApp,
            string? city,
            string? area,
            string? addressDetail)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            var user = await _userRepository.GetByIdAsync(userId.Value);
            if (user == null) return RedirectToAction("Login", "Account");

            user.Phone = phone;
            user.City = city;
            user.Area = area;
            user.AddressDetail = addressDetail;

            await _userRepository.UpdateAsync(user);

            TempData["ContactSuccess"] = "Contact info saved successfully!";
            return RedirectToAction("Contact_Info");
        }

        // =========================
        // باقي الصفحات — GET فقط
        // =========================
       
      
        public async Task<IActionResult> Index()
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return RedirectToAction("Login", "Account");

            var pets = await _petRepo.GetByUserIdAsync(user.UserId);

            var vm = new PetProfileViewModel
            {
                User = user,
                Pets = pets.ToList()
            };

            return View(vm);
        }

        // ════════════════════════════════════════════════════
        // MY WALLET — GET
        // ════════════════════════════════════════════════════

        public async Task<IActionResult> My_Wallet()
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return RedirectToAction("Login", "Account");

            var wallet = await _walletRepo.GetByUserIdAsync(user.UserId);
            var transactions = (await _walletRepo.GetTransactionsByUserIdAsync(user.UserId)).ToList();

            return View(new WalletViewModel
            {
                User = user,
                Wallet = wallet,
                Transactions = transactions
            });
        }

        // ════════════════════════════════════════════════════
        // TOP UP WALLET — POST (Simulated)
        // ════════════════════════════════════════════════════
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TopUpWallet(decimal amount, string paymentMethod)
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return RedirectToAction("Login", "Account");

            if (amount <= 0 || amount > 1000)
            {
                TempData["WalletError"] = "Invalid amount. Please enter between 1 and 1000 JOD.";
                return RedirectToAction("My_Wallet");
            }

            // ✅ جلب أو إنشاء الـ Wallet
            var wallet = await _walletRepo.GetByUserIdAsync(user.UserId);
            if (wallet == null)
                await _walletRepo.CreateWalletAsync(user.UserId);

            wallet = await _walletRepo.GetByUserIdAsync(user.UserId);

            // ✅ أضف المبلغ للـ Wallet
            await _walletRepo.AddBalanceAsync(wallet!.IdWallet, amount);

            // ✅ سجّل الـ Transaction
            await _walletRepo.AddTransactionAsync(new WalletTransaction
            {
                IdWallet = wallet.IdWallet,
                TransactionCode = $"WAF-{Guid.NewGuid().ToString()[..8].ToUpper()}",
                Type = "TopUp",
                Amount = amount,
                PaymentMethod = paymentMethod,
                Status = "Completed",
                CreatedAt = DateTime.UtcNow
            });

            TempData["WalletSuccess"] = $"✅ {amount:F2} JOD added to your wallet successfully!";
            return RedirectToAction("My_Wallet");
        }

        // ════════════════════════════════════════════════════
        // REWARDS — GET
        // ════════════════════════════════════════════════════
        public async Task<IActionResult> Rewards()
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return RedirectToAction("Login", "Account");

            var finderRewards = (await _claimRepo.GetByUserIdAsync(user.UserId))
                .Where(c => c.Status == "Confirmed" && c.RewardAmount > 0)
                .ToList();

            var surpriseRewards = (await _rewardLogRepo.GetByUserIdAsync(user.UserId)).ToList();

            var wallet = await _walletRepo.GetByUserIdAsync(user.UserId);
            var transactions = (await _walletRepo.GetTransactionsByUserIdAsync(user.UserId)).ToList();

            return View(new WalletViewModel
            {
                User = user,
                Wallet = wallet,
                Transactions = transactions,
                FinderRewards = finderRewards,
                SurpriseRewards = surpriseRewards    // ← جديد
            });
        }
        public async Task<IActionResult> Appointments()
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return RedirectToAction("Login", "Account");

            var wallet = await _walletRepo.GetByUserIdAsync(user.UserId);
            ViewBag.SidebarBalance = (wallet?.Balance ?? 0).ToString("F2");

            return View();
        }


        [HttpGet]
        public async Task<IActionResult> Orders()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            var wallet = await _walletRepo.GetByUserIdAsync(userId.Value);
            ViewBag.SidebarBalance = (wallet?.Balance ?? 0).ToString("F2");
            // جلب طلبات هذا اليوزر
            var orders = (await _orderRepo.GetByUserIdAsync(userId.Value))
                         .OrderByDescending(o => o.CreatedAt)
                         .ToList();

            // جلب أسماء المتاجر
            var storeIds = orders.Select(o => o.IdStore).Distinct().ToList();
            var stores = new Dictionary<int, string>();
            foreach (var id in storeIds)
            {
                var s = await _storeRepo.GetByIdAsync(id);
                stores[id] = s?.Name ?? "Store";
            }

            ViewBag.StoreNames = stores;
            ViewBag.FullName = HttpContext.Session.GetString("FullName");
            ViewBag.Email = HttpContext.Session.GetString("Email");

            return View(orders);
        }

        // AJAX endpoint لجلب عناصر الطلب داخل الـ modal
        [HttpGet]
        public async Task<IActionResult> OrderItems(int orderId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Json(new { items = new List<object>(), total = "0.000" });

            // تأكد أن الطلب ينتمي لهذا اليوزر
            var order = await _orderRepo.GetByIdAsync(orderId);
            if (order == null || order.IdUser != userId.Value)
                return Json(new { items = new List<object>(), total = "0.000" });

            var items = (await _orderItemRepo.GetByOrderIdAsync(orderId)).ToList();
            var total = items.Sum(i => i.Quantity * (i.SalePrice ?? i.UnitPrice));

            return Json(new
            {
                items = items.Select(i => new
                {
                    name = i.ProductNameSnapshot,
                    qty = i.Quantity,
                    subtotal = (i.Quantity * (i.SalePrice ?? i.UnitPrice)).ToString("F3")
                }),
                total = total.ToString("F3")
            });
        }

        // ════════════════════════════════════════════════════
        // ADOPTION ADS — GET
        // ════════════════════════════════════════════════════
        public async Task<IActionResult> Adoption_Ads()
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return RedirectToAction("Login", "Account");

            var wallet = await _walletRepo.GetByUserIdAsync(user.UserId);
            ViewBag.SidebarBalance = (wallet?.Balance ?? 0).ToString("F2");

            var posts = (await _adoptionRepo.GetByUserIdAsync(user.UserId)).ToList();
            var reservations = new Dictionary<int, List<AdoptionReservation>>();
            foreach (var post in posts)
            {
                var reqs = (await _reservationRepo.GetByPostIdAsync(post.IdPost)).ToList();
                reservations[post.IdPost] = reqs;
            }

            var myRequests = (await _reservationRepo.GetByRequesterIdAsync(user.UserId)).ToList();

            var postIds = posts.Select(p => p.IdPost);
            var flagCounts = await _flagRepo.GetFlagCountsForPostsAsync(postIds);

            return View(new AdoptionAdsViewModel
            {
                User = user,
                Posts = posts,
                Reservations = reservations,
                MyRequests = myRequests,
                FlagCounts = flagCounts   
            });
        }


            

        // ════════════════════════════════════════════════════
        // EDIT ADOPTION POST — POST
        // ════════════════════════════════════════════════════
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAdoptionPost(
            int postId,
            string? city,
            string? area,
            string? contactPhone,
            string? description,
            string? reasonForAdoption,
            string? adoptionStatus)
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return RedirectToAction("Login", "Account");

            var post = new AdoptionPost
            {
                IdPost = postId,
                PostedById = user.UserId,
                City = city,
                Area = area,
                ContactPhone = contactPhone,
                Description = description,
                ReasonForAdoption = reasonForAdoption,
                AdoptionStatus = adoptionStatus ?? "Available"
            };

            await _adoptionRepo.UpdatePostAsync(post);

            TempData["AdoptionSuccess"] = "Ad updated successfully!";
            return RedirectToAction("Adoption_Ads");
        }

        // ════════════════════════════════════════════════════
        // CLOSE ADOPTION POST — POST
        // ════════════════════════════════════════════════════
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CloseAdoptionPost(int postId)
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return RedirectToAction("Login", "Account");

            await _adoptionRepo.ClosePostAsync(postId);

            TempData["AdoptionSuccess"] = "Ad closed successfully.";
            return RedirectToAction("Adoption_Ads");
        }

        // ════════════════════════════════════════════════════
        // LOST REPORTS — GET
        // ════════════════════════════════════════════════════
        public async Task<IActionResult> Lost_Reports()
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return RedirectToAction("Login", "Account");
            var wallet = await _walletRepo.GetByUserIdAsync(user.UserId);
            ViewBag.SidebarBalance = (wallet?.Balance ?? 0).ToString("F2");
            var reports = (await _lostPetRepo.GetByUserIdAsync(user.UserId)).ToList();

            // ✅ Finders لكل تقرير (لصاحب الحيوان)
            var finders = new Dictionary<int, List<FoundPetClaim>>();
            foreach (var r in reports)
            {
                var claims = (await _claimRepo.GetByReportIdAsync(r.IdLostReport)).ToList();
                finders[r.IdLostReport] = claims;
            }

            // ✅ Claims قدّمها المستخدم نفسه (لتتبع العملية)
            var myClaims = (await _claimRepo.GetByUserIdAsync(user.UserId)).ToList();

            return View(new LostReportsViewModel
            {
                User = user,
                Reports = reports,
                Finders = finders,
                MyClaims = myClaims
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RenewReport(int reportId, string duration)
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return RedirectToAction("Login", "Account");

            var canRenew = await _lostPetRepo.CanRenewAsync(reportId);
            if (!canRenew)
            {
                TempData["LostSuccess"] = "⚠️ This report cannot be renewed.";
                return RedirectToAction("Lost_Reports");
            }

            int days = duration switch
            {
                "7 Days" => 7,
                "30 Days" => 30,
                _ => 14
            };

            await _lostPetRepo.RenewReportAsync(
                reportId,
                DateTime.UtcNow.AddDays(days));

            TempData["LostSuccess"] =
                $"✅ Report renewed for {days} days — FREE! If your pet is still not found, " +
                "you'll get a 50% discount on your next post.";
            return RedirectToAction("Lost_Reports");
        }

        // ════════════════════════════════════════════════════
        // CLOSE LOST REPORT — POST
        // ════════════════════════════════════════════════════
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CloseLostReport(int reportId)
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return RedirectToAction("Login", "Account");
            var report = await _lostPetRepo.GetByIdAsync(reportId);

            // ✅ إرجاع المكافأة إذا كانت محجوزة
            if (report?.RewardReserved == true
    && report.RewardAmount > 0
    && report.RewardStatus != "Released")
            {
                var wallet = await _walletRepo.GetByUserIdAsync(user!.UserId);
                if (wallet != null)
                {
                    await _walletRepo.AddBalanceAsync(
                        wallet.IdWallet, report.RewardAmount.Value);

                    await _walletRepo.AddTransactionAsync(new WalletTransaction
                    {
                        IdWallet = wallet.IdWallet,
                        TransactionCode = $"RET-{Guid.NewGuid().ToString()[..8].ToUpper()}",
                        Type = "RewardReturn",
                        Amount = report.RewardAmount.Value, // ← موجب (إرجاع)
                        PaymentMethod = "Wallet",
                        Status = "Completed",
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            await _lostPetRepo.CloseReportAsync(reportId);
            TempData["LostSuccess"] = "Report marked as found.";
            return RedirectToAction("Lost_Reports");
        }


        // ════════════════════════════════════════════════════
        // CONFIRM FOUND PET — POST
        // المالك يؤكد أن هذا الشخص وجد حيوانه
        // ════════════════════════════════════════════════════
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmFoundPet(int claimId)
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return RedirectToAction("Login", "Account");

            // ── جلب الـ Claim والـ Report ──────────────────────
            var claim = await _claimRepo.GetByIdAsync(claimId);
            if (claim == null)
            {
                TempData["LostSuccess"] = "⚠️ Claim not found.";
                return RedirectToAction("Lost_Reports");
            }

            var report = await _lostPetRepo.GetByIdAsync(claim.IdLostReport);
            if (report == null || report.IdUser != user.UserId)
            {
                TempData["LostSuccess"] = "⚠️ You are not authorized.";
                return RedirectToAction("Lost_Reports");
            }
            // ── تأكيد الـ Claim المختار ─────────────────────
            await _claimRepo.ConfirmClaimAsync(claimId);
            // ── رفض باقي الـ Claims تلقائياً ──────────────────
            var allClaims = await _claimRepo.GetByReportIdAsync(report.IdLostReport);
            foreach (var other in allClaims.Where(c => c.IdClaim != claimId && c.Status == "Pending"))
            {
                await _claimRepo.RejectClaimAsync(other.IdClaim);

                if (other.IdUser.HasValue)
                {
                    await _notifRepo.AddAsync(new Notification
                    {
                        IdUser = other.IdUser.Value,
                        Type = "ClaimRejected",
                        Title = "❌ Claim not confirmed",
                        Message = $"Another person was confirmed as the finder of {report.PetName}. Thank you for your help! 🐾",
                        IsRead = false,
                        RelatedEntityType = "LostPetReport",
                        RelatedEntityId = report.IdLostReport,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            // ── نقل المكافأة إذا كانت محجوزة ────────────────
            if (report.RewardAmount > 0
                && report.RewardReserved
                && report.RewardStatus != "Released"
                && claim.IdUser.HasValue)
            {
                var finderId = claim.IdUser.Value;

                // جلب أو إنشاء محفظة الـ Finder
                var finderWallet = await _walletRepo.GetByUserIdAsync(finderId);
                if (finderWallet == null)
                {
                    await _walletRepo.CreateWalletAsync(finderId);
                    finderWallet = await _walletRepo.GetByUserIdAsync(finderId);
                }

                if (finderWallet != null)
                {
                    // ✅ أضف المكافأة لمحفظة الـ Finder
                    await _walletRepo.AddBalanceAsync(
                        finderWallet.IdWallet, report.RewardAmount.Value);

                    await _walletRepo.AddTransactionAsync(new WalletTransaction
                    {
                        IdWallet = finderWallet.IdWallet,
                        TransactionCode = $"RWD-{Guid.NewGuid().ToString()[..8].ToUpper()}",
                        Type = "FinderReward",
                        Amount = report.RewardAmount.Value,
                        PaymentMethod = "Waffy Reward",
                        Status = "Completed",
                        CreatedAt = DateTime.UtcNow
                    });
                }

                // ✅ سجّل في محفظة المالك كـ "تم تحرير المكافأة"
                var ownerWallet = await _walletRepo.GetByUserIdAsync(user.UserId);
                if (ownerWallet != null)
                {
                    await _walletRepo.AddTransactionAsync(new WalletTransaction
                    {
                        IdWallet = ownerWallet.IdWallet,
                        TransactionCode = $"REL-{Guid.NewGuid().ToString()[..8].ToUpper()}",
                        Type = "RewardRelease",
                        Amount = -report.RewardAmount.Value,   // سالب = خرج من المحفظة
                        PaymentMethod = "Waffy Reward",
                        Status = "Completed",
                        CreatedAt = DateTime.UtcNow
                    });
                }

                // ✅ حدّث RewardStatus = "Released"
                await _lostPetRepo.UpdateRewardStatusAsync(report.IdLostReport, "Released");

                // ✅ إشعار للـ Finder بالمكافأة
                var finder = await _userRepository.GetByIdAsync(finderId);
                await _notifRepo.AddAsync(new Notification
                {
                    IdUser = finderId,
                    Type = "RewardTransferred",
                    Title = "🎁 Reward received!",
                    Message = $"Congratulations! {report.RewardAmount:F2} JOD has been added to your wallet " +
                              $"as a reward for finding {report.PetName}. Thank you! 🐾",
                    IsRead = false,
                    RelatedEntityType = "LostPetReport",
                    RelatedEntityId = report.IdLostReport,
                    CreatedAt = DateTime.UtcNow
                });
            }

            // ── أغلق البوست ──────────────────────────────────
            await _lostPetRepo.CloseReportAsync(report.IdLostReport);

            // ── إشعار للمالك نفسه ─────────────────────────────
            await _notifRepo.AddAsync(new Notification
            {
                IdUser = user.UserId,
                Type = "PetFound",
                Title = "✅ Pet confirmed as found!",
                Message = $"You confirmed that {report.PetName} has been found. " +
                          (report.RewardAmount > 0
                              ? $"The reward of {report.RewardAmount:F2} JOD has been transferred to the finder."
                              : "Thank you for using Waffy! 🐾"),
                IsRead = false,
                RelatedEntityType = "LostPetReport",
                RelatedEntityId = report.IdLostReport,
                CreatedAt = DateTime.UtcNow
            });

            TempData["LostSuccess"] = report.RewardAmount > 0
                ? $"✅ Finder confirmed! {report.RewardAmount:F2} JOD reward transferred."
                : "✅ Finder confirmed! Report closed.";

            return RedirectToAction("Lost_Reports");
        }

        // ════════════════════════════════════════════════════
        // REJECT CLAIM — POST
        // المالك يرفض هذا الشخص (ليس هو من وجد الحيوان)
        // ════════════════════════════════════════════════════
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectClaim(int claimId)
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return RedirectToAction("Login", "Account");

            var claim = await _claimRepo.GetByIdAsync(claimId);
            if (claim == null)
            {
                TempData["LostSuccess"] = "⚠️ Claim not found.";
                return RedirectToAction("Lost_Reports");
            }

            var report = await _lostPetRepo.GetByIdAsync(claim.IdLostReport);
            if (report == null || report.IdUser != user.UserId)
            {
                TempData["LostSuccess"] = "⚠️ You are not authorized.";
                return RedirectToAction("Lost_Reports");
            }

            await _claimRepo.RejectClaimAsync(claimId);

            // ✅ أبلغ الـ Finder بالرفض
            if (claim.IdUser.HasValue)
            {
                await _notifRepo.AddAsync(new Notification
                {
                    IdUser = claim.IdUser.Value,
                    Type = "ClaimRejected",
                    Title = "❌ Claim not confirmed",
                    Message = $"The owner of {report.PetName} could not verify your claim. " +
                              "If you believe this is an error, please contact support.",
                    IsRead = false,
                    RelatedEntityType = "LostPetReport",
                    RelatedEntityId = report.IdLostReport,
                    CreatedAt = DateTime.UtcNow
                });
            }

            TempData["LostSuccess"] = "❌ Claim rejected and finder notified.";
            return RedirectToAction("Lost_Reports");
        }
        // ════════════════════════════════════════════════════
        // TIMELINE — GET
        // ════════════════════════════════════════════════════
        public async Task<IActionResult> Timeline()
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return RedirectToAction("Login", "Account");

            var events = new List<TimelineEvent>();

            // ✅ 1 — تسجيل الحساب
            events.Add(new TimelineEvent
            {
                Icon = "🎉",
                Title = "Account Created",
                Description = "Welcome to Waffy!",
                Date = user.CreatedAt,
                Color = "#7bb33a"
            });

            // ✅ 2 — Lost Reports
            var reports = await _lostPetRepo.GetByUserIdAsync(user.UserId);
            foreach (var r in reports)
            {
                events.Add(new TimelineEvent
                {
                    Icon = "🔍",
                    Title = $"Lost Pet Report — {r.PetName}",
                    Description = $"{r.City} · {(r.RewardAmount > 0 ? $"Reward: {r.RewardAmount} JOD" : "No reward")}",
                    Date = r.CreatedAt,
                    Color = "#e65100"
                });
            }

            // ✅ 3 — Adoption Posts
            var posts = await _adoptionRepo.GetByUserIdAsync(user.UserId);
            foreach (var p in posts)
            {
                events.Add(new TimelineEvent
                {
                    Icon = "🐾",
                    Title = $"Adoption Ad — {p.PetName}",
                    Description = $"{p.AnimalType} · {p.City} · {p.AdoptionStatus}",
                    Date = p.CreatedAt,
                    Color = "#7bb33a"
                });
            }

            // ✅ 4 — Finder Claims (كـ finder)
            var myClaims = await _claimRepo.GetByUserIdAsync(user.UserId);
            foreach (var c in myClaims)
            {
                events.Add(new TimelineEvent
                {
                    Icon = c.Status == "Confirmed" ? "🎁" : "🔎",
                    Title = c.Status == "Confirmed"
                                    ? $"Found Pet Reward — {c.PetName}"
                                    : $"Found Pet Claim — {c.PetName}",
                    Description = c.Status == "Confirmed" && c.RewardAmount > 0
                                    ? $"+{c.RewardAmount} JOD added to wallet"
                                    : $"Status: {c.Status}",
                    Date = c.CreatedAt ?? DateTime.UtcNow,
                    Color = c.Status == "Confirmed" ? "#2d7a2d" : "#7bb33a"
                });
            }

            // ✅ 5 — Wallet Transactions (بدون Donation لأنها ستظهر بشكل منفصل)
            var transactions = await _walletRepo.GetTransactionsByUserIdAsync(user.UserId);
            foreach (var t in transactions.Where(t =>
     t.Type != "Donation" &&
     t.Type != "LostPetFee" &&
     t.Type != "RewardHold" &&
     t.Type != "RewardReturn"))
            {
                events.Add(new TimelineEvent
                {
                    Icon = t.Amount > 0 ? "💰" : "💸",
                    Title = t.Amount > 0
                                    ? $"Wallet Top-up — +{t.Amount} JOD"
                                    : $"Payment — {Math.Abs(t.Amount)} JOD",
                    Description = t.PaymentMethod ?? t.Type ?? "",
                    Date = t.CreatedAt ?? DateTime.UtcNow,
                    Color = t.Amount > 0 ? "#2d7a2d" : "#b91c1c"
                });
            }

            // ✅ Lost Pet Fee Transactions
            foreach (var t in transactions.Where(t => t.Type == "LostPetFee"))
            {
                events.Add(new TimelineEvent
                {
                    Icon = "💳",
                    Title = "Lost Pet Posting Fee",
                    Description = $"{Math.Abs(t.Amount):F2} JOD deducted · Wallet",
                    Date = t.CreatedAt ?? DateTime.UtcNow,
                    Color = "#b91c1c"
                });
            }

            // ✅ Reward Hold Transactions
            foreach (var t in transactions.Where(t => t.Type == "RewardHold"))
            {
                events.Add(new TimelineEvent
                {
                    Icon = "🔒",
                    Title = "Reward Reserved",
                    Description = $"{Math.Abs(t.Amount):F2} JOD held in wallet for finder reward",
                    Date = t.CreatedAt ?? DateTime.UtcNow,
                    Color = "#9b6000"
                });
            }

            foreach (var t in transactions.Where(t => t.Type == "RewardReturn"))
            {
                events.Add(new TimelineEvent
                {
                    Icon = "↩️",
                    Title = "Reward Returned",
                    Description = $"+{t.Amount:F2} JOD returned — pet not found via finder",
                    Date = t.CreatedAt ?? DateTime.UtcNow,
                    Color = "#2d7a2d"
                });
            }

            // ✅ 6 — Donations (جديد)
            var donations = await _donationRepo.GetByUserIdAsync(user.UserId);
            foreach (var d in donations)
            {
                var icon = d.DonationType switch
                {
                    "Money" => "💵",
                    "Food" => "🍖",
                    "Medicine" => "💊",
                    _ => "📦"
                };
                events.Add(new TimelineEvent
                {
                    Icon = icon,
                    Title = $"Donation — {d.DonationType}",
                    Description = d.DonationType == "Money"
                        ? $"{d.Amount:F2} JOD donated · Ref: {d.ReferenceCode}"
                        : $"Ref: {d.ReferenceCode} · {d.Description}",
                    Date = d.CreatedAt ?? DateTime.UtcNow,
                    Color = "#5a9ecf"
                });
            }
            // ✅ 7 — Danger Reports
            var dangerReports = await _dangerRepo.GetByUserIdAsync(user.UserId);
            foreach (var dr in dangerReports)
            {
                var severityIcon = dr.Severity switch
                {
                    "High" => "🔴",
                    "Medium" => "🟠",
                    _ => "🟢"
                };
                events.Add(new TimelineEvent
                {
                    Icon = "🚨",
                    Title = $"Danger Report — {dr.AnimalType}",
                    Description = $"{severityIcon} {dr.Severity} · {dr.City} · Ref: {dr.ReportCode}",
                    Date = dr.CreatedAt ?? DateTime.UtcNow,
                    Color = dr.Severity == "High" ? "#dc2626" :
                                  dr.Severity == "Medium" ? "#b36d2c" : "#2d7a2d"
                });
            }
            // ✅ 8 — Complaints
            var complaints = await _complaintRepo.GetByUserIdAsync(user.UserId);
            foreach (var comp in complaints)
            {
                events.Add(new TimelineEvent
                {
                    Icon = "📢",
                    Title = $"Complaint — {comp.TargetType}",
                    Description = $"{comp.Title} · Status: {comp.Status} · Priority: {comp.Priority}",
                    Date = comp.CreatedAt ?? DateTime.UtcNow,
                    Color = comp.Priority == "High" || comp.Priority == "Urgent"
                                    ? "#dc2626" : "#b36d2c"
                });
            }

            // ✅ Surprise Rewards من الأدمن
            var surpriseRewards = (await _rewardLogRepo.GetByUserIdAsync(user.UserId)).ToList();
            foreach (var sr in surpriseRewards)
            {
                var icon = sr.RewardType switch
                {
                    "WalletCredit" => "💰",
                    "StoreDiscount" => "🏪",
                    "ClinicSession" => "🏥",
                    "DeliveryVoucher" => "🚚",
                    _ => "🎁"
                };
                events.Add(new TimelineEvent
                {
                    Icon = icon,
                    Title = $"Surprise Reward — {sr.RewardTitle}",
                    Description = $"{sr.GiftTypeLabel} · {sr.ReasonCategory}" +
                                  (sr.ReasonNote != null ? $" · {sr.ReasonNote}" : "") +
                                  $" | {sr.RewardDisplay}",
                    Date = sr.CreatedAt,
                    Color = "#9b6000"
                });
            }

            // إبلاغات قدّمها المستخدم
            var myFlags = await _flagRepo.GetByUserIdAsync(user.UserId);
            foreach (var f in myFlags)
            {
                events.Add(new TimelineEvent
                {
                    Icon = "🚩",
                    Title = "Post Reported",
                    Description = $"You reported a post as: {f.Reason}",
                    Date = f.CreatedAt,
                    Color = "#dc2626"
                });
            }

            // ✅ رتّب حسب التاريخ — الأحدث أولاً
            events = events.OrderByDescending(e => e.Date).ToList();

            return View(new WalletViewModel
            {
                User = user,
                Wallet = await _walletRepo.GetByUserIdAsync(user.UserId),
                TimelineEvents = events
            });
        }

        // ════════════════════════════════════════════════════
        // MY DONATIONS — GET
        // ════════════════════════════════════════════════════
        public async Task<IActionResult> My_Donations()
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return RedirectToAction("Login", "Account");

            var wallet = await _walletRepo.GetByUserIdAsync(user.UserId);
            var donations = (await _donationRepo.GetByUserIdAsync(user.UserId)).ToList();

            return View(new WalletViewModel
            {
                User = user,
                Wallet = wallet,
                Donations = donations
            });
        }

        // ════════════════════════════════════════════════════
        // MY DANGER REPORTS — GET
        // ════════════════════════════════════════════════════
        public async Task<IActionResult> My_Danger_Reports()
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return RedirectToAction("Login", "Account");

            var wallet = await _walletRepo.GetByUserIdAsync(user.UserId);
            var reports = (await _dangerRepo.GetByUserIdAsync(user.UserId)).ToList();

            return View(new WalletViewModel
            {
                User = user,
                Wallet = wallet,
                DangerReports = reports
            });
        }
        // ════════════════════════════════════════════════════
        // COMPLAINTS — GET
        // ════════════════════════════════════════════════════
        public async Task<IActionResult> Complaints()
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return RedirectToAction("Login", "Account");

            var wallet = await _walletRepo.GetByUserIdAsync(user.UserId);
            var complaints = (await _complaintRepo.GetByUserIdAsync(user.UserId)).ToList();
            // ✅ المعتمدون فقط
            var clinics = (await _clinicRepo.GetAllAsync())
                .Where(c => c.VerificationStatus == "Approved").ToList();
            var stores = (await _storeRepo.GetAllAsync())
                .Where(s => s.VerificationStatus == "Approved").ToList();

            ViewBag.Clinics = clinics;
            ViewBag.Stores = stores;

            return View(new WalletViewModel
            {
                User = user,
                Wallet = wallet,
                Complaints = complaints
            });
        }

        // ════════════════════════════════════════════════════
        // SUBMIT COMPLAINT — POST
        // ════════════════════════════════════════════════════
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitComplaint(
            string targetType,
            int? targetId,
            string title,
            string description,
            string priority)
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return RedirectToAction("Login", "Account");

            if (string.IsNullOrWhiteSpace(title) ||
                string.IsNullOrWhiteSpace(description))
            {
                TempData["ComplaintError"] = "Please fill in all required fields.";
                return RedirectToAction("Complaints");
            }

            await _complaintRepo.CreateAsync(new Complaint
            {
                IdUser = user.UserId,
                TargetType = targetType,
                TargetId = targetType == "System" ? null : targetId,
                Title = title,
                Description = description,
                Status = "Open",
                Priority = priority ?? "Medium",
                CreatedAt = DateTime.UtcNow
            });

            TempData["ComplaintSuccess"] = "✅ Your complaint was submitted successfully!";
            return RedirectToAction("Complaints");
        }
        // =========================
        // Security — GET
        // =========================
        [HttpGet]
        public async Task<IActionResult> Security()
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return RedirectToAction("Login", "Account");

            var wallet = await _walletRepo.GetByUserIdAsync(user.UserId);
            ViewBag.SidebarBalance = (wallet?.Balance ?? 0).ToString("F2");

            return View(user);
        }

        // =========================
        // Security — POST (Change Password)
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Security(
            string currentPassword,
            string newPassword,
            string confirmPassword)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            var user = await _userRepository.GetByIdAsync(userId.Value);
            if (user == null) return RedirectToAction("Login", "Account");

            // ── التحقق من كلمة المرور الحالية ──
            if (user.PasswordHash != currentPassword)
            {
                TempData["SecurityError"] = "Current password is incorrect.";
                return View(user);
            }

            // ── التحقق من كلمة المرور الجديدة ──
            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 8)
            {
                TempData["SecurityError"] = "New password must be at least 8 characters.";
                return View(user);
            }

            if (newPassword != confirmPassword)
            {
                TempData["SecurityError"] = "Passwords do not match.";
                return View(user);
            }

            if (newPassword == currentPassword)
            {
                TempData["SecurityError"] = "New password must be different from current password.";
                return View(user);
            }

            // ── تحديث كلمة المرور ──
            user.PasswordHash = newPassword;
            await _userRepository.UpdateAsync(user);

            TempData["SecuritySuccess"] = "Password updated successfully!";
            return RedirectToAction("Security");
        }


        // ── helper: Load cities and areas for dropdowns ──
        private async Task LoadLocationsToViewBag()
        {
            var governorates = (await _locationRepo.GetAllGovernoratesAsync()).ToList();
            var districts = (await _locationRepo.GetAllDistrictsAsync()).ToList();

            ViewBag.LocationsJson = System.Text.Json.JsonSerializer.Serialize(
                governorates.Select(g => new
                {
                    id = g.Id,
                    name = g.NameEn,
                    districts = districts
                        .Where(d => d.GovernorateId == g.Id)
                        .OrderBy(d => d.NameEn)
                        .Select(d => d.NameEn)
                        .ToList()
                })
            );
        }
    }
}