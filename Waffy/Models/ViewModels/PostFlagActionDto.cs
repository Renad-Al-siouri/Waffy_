namespace Waffy.Models.ViewModels
{
    public class PostFlagActionDto
    {
        public int IdAction { get; set; }
        public int IdPost { get; set; }
        public int AdminId { get; set; }
        public string ActionType { get; set; } = string.Empty;
        public int? TargetUserId { get; set; }
        public string? TargetName { get; set; }
        public string? Message { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}