using System.Text.Json;

namespace Waffy.Models.ViewModels
{
    public class AdoptionPageViewModel
    {
        public List<AdoptionCardDto> Posts { get; set; } = new();
    }
}