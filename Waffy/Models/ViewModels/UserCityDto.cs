namespace Waffy.Models.ViewModels
{
    public class UserCityDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string? Area { get; set; }
        public int ProximityRank { get; set; } // 1=نفس المنطقة, 2=نفس المدينة
    }
}