namespace Waffy.Models
{
    public class StoreImage
    {
        public int Id { get; set; }
        public int IdStore { get; set; }
        public string ImageUrl { get; set; } = "";
        // "Logo" | "Cover" | "Gallery"
        public string ImageType { get; set; } = "Gallery";
        public int SortOrder { get; set; }
        public DateTime UploadedAt { get; set; }
    }
}