using Waffy.Models;
public class LostReportDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Breed { get; set; } = "";
    public string Color { get; set; } = "";
    public string City { get; set; } = "";
    public string Area { get; set; } = "";
    public decimal Reward { get; set; }
    public string Image { get; set; } = "";
    public string ReportCode { get; set; } = "";
    public string DateLost { get; set; } = "";
    public string Description { get; set; } = "";
    public string SpecialMarks { get; set; } = "";

    public static LostReportDto FromReport(LostPetReport r, string imageUrl)
    {
        return new LostReportDto
        {
            Id = r.IdLostReport,
            Name = r.PetName ?? "Unknown",
            Breed = r.Breed ?? "",
            Color = r.Color ?? "",
            City = r.City ?? "",
            Area = r.Area ?? "",
            Reward = r.RewardAmount ?? 0,
            Image = string.IsNullOrEmpty(imageUrl)
                           ? "/Images/lost/Beagle.png"
                           : imageUrl,
            ReportCode = r.ReportCode ?? "",
            DateLost = r.DateLost.HasValue
                           ? r.DateLost.Value.ToString("yyyy-MM-dd")
                           : "",
            Description = r.Description ?? "",
            SpecialMarks = r.SpecialMarks ?? ""
        };
    }
}