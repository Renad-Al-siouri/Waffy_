namespace Waffy.Models
{
    public class Cart
    {
        public int IdCart { get; set; }      // ← يجب أن يطابق اسم عمود DB بالضبط
        public int IdUser { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}