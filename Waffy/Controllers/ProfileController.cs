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
        public ProfileController(
            IUserRepository userRepository,
            IPetRepository petRepo,
            IAdoptionRepository adoptionRepo,  
            ILostPetRepository lostPetRepo,
            IReservationRepository reservationRepo,
            IFoundPetClaimRepository claimRepo,
            IDonationRepository donationRepo,
            IDangerousAnimalReportRepository dangerRepo,
            IWalletRepository walletRepo)
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

            // ✅ مكافآت الـ finder من FoundPetClaim
            var finderRewards = (await _claimRepo.GetByUserIdAsync(user.UserId))
                .Where(c => c.Status == "Confirmed" && c.RewardAmount > 0)
                .ToList();

            var wallet = await _walletRepo.GetByUserIdAsync(user.UserId);
            var transactions = (await _walletRepo.GetTransactionsByUserIdAsync(user.UserId)).ToList();

            return View(new WalletViewModel
            {
                User = user,
                Wallet = wallet,
                Transactions = transactions,
                FinderRewards = finderRewards
            });
        }
        public IActionResult Appointments() => View();
        public IActionResult Orders() => View();
        // ════════════════════════════════════════════════════
        // ADOPTION ADS — GET
        // ════════════════════════════════════════════════════
        public async Task<IActionResult> Adoption_Ads()
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return RedirectToAction("Login", "Account");

            var posts = (await _adoptionRepo.GetByUserIdAsync(user.UserId)).ToList();
            var reservations = new Dictionary<int, List<AdoptionReservation>>();
            foreach (var post in posts)
            {
                var reqs = (await _reservationRepo.GetByPostIdAsync(post.IdPost)).ToList();
                reservations[post.IdPost] = reqs;
            }

            var myRequests = (await _reservationRepo.GetByRequesterIdAsync(user.UserId)).ToList();

            return View(new AdoptionAdsViewModel
            {
                User = user,
                Posts = posts,
                Reservations = reservations,
                MyRequests = myRequests
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

        // ════════════════════════════════════════════════════
        // CLOSE LOST REPORT — POST
        // ════════════════════════════════════════════════════
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CloseLostReport(int reportId)
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return RedirectToAction("Login", "Account");

            await _lostPetRepo.CloseReportAsync(reportId);

            TempData["LostSuccess"] = "Report marked as found.";
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

            // ✅ 5 — Wallet Transactions
            // ✅ 5 — Wallet Transactions (بدون Donation لأنها ستظهر بشكل منفصل)
            var transactions = await _walletRepo.GetTransactionsByUserIdAsync(user.UserId);
            foreach (var t in transactions.Where(t => t.Type != "Donation"))
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
        public IActionResult Complaints() => View();
        // =========================
        // Security — GET
        // =========================
        [HttpGet]
        public async Task<IActionResult> Security()
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return RedirectToAction("Login", "Account");
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
    }
}