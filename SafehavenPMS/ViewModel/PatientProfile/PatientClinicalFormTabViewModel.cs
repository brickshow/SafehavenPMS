using System;
using System.Collections.Generic;

namespace SafehavenPMS.ViewModel.PatientProfile
{
    public class ClinicalFormCardViewModel
    {
        public string FormType { get; set; } = "";
        public int? FormId { get; set; }
        public string Status { get; set; } = "In Progress";
        public DateTime? CreatedAt { get; set; }
        public string Clinician { get; set; } = "-";
        public string ActionUrl { get; set; } = "#";
        public bool Exists => FormId.HasValue;
        public bool IsArchived => Status == "Archived";
    }

    public class PatientClinicalFormTabViewModel
    {
        public int PatientId { get; set; }
        public List<ClinicalFormCardViewModel> Forms { get; set; } = new();
    }
}