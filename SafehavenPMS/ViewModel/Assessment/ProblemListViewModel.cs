using Microsoft.AspNetCore.Authorization;
namespace SafehavenPMS.ViewModel.Assessment
{
[Authorize]
    public class ProblemListViewModel
    {
        public List<string> Problems { get; set; } = new List<string>();
    }
}
