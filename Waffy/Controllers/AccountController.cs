using Microsoft.AspNetCore.Mvc;
using Waffy.Interfaces;
using Waffy.Models;

namespace Waffy.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly IStoreRepository _storeRepository;
        private readonly IStoreDocumentRepository _documentRepository;
        private readonly IClinicRepository _clinicRepository;
        private readonly IClinicDocumentRepository _clinicDocumentRepository;

        public AccountController(
            IUserRepository userRepository,
            IStoreRepository storeRepository,
            IStoreDocumentRepository documentRepository,
            IClinicRepository clinicRepository,
            IClinicDocumentRepository clinicDocumentRepository)
        {
            _userRepository = userRepository;
            _storeRepository = storeRepository;
            _documentRepository = documentRepository;
            _clinicRepository = clinicRepository;
            _clinicDocumentRepository = clinicDocumentRepository;
        }

        // ══════════════════════════════════════════════════════════
        // HELPERS
        // ══════════════════════════════════════════════════════════

        /// <summary>حفظ ملف على القرص وإرجاع المسار النسبي</summary>
        private async Task<string> SaveFileAsync(IFormFile file, string subFolder)
        {
            var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", subFolder);
            Directory.CreateDirectory(folder);
            var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            var fullPath = Path.Combine(folder, fileName);
            await using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);
            return $"/uploads/{subFolder}/{fileName}";
        }

        /// <summary>تفعيل الـ Session بعد تسجيل الدخول الناجح</summary>
        private void SetSession(LoginUserResult user)
        {
            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("FullName", user.FullName ?? "");
            HttpContext.Session.SetString("RoleName", user.RoleName ?? "");
        }

        // ══════════════════════════════════════════════════════════
        // LOGIN
        // ══════════════════════════════════════════════════════════

        [HttpGet]
        public IActionResult Login() => View();

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

            // ── حساب متجر — فحص الـ VerificationStatus ──────────
            if (user.RoleName == "Store")
            {
                var store = await _storeRepository.GetByOwnerUserIdAsync(user.UserId);
                if (store == null)
                {
                    TempData["LoginError"] = "No store found for this account.";
                    return RedirectToAction("Login");
                }

                switch (store.VerificationStatus)
                {
                    case "Pending":
                        TempData["LoginError"] = "Your store is under review. Please wait for admin approval.";
                        return RedirectToAction("Login");
                    case "Rejected":
                        TempData["LoginError"] = "Your store application was rejected. Please contact support.";
                        return RedirectToAction("Login");
                    case "Suspended":
                        TempData["LoginError"] = "Your store has been suspended. Please contact support.";
                        return RedirectToAction("Login");
                }

                // ✅ Approved
                SetSession(user);
                return RedirectToAction("Dashboard", "StoreAdmin");
            }

            // ── حساب عيادة — فحص الـ VerificationStatus ─────────
            if (user.RoleName == "Clinic")
            {
                var clinic = await _clinicRepository.GetByOwnerUserIdAsync(user.UserId);
                if (clinic == null)
                {
                    TempData["LoginError"] = "No clinic found for this account.";
                    return RedirectToAction("Login");
                }

                switch (clinic.VerificationStatus)
                {
                    case "Pending":
                        TempData["LoginError"] = "Your clinic is under review. Please wait for admin approval.";
                        return RedirectToAction("Login");
                    case "Rejected":
                        TempData["LoginError"] = "Your clinic application was rejected. Please contact support.";
                        return RedirectToAction("Login");
                    case "Suspended":
                        TempData["LoginError"] = "Your clinic has been suspended. Please contact support.";
                        return RedirectToAction("Login");
                }

                // ✅ Approved
                SetSession(user);
                return RedirectToAction("Dashboard", "ClinicAdmin");
            }

            // ── باقي الأدوار (Admin / User) ───────────────────────
            if (user.AccountStatus != "Active")
            {
                TempData["LoginError"] = "Your account is not active. Please contact support.";
                return RedirectToAction("Login");
            }

            SetSession(user);

            return user.RoleName switch
            {
                "Admin" => RedirectToAction("Users", "SuperAdmin"),
                "User" => RedirectToAction("Home_After", "Home"),
                _ => RedirectToAction("Home_After", "Home")
            };
        }

        // ══════════════════════════════════════════════════════════
        // SIGNUP
        // ══════════════════════════════════════════════════════════

        [HttpGet] public IActionResult Signup() => View();

        // ── User Signup ──────────────────────────────────────────

        [HttpGet]
        public IActionResult user_signup() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> user_signup(
            string fullName, string email, string password,
            string confirmPassword, string phone, string city)
        {
            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(confirmPassword) ||
                string.IsNullOrWhiteSpace(phone) || string.IsNullOrWhiteSpace(city))
            {
                TempData["SignupError"] = "Please fill in all required fields.";
                return RedirectToAction("user_signup");
            }

            if (password != confirmPassword)
            {
                TempData["SignupError"] = "Passwords do not match.";
                return RedirectToAction("user_signup");
            }

            if (password.Length < 8)
            {
                TempData["SignupError"] = "Password must be at least 8 characters.";
                return RedirectToAction("user_signup");
            }

            if (await _userRepository.GetByEmailAsync(email) != null)
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

        // ══════════════════════════════════════════════════════════
        // STORE SIGNUP — Step 1 (بيانات المالك)
        // ══════════════════════════════════════════════════════════

        [HttpGet]
        public IActionResult Stor_step1() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Stor_step1(
            string firstName, string lastName, string email,
            string phone, string password, string confirmPassword)
        {
            // ── Validation ──
            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName) ||
                string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(phone) ||
                string.IsNullOrWhiteSpace(password))
            {
                TempData["StoreError"] = "Please fill in all required fields.";
                TempData["firstName"] = firstName;
                TempData["lastName"] = lastName;
                TempData["email"] = email;
                TempData["phone"] = phone;
                return RedirectToAction("Stor_step1");
            }

            if (password.Length < 8)
            {
                TempData["StoreError"] = "Password must be at least 8 characters.";
                TempData["firstName"] = firstName;
                TempData["lastName"] = lastName;
                TempData["email"] = email;
                TempData["phone"] = phone;
                return RedirectToAction("Stor_step1");
            }

            if (password != confirmPassword)
            {
                TempData["StoreError"] = "Passwords do not match.";
                TempData["firstName"] = firstName;
                TempData["lastName"] = lastName;
                TempData["email"] = email;
                TempData["phone"] = phone;
                return RedirectToAction("Stor_step1");
            }

            if (await _userRepository.GetByEmailAsync(email) != null)
            {
                TempData["StoreError"] = "This email is already registered.";
                TempData["email"] = email;
                return RedirectToAction("Stor_step1");
            }

            // ── إنشاء المستخدم ──
            var user = new User
            {
                FullName = $"{firstName.Trim()} {lastName.Trim()}",
                Email = email,
                PasswordHash = password,
                Phone = phone,
                AccountStatus = "Pending"
            };

            await _userRepository.AddStoreAsync(user);
            var created = await _userRepository.GetByEmailAsync(email);
            HttpContext.Session.SetInt32("NewStoreOwnerId", created!.UserId);

            return RedirectToAction("Stor_step2");
        }

        // ══════════════════════════════════════════════════════════
        // STORE SIGNUP — Step 2 (بيانات المتجر + الوثائق)
        // ══════════════════════════════════════════════════════════

        [HttpGet]
        public IActionResult Stor_step2()
        {
            if (HttpContext.Session.GetInt32("NewStoreOwnerId") == null)
                return RedirectToAction("Stor_step1");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Stor_step2(
            string name, string? description, string email, string phone,
            string? whatsApp, string city,
            IFormFile? commercialRegister, IFormFile? businessLicense,
            IFormFile? trademarkCertificate, IFormFile? additionalFile)
        {
            var ownerId = HttpContext.Session.GetInt32("NewStoreOwnerId");
            if (ownerId == null)
                return RedirectToAction("Stor_step1");

            // ── Validation ──
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(phone) || string.IsNullOrWhiteSpace(city))
            {
                TempData["StoreStep2Error"] = "Please fill in all required fields.";
                return RedirectToAction("Stor_step2");
            }

            if (commercialRegister == null || commercialRegister.Length == 0 ||
                businessLicense == null || businessLicense.Length == 0)
            {
                TempData["StoreStep2Error"] = "Please upload the mandatory documents (Commercial Register & Business License).";
                return RedirectToAction("Stor_step2");
            }

            // ── إنشاء المتجر ──
            var store = new Store
            {
                OwnerUserId = ownerId.Value,
                Name = name,
                Description = description,
                Email = email,
                Phone = phone,
                WhatsApp = whatsApp,
                Country = "Jordan",
                City = city,
                VerificationStatus = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            int newStoreId = await _storeRepository.AddAsync(store);

            // ── حفظ الوثائق ──
            async Task SaveDoc(IFormFile? file, string docType)
            {
                if (file == null || file.Length == 0) return;
                var url = await SaveFileAsync(file, "store-docs");
                await _documentRepository.AddAsync(new StoreDocument
                {
                    IdStore = newStoreId,
                    DocumentType = docType,
                    FileUrl = url,
                    Status = "Pending",
                    UploadedAt = DateTime.UtcNow
                });
            }

            await SaveDoc(commercialRegister, "CommercialRegister");
            await SaveDoc(businessLicense, "BusinessLicense");
            await SaveDoc(trademarkCertificate, "TrademarkCertificate");
            await SaveDoc(additionalFile, "AdditionalFile");

            HttpContext.Session.Remove("NewStoreOwnerId");
            return RedirectToAction("Stor_success");
        }

        [HttpGet]
        public IActionResult Stor_success() => View();

        // ══════════════════════════════════════════════════════════
        // CLINIC SIGNUP — Step 1 (بيانات العيادة + الوثائق)
        // ══════════════════════════════════════════════════════════

        [HttpGet]
        public IActionResult Clinic_step1() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Clinic_step1(
            string name, string city, string phone, string? description,
            IFormFile? docMunicipal, IFormFile? docVet,
            IFormFile? docId, IFormFile? docLogo)
        {
            // ── Validation ──
            if (string.IsNullOrWhiteSpace(name) ||
                string.IsNullOrWhiteSpace(city) ||
                string.IsNullOrWhiteSpace(phone))
            {
                TempData["ClinicStep1Error"] = "Please fill in all required fields.";
                TempData["cc_name"] = name;
                TempData["cc_city"] = city;
                TempData["cc_phone"] = phone;
                TempData["cc_desc"] = description;
                return RedirectToAction("Clinic_step1");
            }

            if (docMunicipal == null || docMunicipal.Length == 0 ||
                docVet == null || docVet.Length == 0 ||
                docId == null || docId.Length == 0)
            {
                TempData["ClinicStep1Error"] = "Please upload all required documents.";
                TempData["cc_name"] = name;
                TempData["cc_city"] = city;
                TempData["cc_phone"] = phone;
                TempData["cc_desc"] = description;
                return RedirectToAction("Clinic_step1");
            }

            // ── رفع الوثائق وحفظ المسارات في Session ──
            var municipalUrl = await SaveFileAsync(docMunicipal, "clinic-docs");
            var vetUrl = await SaveFileAsync(docVet, "clinic-docs");
            var idUrl = await SaveFileAsync(docId, "clinic-docs");
            var logoUrl = docLogo != null && docLogo.Length > 0
                                ? await SaveFileAsync(docLogo, "clinic-logos")
                                : null;

            HttpContext.Session.SetString("Clinic_Name", name);
            HttpContext.Session.SetString("Clinic_City", city);
            HttpContext.Session.SetString("Clinic_Phone", phone);
            HttpContext.Session.SetString("Clinic_Description", description ?? "");
            HttpContext.Session.SetString("Clinic_DocMunicipal", municipalUrl);
            HttpContext.Session.SetString("Clinic_DocVet", vetUrl);
            HttpContext.Session.SetString("Clinic_DocId", idUrl);
            HttpContext.Session.SetString("Clinic_LogoUrl", logoUrl ?? "");

            return RedirectToAction("Clinic_step2");
        }

        // ══════════════════════════════════════════════════════════
        // CLINIC SIGNUP — Step 2 (بيانات المالك)
        // ══════════════════════════════════════════════════════════

        [HttpGet]
        public IActionResult Clinic_step2()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("Clinic_Name")))
                return RedirectToAction("Clinic_step1");

            ViewBag.ReviewName = HttpContext.Session.GetString("Clinic_Name");
            ViewBag.ReviewCity = HttpContext.Session.GetString("Clinic_City");
            ViewBag.ReviewPhone = HttpContext.Session.GetString("Clinic_Phone");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Clinic_step2(
            string fullName, string ownerPhone, string email,
            string password, string confirmPassword)
        {
            // ── استرجاع بيانات Step 1 من Session ──
            var clinicName = HttpContext.Session.GetString("Clinic_Name");
            var clinicCity = HttpContext.Session.GetString("Clinic_City");
            var clinicPhone = HttpContext.Session.GetString("Clinic_Phone");
            var clinicDesc = HttpContext.Session.GetString("Clinic_Description");
            var docMunicipal = HttpContext.Session.GetString("Clinic_DocMunicipal");
            var docVet = HttpContext.Session.GetString("Clinic_DocVet");
            var docId = HttpContext.Session.GetString("Clinic_DocId");
            var logoUrl = HttpContext.Session.GetString("Clinic_LogoUrl");

            if (string.IsNullOrEmpty(clinicName))
                return RedirectToAction("Clinic_step1");

            void SetReviewBag()
            {
                ViewBag.ReviewName = clinicName;
                ViewBag.ReviewCity = clinicCity;
                ViewBag.ReviewPhone = clinicPhone;
            }

            // ── Validation ──
            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(ownerPhone) ||
                string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(confirmPassword))
            {
                TempData["ClinicStep2Error"] = "Please fill in all required fields.";
                SetReviewBag();
                return View();
            }

            if (password.Length < 8)
            {
                TempData["ClinicStep2Error"] = "Password must be at least 8 characters.";
                SetReviewBag();
                return View();
            }

            if (password != confirmPassword)
            {
                TempData["ClinicStep2Error"] = "Passwords do not match.";
                SetReviewBag();
                return View();
            }

            if (await _userRepository.GetByEmailAsync(email) != null)
            {
                TempData["ClinicStep2Error"] = "This email is already registered.";
                SetReviewBag();
                return View();
            }

            // ── 1. إنشاء User بـ Role = "Clinic" ──
            var user = new User
            {
                FullName = fullName,
                Email = email,
                PasswordHash = password,
                Phone = ownerPhone,
                AccountStatus = "Pending"
            };

            await _userRepository.AddClinicAsync(user);
            var createdUser = await _userRepository.GetByEmailAsync(email);

            // ── 2. إنشاء Clinic ──
            var clinic = new Clinic
            {
                OwnerUserId = createdUser!.UserId,
                Name = clinicName!,
                ClinicType = "Veterinary",
                Phone = clinicPhone!,
                Email = email,
                Description = clinicDesc,
                City = clinicCity!,
                Area = "",
                Address = "",
                HasEmergency = false,
                VerificationStatus = "Pending",
                Rating = 0.00m,
                LogoUrl = string.IsNullOrEmpty(logoUrl) ? null : logoUrl,
                CreatedAt = DateTime.UtcNow
            };

            int newClinicId = await _clinicRepository.AddAsync(clinic);

            // ── 3. حفظ الوثائق ──
            async Task SaveClinicDoc(string? fileUrl, string docType)
            {
                if (string.IsNullOrEmpty(fileUrl)) return;
                await _clinicDocumentRepository.AddAsync(new ClinicDocument
                {
                    IdClinic = newClinicId,
                    DocumentType = docType,
                    FileUrl = fileUrl,
                    Status = "Pending",
                    UploadedAt = DateTime.UtcNow
                });
            }

            await SaveClinicDoc(docMunicipal, "MunicipalLicense");
            await SaveClinicDoc(docVet, "VeterinaryLicense");
            await SaveClinicDoc(docId, "NationalId");
            await SaveClinicDoc(logoUrl, "Logo");

            // ── 4. مسح Session ──
            foreach (var key in new[]
            {
                "Clinic_Name", "Clinic_City", "Clinic_Phone", "Clinic_Description",
                "Clinic_DocMunicipal", "Clinic_DocVet", "Clinic_DocId", "Clinic_LogoUrl"
            })
            {
                HttpContext.Session.Remove(key);
            }

            return RedirectToAction("Clinic_success");
        }

        [HttpGet]
        public IActionResult Clinic_success() => View();

        // ══════════════════════════════════════════════════════════
        // FORGOT PASSWORD / OTP / RESET
        // ══════════════════════════════════════════════════════════

        [HttpGet] public IActionResult ForgotPass() => View();
        [HttpGet] public IActionResult OTP() => View();
        [HttpGet] public IActionResult ResetPass() => View();

        // ══════════════════════════════════════════════════════════
        // PROFILE
        // ══════════════════════════════════════════════════════════

        public async Task<IActionResult> Profile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login");

            var user = await _userRepository.GetByIdAsync(userId.Value);
            if (user == null) return NotFound();

            return View(user);
        }

        // ══════════════════════════════════════════════════════════
        // LOGOUT
        // ══════════════════════════════════════════════════════════

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}