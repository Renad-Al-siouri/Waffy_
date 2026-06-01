using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;
using Waffy.Interfaces;
using Waffy.Models;
using Waffy.Models.ViewModels;
using Waffy.Repositories;

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
        private readonly ILocationRepository _locationRepo;
        private readonly IDoctorRepository _doctorRepository;

        public AccountController(
            IUserRepository userRepository,
            IStoreRepository storeRepository,
            IStoreDocumentRepository documentRepository,
            IClinicRepository clinicRepository,
            IClinicDocumentRepository clinicDocumentRepository,
            IPetRepository petRepository,
                IDoctorRepository doctorRepository,
            ILocationRepository locationRepo)
        {
            _userRepository = userRepository;
            _storeRepository = storeRepository;
            _documentRepository = documentRepository;
            _clinicRepository = clinicRepository;
            _clinicDocumentRepository = clinicDocumentRepository;
            _petRepository = petRepository;
            _locationRepo = locationRepo;
            _doctorRepository = doctorRepository;
        }

        // ══════════════════════════════════════════════════════════
        // HELPERS
        // ══════════════════════════════════════════════════════════

        private async Task<string> SaveFileAsync(IFormFile file, string subFolder)
        {
            var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", subFolder);
            Directory.CreateDirectory(folder);
            var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            await using var stream = new FileStream(Path.Combine(folder, fileName), FileMode.Create);
            await file.CopyToAsync(stream);
            return $"/uploads/{subFolder}/{fileName}";
        }

        // ✅ حل مشكلة RoleName — يجلب من Roles عبر RoleId
        private void SetSession(LoginUserResult user, string roleName)
        {
            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("FullName", user.FullName ?? "");
            HttpContext.Session.SetString("RoleName", roleName);
        }

        // ضعه في AccountController كـ private method
        private bool IsValidJordanianPhone(string phone)
        {
            // يقبل: 07XXXXXXXX أو 7XXXXXXXX أو +9627XXXXXXXX أو 9627XXXXXXXX
            var cleaned = phone.Trim()
                .Replace(" ", "")
                .Replace("-", "");

            // إزالة البادئات الدولية
            if (cleaned.StartsWith("+962"))
                cleaned = "0" + cleaned.Substring(4);
            else if (cleaned.StartsWith("962"))
                cleaned = "0" + cleaned.Substring(3);
            else if (cleaned.StartsWith("7") && cleaned.Length == 9)
                cleaned = "0" + cleaned;

            // يجب أن يكون 10 أرقام يبدأ بـ 077, 078, 079
            return cleaned.Length == 10 &&
                   System.Text.RegularExpressions.Regex.IsMatch(
                       cleaned, @"^07[789]\d{7}$");
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

            // ✅ loginResult من نوع LoginUserResult وليس User
            var loginResult = await _userRepository.LoginAsync(email, password);
            if (loginResult == null)
            {
                TempData["LoginError"] = "Invalid email or password.";
                return RedirectToAction("Login");
            }

            var roleName = loginResult.RoleName
                           ?? await _userRepository.GetRoleNameByIdAsync(loginResult.RoleId);

            // ── حساب متجر ──────────────────────────────────────────
            if (roleName == "Store")
            {
                // ✅ loginResult.UserId موجود الآن
                var store = await _storeRepository.GetByOwnerUserIdAsync(loginResult.UserId);

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

                // ✅ SetSession يستقبل LoginUserResult
                SetSession(loginResult, roleName);
                TempData["LoginSuccess"] = "Welcome back!";
                return RedirectToAction("Dashboard", "StoreAdmin");
            }

            // ── حساب عيادة ─────────────────────────────────────────
            if (roleName == "Clinic")
            {
                var clinic = await _clinicRepository.GetByOwnerUserIdAsync(loginResult.UserId);

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

                // ✅ Session الأساسية
                SetSession(loginResult, roleName);

                // ✅ Session إضافية للـ ClinicAdmin
                HttpContext.Session.SetInt32("RoleId", loginResult.RoleId);
                HttpContext.Session.SetInt32("ClinicId", clinic.IdClinic);
                HttpContext.Session.SetString("ClinicName", clinic.Name);
                HttpContext.Session.SetString("UserName", loginResult.FullName ?? "");

                TempData["LoginSuccess"] = "Welcome back!";
                return RedirectToAction("Dashboard", "ClinicAdmin");
            }

            // ── حساب طبيب ──────────────────────────────────────────
            if (roleName == "Doctor")
            {
                var clinicId = await _clinicRepository.GetClinicIdByDoctorUserIdAsync(loginResult.UserId);

                if (clinicId == null)
                {
                    TempData["LoginError"] = "No clinic associated with this doctor account.";
                    return RedirectToAction("Login");
                }

                var clinic = await _clinicRepository.GetByIdAsync(clinicId.Value);

                // ✅ جلب DoctorId من Doctor table
                var doctorId = await _doctorRepository.GetDoctorIdByUserIdAsync(loginResult.UserId);

                SetSession(loginResult, roleName);
                HttpContext.Session.SetInt32("RoleId", loginResult.RoleId);
                HttpContext.Session.SetInt32("ClinicId", clinicId.Value);
                HttpContext.Session.SetInt32("DoctorId", doctorId ?? 0);     // ← جديد
                HttpContext.Session.SetString("ClinicName", clinic?.Name ?? "");
                HttpContext.Session.SetString("UserName", loginResult.FullName ?? "");

                TempData["LoginSuccess"] = "Welcome back, Doctor!";
                return RedirectToAction("Dashboard", "DoctorAdmin"); // ← تغيير هنا
            }

            // ── Admin / User ────────────────────────────────────────
            if (loginResult.AccountStatus != "Active")
            {
                TempData["LoginError"] = "Your account is not active. Please contact support.";
                return RedirectToAction("Login");
            }

            SetSession(loginResult, roleName);
            TempData["LoginSuccess"] = "Logged in successfully.";

            return roleName switch
            {
                "Admin" => RedirectToAction("Users", "SuperAdmin"),
                "User" => RedirectToAction("Home_After", "Home"),
                _ => RedirectToAction("Home_After", "Home")
            };
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

        // ══════════════════════════════════════════════════════════
        // PROFILE
        // ══════════════════════════════════════════════════════════

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

        // ══════════════════════════════════════════════════════════
        // SIGNUP — صفحة الاختيار
        // ══════════════════════════════════════════════════════════

        [HttpGet]
        public IActionResult Signup() => View();

        // ══════════════════════════════════════════════════════════
        // USER SIGNUP
        // ══════════════════════════════════════════════════════════

        [HttpGet]
        public async Task<IActionResult> user_signup()
        {
            await LoadLocationsToViewBag();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> user_signup(
            string fullName, string email, string password,
            string confirmPassword, string phone, 
            string city, string? area)
        {
            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(confirmPassword) ||
                string.IsNullOrWhiteSpace(phone) || string.IsNullOrWhiteSpace(city))
            {
                TempData["SignupError"] = "Please fill in all required fields.";
                return RedirectToAction("user_signup");
            }

            if (!IsValidJordanianPhone(phone))
            {
                TempData["SignupError"] = "Please enter a valid Jordanian phone number (07X XXX XXXX).";
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
                Area = area,
                IsPhoneVerified = false,
                IsEmailVerified = false,
                AccountStatus = "Active"
            };


            await _userRepository.AddNormalUserAsync(user);
            TempData["SignupSuccess"] = "Account created successfully. Please log in.";
            return RedirectToAction("Login");
        }

        // ══════════════════════════════════════════════════════════
        // STORE SIGNUP — Step 1
        // ══════════════════════════════════════════════════════════

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

            if (!IsValidJordanianPhone(phone))
            {
                TempData["StoreError"] = "Please enter a valid Jordanian phone number (07X XXX XXXX).";
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
        // STORE SIGNUP — Step 2
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
            if (ownerId == null) return RedirectToAction("Stor_step1");

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
        // CLINIC SIGNUP — Step 1
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

            if (!IsValidJordanianPhone(phone))
            {
                TempData["ClinicStep1Error"] = "Please enter a valid Jordanian phone number (07X XXX XXXX).";
                TempData["cc_name"] = name;
                TempData["cc_city"] = city;
                TempData["cc_phone"] = phone;
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

            var municipalUrl = await SaveFileAsync(docMunicipal, "clinic-docs");
            var vetUrl = await SaveFileAsync(docVet, "clinic-docs");
            var idUrl = await SaveFileAsync(docId, "clinic-docs");
            var logoUrl = (docLogo != null && docLogo.Length > 0)
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
        // CLINIC SIGNUP — Step 2
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
            string fullName, string ownerPhone,
            string email, string password, string confirmPassword)
        {
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

            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(ownerPhone) ||
                string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(confirmPassword))
            {
                TempData["ClinicStep2Error"] = "Please fill in all required fields.";
                SetReviewBag(); return View();
            }

            if (!IsValidJordanianPhone(ownerPhone))
            {
                TempData["ClinicStep2Error"] = "Please enter a valid Jordanian phone number (07X XXX XXXX).";
                SetReviewBag();
                return View();
            }

            if (password.Length < 8)
            {
                TempData["ClinicStep2Error"] = "Password must be at least 8 characters.";
                SetReviewBag(); return View();
            }

            if (password != confirmPassword)
            {
                TempData["ClinicStep2Error"] = "Passwords do not match.";
                SetReviewBag(); return View();
            }

            if (await _userRepository.GetByEmailAsync(email) != null)
            {
                TempData["ClinicStep2Error"] = "This email is already registered.";
                SetReviewBag(); return View();
            }

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

            foreach (var key in new[]
            {
                "Clinic_Name","Clinic_City","Clinic_Phone","Clinic_Description",
                "Clinic_DocMunicipal","Clinic_DocVet","Clinic_DocId","Clinic_LogoUrl"
            })
            { HttpContext.Session.Remove(key); }

            return RedirectToAction("Clinic_success");
        }

        [HttpGet]
        public IActionResult Clinic_success() => View();



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

        // ══════════════════════════════════════════════════════════
        // FORGOT PASSWORD / OTP / RESET
        // ══════════════════════════════════════════════════════════

        [HttpGet] public IActionResult ForgotPass() => View();
        [HttpGet] public IActionResult OTP() => View();
        [HttpGet] public IActionResult ResetPass() => View();
    }
}