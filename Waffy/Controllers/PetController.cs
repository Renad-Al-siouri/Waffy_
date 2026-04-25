
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
using Waffy.Interfaces;
using Waffy.Models;
using Waffy.Models.Enums;
using Waffy.Models.ViewModels;

namespace Waffy.Controllers
{
    public class PetController : Controller
    {
        private readonly IPetRepository _petRepo;
        private readonly IUserRepository _userRepo;
        private readonly IAdoptionRepository _adoptionRepo;
        private readonly ILostPetRepository _lostPetRepo;

        public PetController(
            IPetRepository petRepo,
            IUserRepository userRepo,
            IAdoptionRepository adoptionRepo,
            ILostPetRepository lostPetRepo)
        {
            _petRepo = petRepo;
            _userRepo = userRepo;
            _adoptionRepo = adoptionRepo;
            _lostPetRepo = lostPetRepo;
        }

        // ── Helper ───────────────────────────────────────────
        private int? GetCurrentUserId() =>
            HttpContext.Session.GetInt32("UserId");

        private async Task<User?> GetCurrentUserAsync()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return null;
            return await _userRepo.GetByIdAsync(userId.Value);
        }

        private static string? Trunc(string? s, int max) =>
            s == null ? null : s.Length <= max ? s : s[..max];

        // ════════════════════════════════════════════════════
        // ADOPTION PAGE — Adapt.cshtml
        // ════════════════════════════════════════════════════

        [HttpGet]
        public async Task<IActionResult> Adapt()
        {
            var posts = (await _adoptionRepo.GetApprovedAsync()).ToList();
            var userId = GetCurrentUserId();

            var cards = new List<AdoptionCardDto>();
            foreach (var p in posts)
            {
                var imageUrl = await _adoptionRepo.GetFirstImageAsync(p.IdPost);
                var card = AdoptionCardDto.FromPost(p, imageUrl ?? "");

                // ✅ تحقق من DB مش Session
                if (userId.HasValue)
                    card.IsLikedByCurrentUser = await _adoptionRepo.HasLikedAsync(p.IdPost, userId.Value);

                cards.Add(card);
            }

            return View(new AdoptionPageViewModel { Posts = cards });
        }


        // ════════════════════════════════════════════════════
        // LOST PETS PAGE — Lost.cshtml
        // ════════════════════════════════════════════════════



        [HttpGet]
        public async Task<IActionResult> Lost(int page = 1)
        {
            const int pageSize = 6; // ✅ عدد الكروت في كل صفحة

            var totalCount = await _lostPetRepo.GetActiveTotalCountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            // ✅ تأكد أن الـ page في النطاق الصحيح
            page = Math.Max(1, Math.Min(page, totalPages == 0 ? 1 : totalPages));

            var reports = (await _lostPetRepo.GetActivePagedAsync(page, pageSize)).ToList();

            Console.WriteLine($"[Lost] Page {page}/{totalPages} — Reports: {reports.Count}");

            var dtos = new List<LostReportDto>();
            foreach (var r in reports)
            {
                var imageUrl = await _lostPetRepo.GetFirstImageAsync(r.IdLostReport);
                dtos.Add(LostReportDto.FromReport(r, imageUrl ?? ""));
            }

            var vm = new LostPageViewModel
            {
                Reports = dtos,
                CurrentPage = page,
                TotalPages = totalPages == 0 ? 1 : totalPages
            };

            return View(vm);
        }

        // ════════════════════════════════════════════════════
        // PET RECORD — Petrecord.cshtml
        // ════════════════════════════════════════════════════

        [HttpGet]
        public async Task<IActionResult> Petrecord(int petId)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            Pet? pet;

            if (petId > 0)
            {
                pet = await _petRepo.GetByIdAsync(petId);
                if (pet == null || pet.IdUsers != userId.Value)
                    return RedirectToAction("Profile", "Account");
            }
            else
            {
                var userPets = (await _petRepo.GetByUserIdAsync(userId.Value)).ToList();
                if (!userPets.Any())
                    return RedirectToAction("Profile", "Account");
                pet = userPets.First();
            }

            return View(pet);
        }




        // ════════════════════════════════════════════════════
        // ADD PET PAGE — GET
        // ════════════════════════════════════════════════════
        [HttpGet]
        public IActionResult AddPetPage()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Account");
            return View();
        }

        // ════════════════════════════════════════════════════
        // ADD PET — POST
        // ════════════════════════════════════════════════════
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(52428800)]
        [RequestFormLimits(MultipartBodyLengthLimit = 52428800)]
        public async Task<IActionResult> AddPet(
         string name,
         string species,
         string? breed,
         string? gender,
         string? birthDate,
         string? color,
         string? energyLevel,
         string? vaccinationStatus,
         bool goodWithChildren,
         bool goodWithPets,
         bool isNeutered,
         bool isTrained,
         string? medicalNotes,
         string? traits,
         string? microchipId,
         decimal? weight,
         IFormFile? petImage)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            try
            {
                var errors = ValidatePetInput(name, species);
                if (errors.Any())
                {
                    TempData["PetError"] = errors.First();
                    return RedirectToAction("Profile", "Account");
                }

                string? imageUrl = await UploadSingleFileAsync(petImage, "pets");

                var pet = new Pet
                {
                    IdUsers = userId.Value,
                    Name = name,
                    Species = species,
                    Breed = breed,
                    Gender = gender,
                    BirthDate = DateTime.TryParse(birthDate, out var bd) ? bd : null,
                    Color = color,
                    EnergyLevel = energyLevel,
                    VaccinationStatus = vaccinationStatus,
                    GoodWithChildren = goodWithChildren,
                    GoodWithPets = goodWithPets,
                    IsNeutered = isNeutered,
                    IsTrained = isTrained,
                    MedicalNotes = medicalNotes,
                    Traits = traits,
                    MicrochipId = microchipId,
                    Weight = weight,
                    ImageUrl = imageUrl,
                    CreatedAt = DateTime.UtcNow
                };

                await _petRepo.AddAsync(pet);

                TempData["PetSuccess"] = $"{name} added successfully!";
                return RedirectToAction("Profile", "Account");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AddPet] ❌ CRASH: {ex.GetType().Name}: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                TempData["PetError"] = "Upload failed. Please try again.";
                return RedirectToAction("Profile", "Account");
            }
        }

        // ════════════════════════════════════════════════════
        // EDIT PET PAGE — GET
        // ════════════════════════════════════════════════════
        [HttpGet]
        public async Task<IActionResult> EditPetPage(int petId)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            var pet = await _petRepo.GetByIdAsync(petId);
            if (pet == null || pet.IdUsers != userId.Value)
                return RedirectToAction("Profile", "Account");

            return View(pet);
        }

        // ════════════════════════════════════════════════════
        // EDIT PET — POST
        // ════════════════════════════════════════════════════
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(10_485_760)]
        [RequestFormLimits(MultipartBodyLengthLimit = 10_485_760)]
        public async Task<IActionResult> EditPet(
           int petId, string name, string species, string? breed, string? gender,
           string? birthDate, string? color, string? energyLevel,
           string? vaccinationStatus, bool goodWithChildren, bool goodWithPets,
           bool isNeutered, bool isTrained, string? medicalNotes, string? traits,
           string? microchipId,  
           decimal? weight,
           IFormFile? petImage)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            try
            {
                var pet = await _petRepo.GetByIdAsync(petId);
                if (pet == null || pet.IdUsers != userId.Value)
                {
                    TempData["PetError"] = "Pet not found.";
                    return RedirectToAction("Profile", "Account");
                }

                var errors = ValidatePetInput(name, species);
                if (errors.Any())
                {
                    TempData["PetError"] = errors.First();
                    return RedirectToAction("Profile", "Account");
                }

                var newImageUrl = await UploadSingleFileAsync(petImage, "pets");
                if (newImageUrl != null) pet.ImageUrl = newImageUrl;

                pet.Name = name;
                pet.Species = species;
                pet.Breed = breed;
                pet.Gender = gender;
                pet.BirthDate = DateTime.TryParse(birthDate, out var bd) ? bd : null;
                pet.Color = color;
                pet.EnergyLevel = energyLevel;
                pet.VaccinationStatus = vaccinationStatus;
                pet.GoodWithChildren = goodWithChildren;
                pet.GoodWithPets = goodWithPets;
                pet.IsNeutered = isNeutered;
                pet.IsTrained = isTrained;
                pet.MedicalNotes = medicalNotes;
                pet.Traits = traits;
                pet.MicrochipId = microchipId;   
                pet.Weight = weight;

                await _petRepo.UpdateAsync(pet);

                TempData["PetSuccess"] = $"{name} updated successfully!";
                return RedirectToAction("Profile", "Account");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EditPet] Error: {ex.Message}");
                TempData["PetError"] = "Something went wrong. Please try again.";
                return RedirectToAction("Profile", "Account");
            }
        }

        // ════════════════════════════════════════════════════
        // DELETE PET — POST
        // ════════════════════════════════════════════════════
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePet(int petId)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            var pet = await _petRepo.GetByIdAsync(petId);
            if (pet == null || pet.IdUsers != userId.Value)
            {
                TempData["PetError"] = "Pet not found.";
                return RedirectToAction("Profile", "Account");
            }

            await _petRepo.DeleteAsync(petId);

            TempData["PetSuccess"] = $"{pet.Name} deleted successfully.";
            return RedirectToAction("Profile", "Account");
        }

        // ════════════════════════════════════════════════════
        // UPLOAD PET PHOTO — POST
        // ════════════════════════════════════════════════════
      
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(10 * 1024 * 1024)]
        public async Task<IActionResult> UploadPetPhoto(int petId, IFormFile photo)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            var pet = await _petRepo.GetByIdAsync(petId);
            if (pet == null || pet.IdUsers != userId.Value)
                return RedirectToAction("Profile", "Account");

            var imageUrl = await UploadSingleFileAsync(photo, "pets");
            if (imageUrl != null)
                await _petRepo.UpdateImageAsync(petId, imageUrl);

            return RedirectToAction("Profile", "Account");
        }

       

        // ════════════════════════════════════════════════════
        // SUBMIT ADOPTION — POST
        // ════════════════════════════════════════════════════
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitAdoption(
            int petId,
            string city,
            string? area,
            string contactPhone,
            string? contactMethod,
            string description,
            string reason,
            List<IFormFile>? photos)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            var pet = await _petRepo.GetByIdAsync(petId);
            if (pet == null || pet.IdUsers != userId.Value)
            {
                TempData["AdoptError"] = "Pet not found.";
                return RedirectToAction("Petrecord", new { petId });
            }

            var errors = ValidateAdoptionInput(city, contactPhone, description, reason);
            if (errors.Any())
            {
                TempData["AdoptError"] = errors.First();
                return RedirectToAction("Petrecord", new { petId });
            }

            // ✅ تحقق: هل يوجد بوست نشط لنفس الحيوان؟
            var existingPosts = await _adoptionRepo.GetByUserIdAsync(userId.Value);
            var hasActivePost = existingPosts.Any(p =>
                p.IdPet == petId &&
                p.AdoptionStatus != "Closed");

            if (hasActivePost)
            {
                TempData["AdoptError"] = "You already have an active adoption post for this pet.";
                return RedirectToAction("Petrecord", new { petId });
            }

            var user = await _userRepo.GetByIdAsync(userId.Value);

            var post = new AdoptionPost
            {
                PostedByType = "User",
                PostedById = userId.Value,
                IdPet = petId,
                PetName = Trunc(pet.Name, 100),
                AnimalType = Trunc(pet.Species, 50),
                Breed = Trunc(pet.Breed, 100),
                Color = Trunc(pet.Color, 100),
                Gender = Trunc(pet.Gender, 10),
                EnergyLevel = Trunc(pet.EnergyLevel, 20),
                GoodWithChildren = pet.GoodWithChildren,
                GoodWithPets = pet.GoodWithPets,
                HealthStatus = Trunc(pet.MedicalNotes, 200),
                IsVaccinated = pet.VaccinationStatus == "UpToDate",
                IsNeutered = pet.IsNeutered,
                City = Trunc(city, 100),
                Area = Trunc(area, 100),
                ContactName = Trunc(user?.FullName, 100),
                ContactPhone = Trunc(contactPhone, 20),
                Description = Trunc(description, 1000),
                ReasonForAdoption = Trunc(reason, 500),
                Status = PostStatus.Approved.ToString(),
                AdoptionStatus = AdoptionStatus.Available.ToString(),
                PostCode = GenerateCode("ADT"),
               
                BirthDate = pet.BirthDate,   // ← أضف هذا
                Age = pet.BirthDate.HasValue
                ? (int)((DateTime.Today - pet.BirthDate.Value).TotalDays / 30.44)
                : null,
                CreatedAt = DateTime.UtcNow
            };
            // ════════════════════════════════════════════════════
            // ✅ الكود الثاني — يضيف مباشرة بدون موافقة Admin
            // يشتغل الآن
            // ════════════════════════════════════════════════════
            /* var post = new AdoptionPost
             {
                 PostedByType = "User",
                 PostedById = userId.Value,
                 IdPet = petId,
                 PetName = pet.Name,
                 AnimalType = pet.Species,
                 Breed = pet.Breed,
                 Color = pet.Color,
                 Gender = pet.Gender,
                 EnergyLevel = pet.EnergyLevel,
                 GoodWithChildren = pet.GoodWithChildren,
                 GoodWithPets = pet.GoodWithPets,
                 HealthStatus = pet.MedicalNotes,
                 IsVaccinated = pet.VaccinationStatus == "UpToDate",
                 IsNeutered = pet.IsNeutered,
                 City = city,
                 Area = area,
                 ContactName = user?.FullName,
                 ContactPhone = contactPhone,
                 Description = description,
                 ReasonForAdoption = reason,
                 Status = PostStatus.Pending.ToString(),       // ✅ يظهر مباشرة
                 AdoptionStatus = AdoptionStatus.Available.ToString(),
                 PostCode = GenerateCode("ADT"),
                 CreatedAt = DateTime.UtcNow
             };*/
            var newPostId = await _adoptionRepo.AddAsync(post);
            await SavePhotosAsync(photos, "adoption", newPostId,
                (id, url) => _adoptionRepo.AddImageAsync(id, url));

            TempData["AdoptSuccess"] = $"Ad submitted! Code: {post.PostCode}";
            return RedirectToAction("Petrecord", new { petId });
        }

        // ════════════════════════════════════════════════════
        // SUBMIT LOST REPORT — POST
        // ════════════════════════════════════════════════════
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitLostReport(
            int petId,
            string city,
            string? area,
            string? lastSeenLocation,
            string? dateLost,
            string description,
            string? specialMarks,
            string secretQuestion,
            string secretAnswer,
            string contactPhone,
            decimal rewardAmount,
            string expiryDays,
            List<IFormFile>? photos)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            var pet = await _petRepo.GetByIdAsync(petId);
            if (pet == null || pet.IdUsers != userId.Value)
            {
                TempData["LostError"] = "Pet not found.";
                return RedirectToAction("Petrecord", new { petId });
            }

            var errors = ValidateLostReportInput(
                city, description, secretQuestion, secretAnswer, contactPhone);
            if (errors.Any())
            {
                TempData["LostError"] = errors.First();
                return RedirectToAction("Petrecord", new { petId });
            }

            int days = expiryDays switch
            {
                "7 Days" => 7,
                "30 Days" => 30,
                _ => 14
            };

            var report = new LostPetReport
            {
                IdUser = userId.Value,
                IdPet = petId,
                PetName = pet.Name,
                AnimalType = pet.Species,
                Breed = pet.Breed,
                Color = pet.Color,
                LastSeenLocation = lastSeenLocation,
                City = city,
                Area = area,
                DateLost = DateTime.TryParse(dateLost, out var pd)
                                   ? pd : DateTime.Today,
                Description = description,
                SpecialMarks = specialMarks,
                MicrochipId = pet.MicrochipId,
                SecretQuestion = secretQuestion,
                SecretAnswerHash = HashAnswer(secretAnswer),
                RewardAmount = rewardAmount > 0 ? rewardAmount : null,
                RewardStatus = rewardAmount > 0
                                   ? RewardStatus.Reserved.ToString()
                                   : RewardStatus.None.ToString(),
                Status = LostPetStatus.Active.ToString(),
                ExpiryDate = DateTime.UtcNow.AddDays(days),
                ReportCode = GenerateCode("L"),
                CreatedAt = DateTime.UtcNow
            };

            var newReportId = await _lostPetRepo.AddAsync(report);
            await SavePhotosAsync(photos, "lost", newReportId,
                (id, url) => _lostPetRepo.AddImageAsync(id, url));

            TempData["LostSuccess"] = $"Lost ad published! Code: {report.ReportCode}";
            return RedirectToAction("Petrecord", new { petId });
        }

        [HttpPost]
        public async Task<IActionResult> ToggleLike(int postId)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Json(new { error = "not_logged_in" });

            var hasLiked = await _adoptionRepo.HasLikedAsync(postId, userId.Value);

            if (!hasLiked)
                await _adoptionRepo.AddLikeAsync(postId, userId.Value);
            else
                await _adoptionRepo.RemoveLikeAsync(postId, userId.Value);

            var count = await _adoptionRepo.GetLikesAsync(postId);
            return Json(new { count, liked = !hasLiked });
        }


        // ════════════════════════════════════════════════════
        // PRIVATE HELPERS
        // ════════════════════════════════════════════════════

        // ✅ Validation — مكان واحد للكل
        private static List<string> ValidatePetInput(string? name, string? species)
        {
            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(name)) errors.Add("Pet name is required.");
            if (name?.Length > 100) errors.Add("Pet name is too long.");
            if (string.IsNullOrWhiteSpace(species)) errors.Add("Pet type is required.");
            return errors;
        }

        private static List<string> ValidateAdoptionInput(
            string? city, string? phone, string? description, string? reason)
        {
            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(city)) errors.Add("City is required.");
            if (string.IsNullOrWhiteSpace(phone)) errors.Add("Contact phone is required.");
            if (string.IsNullOrWhiteSpace(description)) errors.Add("Description is required.");
            if (string.IsNullOrWhiteSpace(reason)) errors.Add("Reason is required.");
            return errors;
        }

        private static List<string> ValidateLostReportInput(
            string? city, string? description,
            string? secretQ, string? secretA, string? phone)
        {
            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(city)) errors.Add("City is required.");
            if (string.IsNullOrWhiteSpace(description)) errors.Add("Description is required.");
            if (string.IsNullOrWhiteSpace(secretQ)) errors.Add("Secret question is required.");
            if (string.IsNullOrWhiteSpace(secretA)) errors.Add("Secret answer is required.");
            if (string.IsNullOrWhiteSpace(phone)) errors.Add("Contact phone is required.");
            return errors;
        }

        // ✅ File upload — مكان واحد
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
        private const long MaxFileSize = 5 * 1024 * 1024; // 5MB

        private async Task<string?> UploadSingleFileAsync(IFormFile? file, string subfolder)
        {
            if (file == null || file.Length == 0) return null;

            try
            {
                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!AllowedExtensions.Contains(ext))
                {
                    Console.WriteLine($"[Upload] ❌ Extension not allowed: {ext}");
                    return null;
                }

                if (file.Length > MaxFileSize)
                {
                    Console.WriteLine($"[Upload] ❌ File too large: {file.Length} bytes");
                    return null;
                }

                // ✅ تأكد من وجود الـ wwwroot
                var wwwroot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                if (!Directory.Exists(wwwroot))
                {
                    Console.WriteLine($"[Upload] ❌ wwwroot not found: {wwwroot}");
                    return null;
                }

                var folder = Path.Combine(wwwroot, "uploads", subfolder);
                Directory.CreateDirectory(folder);

                var safeFileName = Path.GetFileName(file.FileName);
                var fileName = $"{Guid.NewGuid()}_{safeFileName}";
                var filePath = Path.Combine(folder, fileName);

                Console.WriteLine($"[Upload] Saving to: {filePath}");

                await using var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
                await file.CopyToAsync(stream);

                Console.WriteLine($"[Upload] ✅ Saved: /uploads/{subfolder}/{fileName}");
                return $"/uploads/{subfolder}/{fileName}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Upload] ❌ EXCEPTION: {ex.GetType().Name}");
                Console.WriteLine($"[Upload] ❌ Message: {ex.Message}");
                Console.WriteLine($"[Upload] ❌ Stack: {ex.StackTrace}");
                return null;
            }
        }

        private async Task SavePhotosAsync(
            List<IFormFile>? photos,
            string subfolder,
            int entityId,
            Func<int, string, Task> saveImageFn)
        {
            if (photos == null || photos.Count == 0) return;

            foreach (var photo in photos.Take(5))
            {
                var url = await UploadSingleFileAsync(photo, subfolder);
                if (url != null)
                    await saveImageFn(entityId, url);
            }
        }

        // ✅ Hash الـ SecretAnswer
        private static string HashAnswer(string answer)
        {
            var normalized = answer.Trim().ToLower();
            var bytes = Encoding.UTF8.GetBytes(normalized);
            var hash = SHA256.HashData(bytes);
            return Convert.ToHexString(hash);
        }

        // ✅ Code generator
        private static string GenerateCode(string prefix) =>
            $"WAFFY-{prefix}-{Guid.NewGuid().ToString()[..6].ToUpper()}";
    }
}