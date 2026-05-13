namespace Waffy.Models.ViewModels
{
    public class AdoptionReservationAdminDto
    {
        public int IdReservation { get; set; }
        public int IdPost { get; set; }
        public string PetName { get; set; } = string.Empty;
        public string AnimalType { get; set; } = string.Empty;
        public string PostedByType { get; set; } = string.Empty;
        public string RequesterName { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Message { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}