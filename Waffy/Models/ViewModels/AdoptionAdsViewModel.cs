
namespace Waffy.Models.ViewModels
{
    public class AdoptionAdsViewModel
    {
        public List<AdoptionPost> Posts { get; set; } = new();
        public User User { get; set; } = null!;
    }
}