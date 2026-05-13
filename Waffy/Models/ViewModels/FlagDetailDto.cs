namespace Waffy.Models.ViewModels
{
    public class FlagDetailDto
    {
        public int IdFlag { get; set; }
        public int IdPost { get; set; }
        public int IdUser { get; set; }
        public string ReporterName { get; set; } = string.Empty;
        public string ReporterEmail { get; set; } = string.Empty;
        public string? ReporterPhone { get; set; }
        public string Reason { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsReviewed { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string? AdminAction { get; set; }
        public string? AdminNote { get; set; }
    }
}