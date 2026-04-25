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

        // ✅ Mapping هنا في DTO — مش في Controller ومش في Model
        public static AdoptionCardDto FromPost(AdoptionPost post, string imageUrl)
        {
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

                Age = CalcAge(post.BirthDate),

                // ✅ Size من الـ EnergyLevel
                Size = ToSize(post.AnimalType),

                // ✅ Temperament حقيقي
                Temperament = BuildTemperament(post),

                // ✅ Vaccinations حقيقي
                Vaccinations = post.IsVaccinated
                               ? new[] { "✅ Vaccinated" }
                               : new[] { "❌ Not vaccinated" },

                // ✅ Image حقيقي
                Image = string.IsNullOrEmpty(imageUrl)
                        ? "/Images/adapt/dog.png"
                        : imageUrl,

                // ✅ معلومات إضافية
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
        // ✅ helper بنفس منطق Pet.AgeDisplay
        private static string CalcAge(DateTime? birthDate)
        {
            if (!birthDate.HasValue) return "Unknown";

            // ✅ لو التاريخ في المستقبل
            if (birthDate.Value > DateTime.Today) return "Newborn";

            var today = DateTime.Today;
            var birth = birthDate.Value;

            int years = today.Year - birth.Year;
            int months = today.Month - birth.Month;

            if (today.Day < birth.Day)
                months--;

            if (months < 0)
            {
                years--;
                months += 12;
            }

            if (years > 0 && months > 0)
                return $"{years} Year{(years > 1 ? "s" : "")} & {months} Month{(months > 1 ? "s" : "")}";

            if (years > 0)
                return $"{years} Year{(years > 1 ? "s" : "")}";

            if (months > 0)
                return $"{months} Month{(months > 1 ? "s" : "")}";

            int days = (today - birth).Days;
            return days <= 0 ? "Newborn" : $"{days} Day{(days > 1 ? "s" : "")}";
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