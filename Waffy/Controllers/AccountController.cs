using Microsoft.AspNetCore.Mvc;
using Waffy.Interfaces;
using Waffy.Models;
using Waffy.Models.ViewModels;
using Waffy.Repository; 

namespace Waffy.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly IStoreRepository _storeRepository;
        private readonly IStoreDocumentRepository _documentRepository;
        private readonly IClinicRepository _clinicRepository;
        private readonly IClinicDocumentRepository _clinicDocumentRepository;
        private readonly IPetRepository _petRepository;
        public AccountController(
            IUserRepository userRepository,
            IStoreRepository storeRepository,
            IStoreDocumentRepository documentRepository,
            IClinicRepository clinicRepository,
            IClinicDocumentRepository clinicDocumentRepository,
            IPetRepository petRepository)    
        {
            _userRepository = userRepository;
            _storeRepository = storeRepository;
            _documentRepository = documentRepository;
            _clinicRepository = clinicRepository;
            _clinicDocumentRepository = clinicDocumentRepository;
            _petRepository = petRepository;
        }

        // =========================
        // Login
        // =========================
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

            // ── حساب متجر — فحص خاص ──────────────────────────
            if (user.RoleName == "Store")
            {
                var store = await _storeRepository.GetByOwnerUserIdAsync(user.UserId);

                if (store == null)
                {
                    TempData["LoginError"] = "No store found.";
                    return RedirectToAction("Login");
                }

                if (store.VerificationStatus == "Pending")
                {
                    TempData["LoginError"] = "Your store is under review. Please wait for approval.";
                    return RedirectToAction("Login");
                }

                if (store.VerificationStatus == "Rejected")
                {
                    TempData["LoginError"] = "Your store was rejected. Contact support.";
                    return RedirectToAction("Login");
                }

                if (store.VerificationStatus == "Suspended")
                {
                    TempData["LoginError"] = "Your store is suspended.";
                    return RedirectToAction("Login");
                }

                // ✅ Approved — يدخل للداشبورد
                HttpContext.Session.SetInt32("UserId", user.UserId);
                HttpContext.Session.SetString("FullName", user.FullName);
                HttpContext.Session.SetString("RoleName", user.RoleName);
                return RedirectToAction("Dashboard", "StoreAdmin");
            }

            // ── باقي الأدوار ──────────────────────────────────
            if (user.AccountStatus != "Active")
            {
                TempData["LoginError"] = "Your account is not active.";
                return RedirectToAction("Login");
            }

            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("FullName", user.FullName);
            HttpContext.Session.SetString("RoleName", user.RoleName);

            return user.RoleName switch
            {
                "Admin" => RedirectToAction("Users", "SuperAdmin"),
                "User" => RedirectToAction("Home_After", "Home"),
                "Clinic" => RedirectToAction("Dashboard", "ClinicAdmin"),
                _ => RedirectToAction("Home_After", "Home")
            };
        }

        // =========================
        // Signup
        // =========================
        [HttpGet]
        public IActionResult Signup() => View();

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
        [HttpGet] public IActionResult ForgotPass() => View();
        [HttpGet] public IActionResult OTP() => View();
        [HttpGet] public IActionResult ResetPass() => View();

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
            if (userId == null) return RedirectToAction("Login");

            var user = await _userRepository.GetByIdAsync(userId.Value);
            if (user == null) return NotFound();

            var pets = await _petRepository.GetByUserIdAsync(userId.Value);

            var vm = new PetProfileViewModel
            {
                User = user,
                Pets = pets.ToList()
            };

            return View(vm);
        }



        // =========================
        // Store Signup — Step 1
        // =========================
        [HttpGet]
        public IActionResult Stor_step1() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Stor_step1(
            string firstName, string lastName, string email,
            string phone, string password, string confirmPassword)
        {
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

            if (password != confirmPassword || password.Length < 8)
            {
                TempData["StoreError"] = password.Length < 8
                    ? "Password must be at least 8 characters."
                    : "Passwords do not match.";
                TempData["firstName"] = firstName;
                TempData["lastName"] = lastName;
                TempData["email"] = email;
                TempData["phone"] = phone;
                return RedirectToAction("Stor_step1");
            }

            var existing = await _userRepository.GetByEmailAsync(email);
            if (existing != null)
            {
                TempData["StoreError"] = "This email is already registered.";
                TempData["email"] = email;
                return RedirectToAction("Stor_step1");
            }

            var user = new User
            {
                FullName = $"{firstName} {lastName}",
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

        // =========================
        // Store Signup — Step 2
        // =========================
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

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(phone) || string.IsNullOrWhiteSpace(city))
            {
                TempData["StoreStep2Error"] = "Please fill in all required fields.";
                return RedirectToAction("Stor_step2");
            }

            if (commercialRegister == null || commercialRegister.Length == 0 ||
                businessLicense == null || businessLicense.Length == 0)
            {
                TempData["StoreStep2Error"] = "Please upload the mandatory documents.";
                return RedirectToAction("Stor_step2");
            }

            var store = new Store
            {
                OwnerUserId = ownerId!.Value,
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

            async Task SaveStoreDoc(IFormFile? file, string docType)
            {
                if (file == null || file.Length == 0) return;
                var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "store-docs");
                Directory.CreateDirectory(folder);
                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
                using var stream = new FileStream(Path.Combine(folder, fileName), FileMode.Create);
                await file.CopyToAsync(stream);
                await _documentRepository.AddAsync(new StoreDocument
                {
                    IdStore = newStoreId,
                    DocumentType = docType,
                    FileUrl = $"/uploads/store-docs/{fileName}",
                    Status = "Pending",
                    UploadedAt = DateTime.UtcNow
                });
            }

            await SaveStoreDoc(commercialRegister, "CommercialRegister");
            await SaveStoreDoc(businessLicense, "BusinessLicense");
            await SaveStoreDoc(trademarkCertificate, "TrademarkCertificate");
            await SaveStoreDoc(additionalFile, "AdditionalFile");

            HttpContext.Session.Remove("NewStoreOwnerId");
            return RedirectToAction("Stor_success");
        }

        [HttpGet]
        public IActionResult Stor_success() => View();

        // =========================
        // Clinic Signup — Step 1
        // حفظ بيانات العيادة + الوثائق في Session والانتقال لـ Step 2
        // =========================
        [HttpGet]
        public IActionResult Clinic_step1() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Clinic_step1(
            string name,
            string city,
            string phone,
            string? description,
            IFormFile? docMunicipal,
            IFormFile? docVet,
            IFormFile? docId,
            IFormFile? docLogo)
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

            // ── رفع الوثائق وحفظ المسارات مؤقتاً في Session ──
            async Task<string> SaveClinicDoc(IFormFile file, string subFolder)
            {
                var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", subFolder);
                Directory.CreateDirectory(folder);
                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
                using var stream = new FileStream(Path.Combine(folder, fileName), FileMode.Create);
                await file.CopyToAsync(stream);
                return $"/uploads/{subFolder}/{fileName}";
            }

            var municipalUrl = await SaveClinicDoc(docMunicipal, "clinic-docs");
            var vetUrl = await SaveClinicDoc(docVet, "clinic-docs");
            var idUrl = await SaveClinicDoc(docId, "clinic-docs");
            var logoUrl = docLogo != null && docLogo.Length > 0
                                   ? await SaveClinicDoc(docLogo, "clinic-logos")
                                   : null;

            // ── حفظ في Session ──
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

        // =========================
        // Clinic Signup — Step 2
        // إنشاء User + Clinic + ClinicDocuments
        // =========================
        [HttpGet]
        public IActionResult Clinic_step2()
        {
            // إذا لم تكمل Step 1 أعده
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("Clinic_Name")))
                return RedirectToAction("Clinic_step1");

            // تمرير بيانات Review للـ View
            ViewBag.ReviewName = HttpContext.Session.GetString("Clinic_Name");
            ViewBag.ReviewCity = HttpContext.Session.GetString("Clinic_City");
            ViewBag.ReviewPhone = HttpContext.Session.GetString("Clinic_Phone");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Clinic_step2(
            string fullName,
            string ownerPhone,
            string email,
            string password,
            string confirmPassword)
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

            // ── Validation ──
            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(ownerPhone) ||
                string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(confirmPassword))
            {
                TempData["ClinicStep2Error"] = "Please fill in all required fields.";
                ViewBag.ReviewName = clinicName;
                ViewBag.ReviewCity = clinicCity;
                ViewBag.ReviewPhone = clinicPhone;
                return View();
            }

            if (password != confirmPassword)
            {
                TempData["ClinicStep2Error"] = "Passwords do not match.";
                ViewBag.ReviewName = clinicName;
                ViewBag.ReviewCity = clinicCity;
                ViewBag.ReviewPhone = clinicPhone;
                return View();
            }

            if (password.Length < 8)
            {
                TempData["ClinicStep2Error"] = "Password must be at least 8 characters.";
                ViewBag.ReviewName = clinicName;
                ViewBag.ReviewCity = clinicCity;
                ViewBag.ReviewPhone = clinicPhone;
                return View();
            }

            var existing = await _userRepository.GetByEmailAsync(email);
            if (existing != null)
            {
                TempData["ClinicStep2Error"] = "This email is already registered.";
                ViewBag.ReviewName = clinicName;
                ViewBag.ReviewCity = clinicCity;
                ViewBag.ReviewPhone = clinicPhone;
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

            // ── 2. إنشاء Clinic وربطها بالـ User ──
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
                Rating = 0.00m,        // ✅ هذا هو الإصلاح
                LogoUrl = string.IsNullOrEmpty(logoUrl) ? null : logoUrl,
                CreatedAt = DateTime.UtcNow
            };

            int newClinicId = await _clinicRepository.AddAsync(clinic);

            // ── 3. حفظ الوثائق في ClinicDocument ──
            async Task SaveClinicDocument(string? fileUrl, string docType)
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

            await SaveClinicDocument(docMunicipal, "MunicipalLicense");
            await SaveClinicDocument(docVet, "VeterinaryLicense");
            await SaveClinicDocument(docId, "NationalId");
            await SaveClinicDocument(logoUrl, "Logo");

            // ── 4. مسح Session ──
            HttpContext.Session.Remove("Clinic_Name");
            HttpContext.Session.Remove("Clinic_City");
            HttpContext.Session.Remove("Clinic_Phone");
            HttpContext.Session.Remove("Clinic_Description");
            HttpContext.Session.Remove("Clinic_DocMunicipal");
            HttpContext.Session.Remove("Clinic_DocVet");
            HttpContext.Session.Remove("Clinic_DocId");
            HttpContext.Session.Remove("Clinic_LogoUrl");

            return RedirectToAction("Clinic_success");
        }

        // =========================
        // Clinic Signup — Success
        // =========================
        [HttpGet]
        public IActionResult Clinic_success() => View();
    }
}