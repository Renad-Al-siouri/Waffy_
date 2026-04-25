
namespace Waffy.Models.ViewModels
{
    public class AdoptionAdsViewModel
    {
        public List<AdoptionPost> Posts { get; set; } = new();
        public User User { get; set; } = null!;
        public Dictionary<int, List<AdoptionReservation>> Reservations { get; set; } = new();
        public List<AdoptionReservation> MyRequests { get; set; } = new();

    }
}