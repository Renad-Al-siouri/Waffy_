namespace Waffy.Models.ViewModels
{
    public class FlaggedPostDto
    {
        public int IdPost { get; set; }
        public string PetName { get; set; } = string.Empty;
        public string AnimalType { get; set; } = string.Empty;
        public string PostedByType { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int FlagCount { get; set; }
        public string TopReason { get; set; } = string.Empty;
        public DateTime LastFlagDate { get; set; }
        public int PostedById { get; set; }
    }
}