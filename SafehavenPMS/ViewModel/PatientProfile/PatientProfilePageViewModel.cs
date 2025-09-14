using System;
using System.Collections.Generic;
using SafehavenPMS.ViewModel.PatientProfile;

namespace SafehavenPMS.ViewModel
{
    public class PatientProfilePageViewModel
    {
        // Basic patient header info
        public int PatientId { get; set; }
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
    public class PatientTreatmentPlanTabViewModel { }
    public class PatientProgressNotesTabViewModel { }
    public class PatientActivityLogTabViewModel { }
}