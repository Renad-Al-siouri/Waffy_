using Microsoft.AspNetCore.Mvc;
using Waffy.Interfaces;
using Waffy.Models;

namespace Waffy.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserRepository _userRepository;

        public AccountController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        // =========================
        // Login
        // =========================
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                TempData["LoginError"] = "Please enter your email and password.";
                return RedirectToAction("Login");
            }

            var user = await _userRepository.LoginAsync(email, password);

            if (user == null)
            {
                TempData["LoginError"] = "Invalid email or password.";
                return RedirectToAction("Login");
            }

            if (user.AccountStatus != "Active")
            {
                TempData["LoginError"] = "Your account is not active.";
                return RedirectToAction("Login");
            }

            // ✅ FIX: store user id
            HttpContext.Session.SetInt32("UserId", user.UserId);

            TempData["LoginSuccess"] = "Logged in successfully.";

            return user.RoleName switch
            {
                "User" => RedirectToAction("Home_After", "Home"),
                "Clinic" => RedirectToAction("Clinic_step1", "Account"),
                "Store" => RedirectToAction("Stor_step1", "Account"),
                _ => RedirectToAction("Home_After", "Home")
            };
        }        // =========================
        // Signup
        // =========================
        [HttpGet]
        public IActionResult Signup()
        {
            return View();
        }

        [HttpGet]
        public IActionResult user_signup()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> user_signup(
            string fullName,
            string email,
            string password,
            string confirmPassword,
            string phone,
            string city)
        {
            if (string.IsNullOrWhiteSpace(fullName) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(confirmPassword) ||
                string.IsNullOrWhiteSpace(phone) ||
                string.IsNullOrWhiteSpace(city))
            {
                TempData["SignupError"] = "Please fill in all required fields.";
                return RedirectToAction("user_signup");
            }

            if (password != confirmPassword)
            {
                TempData["SignupError"] = "Passwords do not match.";
                return RedirectToAction("user_signup");
            }

            if (password.Length < 6)
            {
                TempData["SignupError"] = "Password must be at least 6 characters.";
                return RedirectToAction("user_signup");
            }

            var existingUser = await _userRepository.GetByEmailAsync(email);
            if (existingUser != null)
            {
                TempData["SignupError"] = "This email is already registered.";
                return RedirectToAction("user_signup");
            }

            var user = new User
            {
                FullName = fullName,
                Email = email,
                PasswordHash = password,
                Phone = phone,
                City = city,
                IsPhoneVerified = false,
                IsEmailVerified = false,
                AccountStatus = "Active"
            };

            await _userRepository.AddNormalUserAsync(user);

            TempData["SignupSuccess"] = "Account created successfully. Please log in.";
            return RedirectToAction("Login");
        }

        // =========================
        // Forgot Password / OTP / Reset
        // =========================
        [HttpGet]
        public IActionResult ForgotPass()
        {
            return View();
        }

        [HttpGet]
        public IActionResult OTP()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ResetPass()
        {
            return View();
        }

        // =========================
        // Logout
        // =========================
        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // =========================
        // Profile
        // =========================
        public async Task<IActionResult> Profile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
                return RedirectToAction("Login");

            var user = await _userRepository.GetByIdAsync(userId.Value);

            if (user == null)
                return NotFound();

            return View(user);
        }
        [HttpGet]
        public IActionResult ProfileEdit()
        {
            return View();
        }

        // =========================
        // Clinic signup steps
        // =========================
        [HttpGet]
        public IActionResult Clinic_step1()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Clinic_step2()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Clinic_step3()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Clinic_step4()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Clinic_step5()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Clinic_success()
        {
            return View();
        }

        // =========================
        // Store signup steps
        // =========================
        [HttpGet]
        public IActionResult Stor_step1()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Stor_step2()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Stor_step3()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Stor_step4()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Stor_step5()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Stor_success()
        {
            return View();
        }
    }
}