using Waffy.Models;

namespace Waffy.Models.ViewModels
{
    public class PetProfileViewModel
    {
        public User User { get; set; } = null!;
        public List<Pet> Pets { get; set; } = new();

    }
}
