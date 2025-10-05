using Microsoft.AspNetCore.Authorization;
namespace SafehavenPMS.ViewModel.Assessment
{
   namespace SafehavenPMS.ViewModel.Assessment
{
[Authorize]
    public class DiagnosisViewModel
    {
        public DiagnosisViewModel()
        {
            SubstanceUses = new List<SubstanceUseViewModel>();
        }

        // Collection of substance use entries
        public List<SubstanceUseViewModel> SubstanceUses { get; set; }
    }

    public class SubstanceUseViewModel
    {
        public string SubstanceName { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
    }
}
}
