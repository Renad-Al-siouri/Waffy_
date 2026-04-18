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
        public string ContactPhone { get; set; } = "";

        // ✅ Mapping هنا في DTO — مش في Controller ومش في Model
        public static AdoptionCardDto FromPost(AdoptionPost post, string imageUrl)
        {
            return new AdoptionCardDto
            {
                Id = post.IdPost,
                Type = (post.AnimalType ?? "other").ToLower(),
                Name = post.PetName ?? "Unknown",
                Breed = post.Breed ?? "",
                Age = post.Age.HasValue
                               ? $"{post.Age} year{(post.Age > 1 ? "s" : "")}"
                               : "Unknown",
                Size = "Medium",
                Temperament = ToTemperament(post.EnergyLevel),
                Vaccinations = post.IsVaccinated
                               ? new[] { "Up to date" }
                               : new[] { "Not vaccinated" },
                Health = post.HealthStatus ?? "Healthy",
                Gender = post.Gender ?? "Unknown",
                Description = post.Description ?? "",
                Image = string.IsNullOrEmpty(imageUrl)
                               ? "/Images/adapt/dog.png"
                               : imageUrl,
                City = post.City ?? "",
                ContactPhone = post.ContactPhone ?? ""
            };
        }

        // ✅ Temperament mapping في DTO — المكان الصحيح
        private static string ToTemperament(string? energyLevel) => energyLevel switch
        {
            "High" => "Energetic, Playful",
            "Medium" => "Friendly, Calm",
            "Low" => "Gentle, Quiet",
            _ => "Friendly"
        };
    }
}