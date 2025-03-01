using Waffy.Models;

public class LostReportDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string AnimalType { get; set; } = "";
    public string Breed { get; set; } = "";
    public string Color { get; set; } = "";
    public string City { get; set; } = "";
    public string Area { get; set; } = "";
    public decimal Reward { get; set; }
    public bool HasReward => Reward > 0;
    public string Image { get; set; } = "";

    // ✅ جديد: قائمة كل الصور للـ carousel
    public List<string> Images { get; set; } = new();

    public string DateLost { get; set; } = "";
    public string Description { get; set; } = "";
    public string SpecialMarks { get; set; } = "";
    public string ContactPhone { get; set; } = "";
    public string SecretQuestion { get; set; } = "";
    public string ExpiryDate { get; set; } = "";


    // ✅ تغيّر: يقبل List<string> بدل string واحدة
    public static LostReportDto FromReport(LostPetReport r, List<string> imageUrls)
    {
        const string fallback = "/Images/lost/Beagle.png";

        var images = imageUrls.Where(u => !string.IsNullOrEmpty(u)).ToList();
        if (images.Count == 0) images.Add(fallback);

        return new LostReportDto
        {
            Id = r.IdLostReport,
            Name = r.PetName ?? "Unknown",
            AnimalType = r.AnimalType ?? "",
            Breed = r.Breed ?? "",
            Color = r.Color ?? "",
            City = r.City ?? "",
            Area = r.Area ?? "",
            Reward = r.RewardAmount ?? 0,
            Image = images[0],
            Images = images,
            DateLost = r.DateLost.HasValue
                             ? r.DateLost.Value.ToString("MMM dd, yyyy")
                             : "",
            Description = r.Description ?? "",
            SpecialMarks = r.SpecialMarks ?? "",
            ContactPhone = r.ContactPhone ?? "",
            SecretQuestion = r.SecretQuestion ?? "",
            ExpiryDate = r.ExpiryDate.HasValue
                             ? r.ExpiryDate.Value.ToString("MMM dd, yyyy")
                             : ""
        };
    }
}