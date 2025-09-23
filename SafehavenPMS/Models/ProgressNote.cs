namespace SafehavenPMS.Models
{
    public class ProgressNote
    {
        public int ProgressNoteId { get; set; }

        // Required to query notes for a patient
        public int? PatientId { get; set; }

        // Optional: link to an intervention
        public int? InterventionId { get; set; }

        // Who recorded the note
        public string? Clinician { get; set; }

        // Timestamp for the note
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Raw/combined SOAP or full text
        public string? SoapRaw { get; set; }

        // Optional structured SOAP fields
        public string? Subjective { get; set; }
        public string? Objective { get; set; }
        public string? Assessment { get; set; }
        public string? Plan { get; set; }

        // Navigation properties
        public Patient? Patient { get; set; }
        public Intervention? Intervention { get; set; }
    }
}