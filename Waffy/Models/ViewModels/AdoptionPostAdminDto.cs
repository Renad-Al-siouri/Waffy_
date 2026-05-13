namespace Waffy.Models.ViewModels
{
    public class AdoptionPostAdminDto
    {
        public int IdPost { get; set; }
        public string PetName { get; set; } = string.Empty;
        public string AnimalType { get; set; } = string.Empty;
        public string PostedByType { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string AdoptionStatus { get; set; } = string.Empty;
        public int Likes { get; set; }
        public int FlagCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}