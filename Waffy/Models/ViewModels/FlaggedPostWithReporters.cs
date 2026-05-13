namespace Waffy.Models.ViewModels
{
    public class FlaggedPostWithReporters
    {
        public FlaggedPostDto Post { get; set; } = null!;
        public IEnumerable<FlagDetailDto> Reporters { get; set; }
            = new List<FlagDetailDto>();
        public IEnumerable<PostFlagActionDto> Actions { get; set; }
    = new List<PostFlagActionDto>();
    }
}