namespace Waffy.Models
{
    public class DangerReportImage
    {
        public int Id { get; set; }
        public int IdDangReport { get; set; }
        public string ImageUrl { get; set; } = "";
        public int SortOrder { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}