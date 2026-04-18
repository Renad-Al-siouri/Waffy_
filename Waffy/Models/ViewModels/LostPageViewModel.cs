
namespace Waffy.Models.ViewModels
{
    public class LostPageViewModel
    {
        public List<LostReportDto> Reports { get; set; } = new();
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public bool HasNextPage => CurrentPage < TotalPages;
        public bool HasPreviousPage => CurrentPage > 1;

    }
}