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
        public ProfileController(
            IUserRepository userRepository,
            IPetRepository petRepo,
            IAdoptionRepository adoptionRepo,  
            ILostPetRepository lostPetRepo,
            IReservationRepository reservationRepo,
            IFoundPetClaimRepository claimRepo)
        {
            _userRepository = userRepository;
            _petRepo = petRepo;
            _adoptionRepo = adoptionRepo;   
            _lostPetRepo = lostPetRepo;
            _reservationRepo = reservationRepo;
            _claimRepo = claimRepo;
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

        public IActionResult My_Wallet() => View();
        public IActionResult Rewards() => View();
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
        public IActionResult Timeline() => View();
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