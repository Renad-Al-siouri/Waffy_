using Microsoft.AspNetCore.Mvc;
using Waffy.Interfaces;
using Waffy.Models;

namespace Waffy.Controllers
{
    public class ProfileController : Controller
    {
        private readonly IUserRepository _userRepository;

        public ProfileController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
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
            return View(user);
        }

        public IActionResult My_Wallet() => View();
        public IActionResult Rewards() => View();
        public IActionResult Appointments() => View();
        public IActionResult Orders() => View();
        public IActionResult Lost_Reports() => View();
        public IActionResult Adoption_Ads() => View();
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