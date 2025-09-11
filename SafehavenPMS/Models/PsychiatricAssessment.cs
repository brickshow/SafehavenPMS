using System;
using System.ComponentModel.DataAnnotations;

namespace SafehavenPMS.Models
{
    public class PsychiatricAssessment
    {
        public int PsychiatricAssessmentId { get; set; } // Primary Key

        public int PatientId { get; set; }
        public string? Type { get; set; }
        public DateTime? Date { get; set; }
        public string? Time { get; set; }
        public DateTime? CompletedDate { get; set; }
        public string? Status { get; set; }

        // Tab fields
        public string? ChiefComplaint { get; set; }
        public string? HistoryOfPresentIllness { get; set; }
        public string? PersonalAndFamilyHistory { get; set; }
        public string? MentalStatusExamination { get; set; }
        public string? Impression { get; set; }

        //Audit Fields
        public DateTime? CreatedAt { get; set; } = DateTime.Now;
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }

        // Navigation property
        public Patient? Patient { get; set; }
    }
}