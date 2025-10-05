using Microsoft.AspNetCore.Authorization;
namespace SafehavenPMS.ViewModel.Assessment
{
    // Drug History section of the assessment form
[Authorize]
    public class DrugUseHistoryViewModel
    {
        // Each drug use entry represents a row in the drug history table
        public List<DrugUseEntry>? DrugUseEntries { get; set; } = new();
    }

    // Individual drug entry details
    public class DrugUseEntry
    {
        // Basic information
        public int? DrugHistoryId { get; set; }
        public int? AssessmentFormId { get; set; }

        // Drug use details matching the table columns
        public string? SubstanceName { get; set; }
        public string? Route { get; set; }
        public string? QuantityPerDay { get; set; }
        public string? Frequency { get; set; }
        public string? FirstUse { get; set; }
        public string? EffectsWhenHigh { get; set; }
        public string? EffectsWhenWanes { get; set; }

        // Audit fields matching your pattern
        public DateTime? CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }
}

