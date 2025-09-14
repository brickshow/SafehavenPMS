using System;
using System.Collections.Generic;

namespace SafehavenPMS.ViewModel
{
     // Overview tab VM - updated to match the _OverviewTab.cshtml expectations
    public class PatientOverViewTabViewModel
    {
        public int PatientId { get; set; }

        // Simple lists shown as badges
        public List<string> FoodAllergies { get; set; } = new List<string>();
        public List<string> DrugAllergies { get; set; } = new List<string>();
        public List<string> ActiveMedications { get; set; } = new List<string>();

        // Treatment team: can be simple names or richer objects
        public List<TreatmentTeamMemberViewModel> TreatmentTeams { get; set; } = new List<TreatmentTeamMemberViewModel>();

        // Today's notes shown in the Overview card
        public List<NoteSummaryViewModel> TodaysNotes { get; set; } = new List<NoteSummaryViewModel>();
    }

    public class TreatmentTeamMemberViewModel
    {
        public int? ClinicalStaffId { get; set; }
        public string? Firstname { get; set; }
        public string? Lastname { get; set; }
        public string? Position { get; set; }
        public string? AvatarUrl { get; set; }

        public string FullName => $"{Firstname} {Lastname}".Trim();
    }

    public class NoteSummaryViewModel
    {
        public int NoteId { get; set; }
        public string Summary { get; set; } = string.Empty;
        public string? CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}