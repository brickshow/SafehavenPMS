using System;
using System.Collections.Generic;
using SafehavenPMS.Models; // For Intervention
using SafehavenPMS.ViewModel.PatientProfile;
using Microsoft.AspNetCore.Authorization;


namespace SafehavenPMS.ViewModel
{
[Authorize]
    public class PatientProfilePageViewModel
    {
        // Basic patient header info
        public int PatientId { get; set; }
        public string? PatientRefId { get; set; }
        public string PatientName { get; set; } = "-";
        public string? AvatarUrl { get; set; } // URL or path to the patient's avatar image
        public string? PatientNumber { get; set; }
        public string? Status { get; set; }
        public string? Age { get; set; }
        public string? Sex { get; set; }
        public string? Address { get; set; }

        // Tab view models
        public PatientOverViewTabViewModel OverViewTab { get; set; }
        public PatientPersonalInfoTabViewModel PersonalInfoTab { get; set; }
        public PatientMedicalHistoryTabViewModel MedicalHistoryTab { get; set; }
        public PatientClinicalFormTabViewModel ClinicalFormTab { get; set; }
        public PatientTreatmentPlanTabViewModel TreatmentPlanTab { get; set; }
        public PatientProgressNotesTabViewModel ProgressNotesTab { get; set; }
        public PatientActivityLogTabViewModel ActivityLogTab { get; set; }

        // Add this property to hold all interventions for the patient
        public List<Intervention> Interventions { get; set; } = new List<Intervention>();

        public PatientProfilePageViewModel()
        {
            OverViewTab = new PatientOverViewTabViewModel();
            PersonalInfoTab = new PatientPersonalInfoTabViewModel();
            MedicalHistoryTab = new PatientMedicalHistoryTabViewModel();
            ClinicalFormTab = new PatientClinicalFormTabViewModel();
            TreatmentPlanTab = new PatientTreatmentPlanTabViewModel();
            ProgressNotesTab = new PatientProgressNotesTabViewModel();
            ActivityLogTab = new PatientActivityLogTabViewModel();
        }
    }
    public class PatientMedicalHistoryTabViewModel
    {
        // Add properties relevant to medical history
    }
    public class PatientClinicalFormTabViewModel { }

    // replaced empty ProgressNotes VM with a bindable model + helper VM types
    public class PatientProgressNotesTabViewModel
    {
        public int? PatientId { get; set; }
        // Interventions to render in the left panel (summary info)
        public List<InterventionSummaryViewModel> Interventions { get; set; } = new();

        // Optional: which intervention should be selected by default (used for server-side selection)
        public int? SelectedInterventionId { get; set; }

        // Filter (All / Active / Completed) - used by the dropdown in the partial
        public string InterventionFilter { get; set; } = "All";

        // Model used when creating a new progress note from the UI
        public ProgressNoteCreateViewModel NewProgressNote { get; set; } = new ProgressNoteCreateViewModel();

        // Convenience: get selected intervention object (null when none matched)
        public InterventionSummaryViewModel SelectedIntervention =>
            SelectedInterventionId.HasValue
                ? Interventions.FirstOrDefault(x => x.InterventionId == SelectedInterventionId.Value)
                : Interventions.FirstOrDefault();
    }

    // Summary data for an intervention shown in the list
    public class InterventionSummaryViewModel
    {
        public int InterventionId { get; set; }
        public string Title { get; set; } = "-";
        public string Description { get; set; } = "-";
        public string Status { get; set; } = "Active"; // Active | Completed | Inactive
        public string Clinician { get; set; } = "-";

        // Last note date / display string used in the list tile
        public DateTime? LastNoteDate { get; set; }
        public string LastNoteDisplay { get; set; } // Add this property

        // Progress notes tied to this intervention (ordered newest first)
        public List<ProgressNoteSummaryViewModel> ProgressNotes { get; set; } = new();
    }

    // Summary of a progress note for display in the right panel
    public class ProgressNoteSummaryViewModel
    {
        internal int? InterventionId;

        public int ProgressNoteId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Clinician { get; set; } = "-";

        // raw SOAP text or structured fields
        public string SoapRaw { get; set; } = "";

        // Optional structured SOAP parts (S,O,A,P) for easier rendering
        public string Subjective { get; set; } = "";
        public string Objective { get; set; } = "";
        public string Assessment { get; set; } = "";
        public string Plan { get; set; } = "";

        // small helper to render a combined short line (used in list)
        public string ShortSummary
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(Subjective)) return Subjective.Length > 80 ? Subjective[..80] + "…" : Subjective;
                if (!string.IsNullOrWhiteSpace(Objective)) return Objective.Length > 80 ? Objective[..80] + "…" : Objective;
                return SoapRaw.Length > 80 ? SoapRaw[..80] + "…" : SoapRaw;
            }
        }
    }

    // View model used when creating a new progress note
    public class ProgressNoteCreateViewModel
    {
        public int? InterventionId { get; set; }
        public int? PatientId { get; set; } // optional, can be derived from InterventionId
        public string? ClinicalStaffID { get; set; } = "";
        public DateTime NoteDate { get; set; } = DateTime.UtcNow;
        public string Subjective { get; set; } = "";
        public string Objective { get; set; } = "";
        public string Assessment { get; set; } = "";
        public string Plan { get; set; } = "";

        // server-side helper to combine into the pipe-delimited string used in the sample view
        public string ToPipeDelimited()
        {
            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(Subjective)) parts.Add($"S:{Subjective}");
            if (!string.IsNullOrWhiteSpace(Objective)) parts.Add($"O:{Objective}");
            if (!string.IsNullOrWhiteSpace(Assessment)) parts.Add($"A:{Assessment}");
            if (!string.IsNullOrWhiteSpace(Plan)) parts.Add($"P:{Plan}");
            return string.Join("|", parts);
        }
    }
    public class PatientActivityLogTabViewModel { }
}
