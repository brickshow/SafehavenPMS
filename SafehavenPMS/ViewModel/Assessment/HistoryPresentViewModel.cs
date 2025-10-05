using Microsoft.AspNetCore.Authorization;
namespace SafehavenPMS.ViewModel.Assessment
{
[Authorize]
    public class HistoryPresentViewModel
    {
        public int? HistoryPresentId { get; set; }
        public string? OnsetOfDrugUse { get; set; }
        public string? ReasonForFirstUse { get; set; }
        public string? HistoryOfImprisonment { get; set; }

        public string? PreviousDrugRehab { get; set; }
        public string? WhoInvitedFirstUse { get; set; }
        public int? NumberOfPeopleFirstUse { get; set; }
        public string? LastUseOfSubstance { get; set; }
        public string? AmountConsumedFirstUse { get; set; }
    }
}

