using System;

namespace SafehavenPMS.ViewModel
{
    public class ProgressNoteEditViewModel
    {
        public int ProgressNoteId { get; set; }
        public int PatientId { get; set; }
        public int? InterventionId { get; set; }

        // Either edit full raw SOAP or the separate fields below.
        public string? SoapRaw { get; set; }

        public string? Subjective { get; set; }
        public string? Objective { get; set; }
        public string? Assessment { get; set; }
        public string? Plan { get; set; }

        public DateTime? CreatedAt { get; set; }
    }
}