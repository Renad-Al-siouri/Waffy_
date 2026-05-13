namespace Waffy.Models
{
    public class PostFlag
    {
        public int IdFlag { get; set; }
        public int IdPost { get; set; }
        public int IdUser { get; set; }
        public string Reason { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}