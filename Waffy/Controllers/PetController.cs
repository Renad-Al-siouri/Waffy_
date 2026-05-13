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
        private readonly IPostFlagRepository _flagRepo;

        public PetController(
            IPetRepository petRepo,
            IUserRepository userRepo,
            IAdoptionRepository adoptionRepo,
            ILostPetRepository lostPetRepo,
            IPostFlagRepository flagRepo)
        {
            _petRepo = petRepo;
            _userRepo = userRepo;
            _adoptionRepo = adoptionRepo;
            _lostPetRepo = lostPetRepo;
            _flagRepo = flagRepo;
        }

        // ════════════════════════════════════════════════════
        // HELPERS
        // ════════════════════════════════════════════════════

        private int? GetCurrentUserId() =>
            HttpContext.Session.GetInt32("UserId");

        private async Task<User?> GetCurrentUserAsync()
        {
            var id = GetCurrentUserId();
            return id == null ? null : await _userRepo.GetByIdAsync(id.Value);
        }

        private static string? Trunc(string? s, int max) =>
            s == null ? null : s.Length <= max ? s : s[..max];

        // ════════════════════════════════════════════════════
        // ADOPTION PAGE
        // ════════════════════════════════════════════════════

        [HttpGet]
        public async Task<IActionResult> Adapt()
        {
            var posts = (await _adoptionRepo.GetApprovedAsync()).ToList();
            var userId = GetCurrentUserId();
            var cards = new List<AdoptionCardDto>();

            foreach (var p in posts)
            {
                // ✅ جديد: كل الصور بدل صورة واحدة
                var images = (await _adoptionRepo.GetAllImagesAsync(p.IdPost)).ToList();
                var card = AdoptionCardDto.FromPost(p, images);

                if (userId.HasValue)
                    card.IsLikedByCurrentUser =
                        await _adoptionRepo.HasLikedAsync(p.IdPost, userId.Value);

                cards.Add(card);
            }

            return View(new AdoptionPageViewModel { Posts = cards });
        }

        // ════════════════════════════════════════════════════
        // ADOPTION FlagPost
        // ════════════════════════════════════════════════════

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> FlagPost(int postId, string reason)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            var alreadyFlagged = await _flagRepo.HasFlaggedAsync(postId, userId.Value);
            if (alreadyFlagged)
            {
                TempData["FlagError_" + postId] = "already";
                return RedirectToAction("Adapt");
            }

            await _flagRepo.AddFlagAsync(new PostFlag
            {
                IdPost = postId,
                IdUser = userId.Value,
                Reason = reason,
                CreatedAt = DateTime.UtcNow
            });

            TempData["FlagSuccess"] = "✅ Thank you. Our team will review this post.";
            return RedirectToAction("Adapt");
        }

        // ════════════════════════════════════════════════════
        // LOST PETS PAGE
        // ════════════════════════════════════════════════════

        [HttpGet]
        public async Task<IActionResult> Lost(int page = 1)
        {
            const int pageSize = 6;
            var totalCount = await _lostPetRepo.GetActiveTotalCountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            page = Math.Max(1, Math.Min(page, totalPages == 0 ? 1 : totalPages));

            var reports = (await _lostPetRepo.GetActivePagedAsync(page, pageSize)).ToList();
            var dtos = new List<LostReportDto>();

            foreach (var r in reports)
            {
                // ✅ جديد: كل الصور بدل صورة واحدة
                var images = (await _lostPetRepo.GetAllImagesAsync(r.IdLostReport)).ToList();
                dtos.Add(LostReportDto.FromReport(r, images));
            }

            return View(new LostPageViewModel
            {
                Reports = dtos,
                CurrentPage = page,
                TotalPages = totalPages == 0 ? 1 : totalPages
            });
        }

        // ════════════════════════════════════════════════════
        // PET RECORD
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
                if (!userPets.Any()) return RedirectToAction("Profile", "Account");
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
            if (GetCurrentUserId() == null) return RedirectToAction("Login", "Account");
            return View();
        }

        // ════════════════════════════════════════════════════
        // ADD PET — POST
        // ════════════════════════════════════════════════════

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(52_428_800)]
        [RequestFormLimits(MultipartBodyLengthLimit = 52_428_800)]
        public async Task<IActionResult> AddPet(
            string name, string species, string? breed, string? gender,
            string? birthDate, string? color, string? energyLevel,
            string? vaccinationStatus, bool goodWithChildren, bool goodWithPets,
            bool isNeutered, bool isTrained, string? medicalNotes, string? traits,
            string? microchipId, decimal? weight,
            IFormFile? petImage)          // ✅ مفرد — ليس List
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
                    ImageUrl = null,
                    CreatedAt = DateTime.UtcNow
                };

                var newPetId = await _petRepo.AddAsync(pet);

                // ✅ صورة واحدة فقط
                if (petImage != null && petImage.Length > 0)
                {
                    var url = await SaveFileAsync(petImage, "pets");
                    if (url != null)
                        await _petRepo.AddPetImageAsync(newPetId, url);
                }

                TempData["PetSuccess"] = $"{name} added successfully!";
                return RedirectToAction("Profile", "Account");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AddPet] ❌ {ex.GetType().Name}: {ex.Message}");
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

            var images = (await _petRepo.GetImagesAsync(petId)).ToList();
            return View(new EditPetViewModel { Pet = pet, Images = images });
        }

        // ════════════════════════════════════════════════════
        // EDIT PET — POST
        // ════════════════════════════════════════════════════


        [HttpPost]
        [ValidateAntiForgeryToken]        // ✅ أعيدي هذا — الـ form عادي بدون ملف
        [RequestSizeLimit(52_428_800)]
        public async Task<IActionResult> EditPet(
            int petId, string name, string species, string? breed, string? gender,
            string? birthDate, string? color, string? energyLevel,
            string? vaccinationStatus, bool goodWithChildren, bool goodWithPets,
            bool isNeutered, bool isTrained, string? medicalNotes, string? traits,
            string? microchipId, decimal? weight)
        // ✅ لا يوجد IFormFile هنا — الصورة تُدار بشكل منفصل
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
                Console.WriteLine($"[EditPet] ❌ {ex.GetType().Name}: {ex.Message}");
                TempData["PetError"] = "Something went wrong. Please try again.";
                return RedirectToAction("Profile", "Account");
            }
        }


        // ════════════════════════════════════════════════════
        // DELETE PET
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
        // DELETE PET IMAGE
        // ════════════════════════════════════════════════════

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePetImage(int imageId, int petId)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            var pet = await _petRepo.GetByIdAsync(petId);
            if (pet == null || pet.IdUsers != userId.Value)
                return RedirectToAction("Profile", "Account");

            await _petRepo.DeletePetImageAsync(imageId);
            TempData["PetSuccess"] = "Image deleted.";
            return RedirectToAction("EditPetPage", new { petId });
        }

        // ════════════════════════════════════════════════════
        // SET MAIN IMAGE
        // ════════════════════════════════════════════════════

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetMainImage(int imageId, int petId)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            var pet = await _petRepo.GetByIdAsync(petId);
            if (pet == null || pet.IdUsers != userId.Value)
                return RedirectToAction("Profile", "Account");

            await _petRepo.SetMainImageAsync(petId, imageId);
            TempData["PetSuccess"] = "Main image updated.";
            return RedirectToAction("EditPetPage", new { petId });
        }



        // ════════════════════════════════════════════════════
        // UploadPetPhoto — غيّري [FromRoute] إلى [FromQuery]
        // ════════════════════════════════════════════════════
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(52_428_800)]
        public async Task<IActionResult> UploadPetPhoto(
            [FromQuery] int petId,     // ✅ FromQuery بدل FromRoute
            IFormFile? photo)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            try
            {
                if (photo == null || photo.Length == 0)
                {
                    TempData["PetError"] = "No file selected.";
                    return RedirectToAction("Profile", "Account");
                }

                var pet = await _petRepo.GetByIdAsync(petId);
                if (pet == null || pet.IdUsers != userId.Value)
                    return RedirectToAction("Profile", "Account");

                var imageUrl = await SaveFileAsync(photo, "pets");
                if (imageUrl != null)
                    await _petRepo.AddPetImageAsync(petId, imageUrl);

                TempData["PetSuccess"] = "Photo uploaded!";
                return RedirectToAction("Profile", "Account");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UploadPetPhoto] ❌ {ex.Message}");
                TempData["PetError"] = "Upload failed.";
                return RedirectToAction("Profile", "Account");
            }
        }


        // ════════════════════════════════════════════════════
        // ReplacePetPhoto — غيّري [FromRoute] إلى [FromQuery]
        // ════════════════════════════════════════════════════
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(52_428_800)]
        public async Task<IActionResult> ReplacePetPhoto(
            [FromQuery] int petId,     // ✅ FromQuery بدل FromRoute
            IFormFile? photo)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            try
            {
                var pet = await _petRepo.GetByIdAsync(petId);
                if (pet == null || pet.IdUsers != userId.Value)
                    return RedirectToAction("Profile", "Account");

                if (photo == null || photo.Length == 0)
                {
                    TempData["PetError"] = "No file selected.";
                    return RedirectToAction("EditPetPage", new { petId });
                }

                // حذف كل الصور القديمة
                var oldImages = (await _petRepo.GetImagesAsync(petId)).ToList();
                foreach (var img in oldImages)
                    await _petRepo.DeletePetImageAsync(img.Id);

                // رفع الصورة الجديدة
                var url = await SaveFileAsync(photo, "pets");
                if (url != null)
                    await _petRepo.AddPetImageAsync(petId, url);

                TempData["PetSuccess"] = "Photo updated successfully!";
                return RedirectToAction("EditPetPage", new { petId });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ReplacePetPhoto] ❌ {ex.GetType().Name}: {ex.Message}");
                TempData["PetError"] = "Failed to update photo.";
                return RedirectToAction("EditPetPage", new { petId });
            }
        }


        // ════════════════════════════════════════════════════
        // REMOVE PET PHOTO — POST  ✅ جديد
        // يحذف الصورة الحالية بدون استبدال
        // ════════════════════════════════════════════════════
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemovePetPhoto(int petId)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            try
            {
                var pet = await _petRepo.GetByIdAsync(petId);
                if (pet == null || pet.IdUsers != userId.Value)
                    return RedirectToAction("Profile", "Account");

                var images = (await _petRepo.GetImagesAsync(petId)).ToList();
                foreach (var img in images)
                    await _petRepo.DeletePetImageAsync(img.Id);

                TempData["PetSuccess"] = "Photo removed.";
                return RedirectToAction("EditPetPage", new { petId });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RemovePetPhoto] ❌ {ex.GetType().Name}: {ex.Message}");
                TempData["PetError"] = "Failed to remove photo.";
                return RedirectToAction("EditPetPage", new { petId });
            }
        }

        // ════════════════════════════════════════════════════
        // SUBMIT ADOPTION
        // ════════════════════════════════════════════════════


        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(52_428_800)]
        [RequestFormLimits(MultipartBodyLengthLimit = 52_428_800)]
        public async Task<IActionResult> SubmitAdoption(
            int petId, string city, string? area, string contactPhone,
            string? contactMethod, string description, string reason)
        // ✅ حذفنا List<IFormFile>? photos — نقرأها مباشرة من Request
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

            var existingPosts = await _adoptionRepo.GetByUserIdAsync(userId.Value);
            if (existingPosts.Any(p => p.IdPet == petId && p.AdoptionStatus != "Closed"))
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
                Status = PostStatus.Pending.ToString(),
                AdoptionStatus = AdoptionStatus.Available.ToString(),
                PostCode = GenerateCode("ADT"),
                BirthDate = pet.BirthDate,
                Age = pet.BirthDate.HasValue
                                   ? (int)((DateTime.Today - pet.BirthDate.Value).TotalDays / 30.44)
                                   : null,
                CreatedAt = DateTime.UtcNow
            };

            var newPostId = await _adoptionRepo.AddAsync(post);

            // ✅ قراءة الصور مباشرة من Request.Form.Files — أكثر موثوقية من model binding
            var photoFiles = Request.Form.Files.GetFiles("photos");
            foreach (var photo in photoFiles.Where(f => f.Length > 0).Take(5))
            {
                var url = await SaveFileAsync(photo, "adoption");
                if (url != null)
                    await _adoptionRepo.AddImageAsync(newPostId, url);
            }

            TempData["AdoptSuccess"] = $"Ad submitted! Code: {post.PostCode}";
            return RedirectToAction("Petrecord", new { petId });
        }


        // ════════════════════════════════════════════════════
        // SUBMIT LOST REPORT
        // ════════════════════════════════════════════════════


        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(52_428_800)]
        [RequestFormLimits(MultipartBodyLengthLimit = 52_428_800)]
        public async Task<IActionResult> SubmitLostReport(
            int petId, string city, string? area, string? lastSeenLocation,
            string? dateLost, string description, string? specialMarks,
            string secretQuestion, string secretAnswer, string contactPhone,
            decimal rewardAmount, string expiryDays)
        // ✅ حذفنا List<IFormFile>? photos — نقرأها مباشرة من Request
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
                DateLost = DateTime.TryParse(dateLost, out var pd) ? pd : DateTime.Today,
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

            // ✅ قراءة الصور مباشرة من Request.Form.Files — أكثر موثوقية من model binding
            var photoFiles = Request.Form.Files.GetFiles("photos");
            foreach (var photo in photoFiles.Where(f => f.Length > 0).Take(5))
            {
                var url = await SaveFileAsync(photo, "lost");
                if (url != null)
                    await _lostPetRepo.AddImageAsync(newReportId, url);
            }

            TempData["LostSuccess"] = $"Lost ad published! Code: {report.ReportCode}";
            return RedirectToAction("Petrecord", new { petId });
        }

        [HttpPost]
        public async Task<IActionResult> ToggleLike(int postId)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Json(new { error = "not_logged_in" });

            var hasLiked = await _adoptionRepo.HasLikedAsync(postId, userId.Value);
            if (!hasLiked) await _adoptionRepo.AddLikeAsync(postId, userId.Value);
            else await _adoptionRepo.RemoveLikeAsync(postId, userId.Value);

            var count = await _adoptionRepo.GetLikesAsync(postId);
            return Json(new { count, liked = !hasLiked });
        }

        // ════════════════════════════════════════════════════
        // PRIVATE HELPERS
        // ════════════════════════════════════════════════════

        private static readonly string[] AllowedExtensions =
            { ".jpg", ".jpeg", ".png", ".webp" };
        private const long MaxFileSize = 5 * 1024 * 1024; // 5 MB

        // ✅ نفس طريقة المتجر بالضبط — FileMode.Create بدون FileAccess.Write
        //    بدون wwwroot existence check الذي كان يُسكت الأخطاء
        private async Task<string?> SaveFileAsync(IFormFile file, string subFolder)
        {
            try
            {
                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!AllowedExtensions.Contains(ext)) return null;
                if (file.Length > MaxFileSize) return null;

                var folder = Path.Combine(
                    Directory.GetCurrentDirectory(), "wwwroot", "uploads", subFolder);
                Directory.CreateDirectory(folder);

                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";

                // ✅ FileMode.Create بدون FileAccess.Write — نفس المتجر
                await using var stream =
                    new FileStream(Path.Combine(folder, fileName), FileMode.Create);
                await file.CopyToAsync(stream);

                return $"/uploads/{subFolder}/{fileName}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SaveFile] ❌ {ex.GetType().Name}: {ex.Message}");
                return null;
            }
        }

       

        private static List<string> ValidatePetInput(string? name, string? species)
        {
            var e = new List<string>();
            if (string.IsNullOrWhiteSpace(name)) e.Add("Pet name is required.");
            if (name?.Length > 100) e.Add("Pet name is too long.");
            if (string.IsNullOrWhiteSpace(species)) e.Add("Pet type is required.");
            return e;
        }

        private static List<string> ValidateAdoptionInput(
            string? city, string? phone, string? desc, string? reason)
        {
            var e = new List<string>();
            if (string.IsNullOrWhiteSpace(city)) e.Add("City is required.");
            if (string.IsNullOrWhiteSpace(phone)) e.Add("Contact phone is required.");
            if (string.IsNullOrWhiteSpace(desc)) e.Add("Description is required.");
            if (string.IsNullOrWhiteSpace(reason)) e.Add("Reason is required.");
            return e;
        }

        private static List<string> ValidateLostReportInput(
            string? city, string? desc, string? q, string? a, string? phone)
        {
            var e = new List<string>();
            if (string.IsNullOrWhiteSpace(city)) e.Add("City is required.");
            if (string.IsNullOrWhiteSpace(desc)) e.Add("Description is required.");
            if (string.IsNullOrWhiteSpace(q)) e.Add("Secret question is required.");
            if (string.IsNullOrWhiteSpace(a)) e.Add("Secret answer is required.");
            if (string.IsNullOrWhiteSpace(phone)) e.Add("Contact phone is required.");
            return e;
        }

        private static string HashAnswer(string answer)
        {
            var bytes = Encoding.UTF8.GetBytes(answer.Trim().ToLower());
            return Convert.ToHexString(SHA256.HashData(bytes));
        }

        private static string GenerateCode(string prefix) =>
            $"WAFFY-{prefix}-{Guid.NewGuid().ToString()[..6].ToUpper()}";
    }
}