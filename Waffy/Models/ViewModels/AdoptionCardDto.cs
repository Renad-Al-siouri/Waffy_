namespace Waffy.Models.ViewModels
{
    public class AdoptionCardDto
    {
        public int Id { get; set; }
        public string Type { get; set; } = "";
        public string Name { get; set; } = "";
        public string Breed { get; set; } = "";
        public string Age { get; set; } = "";
        public string Size { get; set; } = "";
        public string Temperament { get; set; } = "";
        public int Likes { get; set; }
        public string[] Vaccinations { get; set; } = Array.Empty<string>();
        public string Health { get; set; } = "";
        public string Gender { get; set; } = "";
        public string Description { get; set; } = "";
        public string Image { get; set; } = "";

        // ✅ جديد: قائمة كل الصور للـ carousel
        public List<string> Images { get; set; } = new();

        public string City { get; set; } = "";
        public string Area { get; set; } = "";
        public string ContactPhone { get; set; } = "";
        public string ContactName { get; set; } = "";
        public string Color { get; set; } = "";
        public string EnergyLevel { get; set; } = "";
        public string PostCode { get; set; } = "";
        public bool IsNeutered { get; set; }
        public bool GoodWithChildren { get; set; }
        public bool GoodWithPets { get; set; }
        public bool IsLikedByCurrentUser { get; set; }

        // ✅ تغيّر: يقبل List<string> بدل string واحدة
        public static AdoptionCardDto FromPost(AdoptionPost post, List<string> imageUrls)
        {
            const string fallback = "/Images/adapt/dog.png";

            var images = imageUrls.Where(u => !string.IsNullOrEmpty(u)).ToList();
            if (images.Count == 0) images.Add(fallback);

            return new AdoptionCardDto
            {
                Id = post.IdPost,
                Type = (post.AnimalType ?? "other").ToLower(),
                Name = post.PetName ?? "Unknown",
                Breed = post.Breed ?? "Mixed",
                Gender = post.Gender ?? "Unknown",
                City = post.City ?? "",
                ContactPhone = post.ContactPhone ?? "",
                Description = post.Description ?? "",
                Health = post.HealthStatus ?? "Healthy",
                Likes = post.Likes,
                Age = CalcAge(post.BirthDate, post.Age),
                Size = ToSize(post.AnimalType),
                Temperament = BuildTemperament(post),
                Vaccinations = post.IsVaccinated
                                   ? new[] { "✅ Vaccinated" }
                                   : new[] { "❌ Not vaccinated" },
                Image = images[0],
                Images = images,
                IsNeutered = post.IsNeutered,
                GoodWithChildren = post.GoodWithChildren,
                GoodWithPets = post.GoodWithPets,
                EnergyLevel = post.EnergyLevel ?? "Medium",
                Color = post.Color ?? "",
                PostCode = post.PostCode ?? "",
                Area = post.Area ?? "",
                ContactName = post.ContactName ?? ""
            };
        }

        private static string CalcAge(DateTime? birthDate, int? directAge = null)
        {
            // ① إذا عندنا BirthDate نحسب منه (User flow)
            if (birthDate.HasValue && birthDate.Value <= DateTime.Today)
            {
                var today = DateTime.Today;
                var birth = birthDate.Value;
                int years = today.Year - birth.Year;
                int months = today.Month - birth.Month;

                if (today.Day < birth.Day) months--;
                if (months < 0) { years--; months += 12; }

                if (years > 0 && months > 0)
                    return $"{years} Year{(years > 1 ? "s" : "")} & {months} Month{(months > 1 ? "s" : "")}";
                if (years > 0)
                    return $"{years} Year{(years > 1 ? "s" : "")}";
                if (months > 0)
                    return $"{months} Month{(months > 1 ? "s" : "")}";

                int days = (today - birth).Days;
                return days <= 0 ? "Newborn" : $"{days} Day{(days > 1 ? "s" : "")}";
            }

            // ② Fallback: Age المباشر (Clinic flow)
            if (directAge.HasValue && directAge.Value >= 0)
                return directAge.Value == 0
                    ? "Newborn"
                    : $"{directAge.Value} Year{(directAge.Value > 1 ? "s" : "")}";

            return "Unknown";
        }

        private static string ToSize(string? animalType) => animalType?.ToLower() switch
        {
            "dog" => "Medium",
            "cat" => "Small",
            "bird" => "Small",
            "rabbit" => "Small",
            "hamster" => "Tiny",
            "snake" => "Medium",
            _ => "Medium"
        };

        private static string BuildTemperament(AdoptionPost post)
        {
            var traits = new List<string>();
            traits.Add(post.EnergyLevel switch
            {
                "High" => "Energetic",
                "Medium" => "Friendly",
                "Low" => "Gentle",
                _ => "Friendly"
            });
            if (post.GoodWithChildren) traits.Add("Good with kids");
            if (post.GoodWithPets) traits.Add("Good with pets");
            if (post.IsNeutered) traits.Add("Neutered");
            return string.Join(", ", traits);
        }

        private static string ToTemperament(string? energyLevel) => energyLevel switch
        {
            "High" => "Energetic, Playful",
            "Medium" => "Friendly, Calm",
            "Low" => "Gentle, Quiet",
            _ => "Friendly"
        };
    }
}