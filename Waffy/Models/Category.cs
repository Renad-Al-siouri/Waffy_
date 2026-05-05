namespace Waffy.Models
{
    public class Category
    {
        public int IdCategory { get; set; }
        public int IdStore { get; set; }
        public string Name { get; set; } = "";
        public string? Icon { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}