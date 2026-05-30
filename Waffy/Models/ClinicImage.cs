namespace Waffy.Models
{
    public class ClinicImage
    {
        public int Id { get; set; }
        public int IdClinic { get; set; }
        public string ImageUrl { get; set; } = "";
        public int SortOrder { get; set; }
        public DateTime UploadedAt { get; set; }
    }
}