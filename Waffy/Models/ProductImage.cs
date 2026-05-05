namespace Waffy.Models
{
    public class ProductImage
    {
        public int Id { get; set; }
        public int IdProduct { get; set; }
        public string ImageUrl { get; set; } = "";
        public int SortOrder { get; set; }
        public bool IsMain { get; set; }
        public DateTime UploadedAt { get; set; }
    }
}