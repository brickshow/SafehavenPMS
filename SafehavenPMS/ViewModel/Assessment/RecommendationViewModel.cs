using Microsoft.AspNetCore.Authorization;
namespace SafehavenPMS.ViewModel.Assessment
{
[Authorize]
    public class RecommendationViewModel
    {
        public string? ProgramType { get; set; }
        public string? ExpectedDuration { get; set; }
        public string? Reason { get; set; }
    }
}

