namespace Waffy.Models
{
    public class PetImage
    {
        public int Id { get; set; }
        public int IdPet { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public bool IsMain { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}