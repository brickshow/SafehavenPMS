using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;


namespace SafehavenPMS.Models
{
[Authorize]
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
        public List<PsyProblemList> ProblemLists { get; set; } = new List<PsyProblemList>();
        public List<PsyDiagnosisList> DiagnosisLists { get; set; } = new List<PsyDiagnosisList>();
        public List<Goal> Goals { get; set; } = new List<Goal>();
    }

    public class PsyProblemList
    {
        public int PsyProblemListId { get; set; } // Primary Key
        public int PsychiatricAssessmentId { get; set; } // Foreign Key
        public string? Problem { get; set; }
        public string? Status { get; set; }

        //Audit fields
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }

        public DateTime? CreatedAt { get; set; }
        public string? CreatedBy { get; set; }



        // Navigation property
        public PsychiatricAssessment? PsychiatricAssessment { get; set; }
        public List<Goal> Goals { get; set; } = new List<Goal>();
        public List<MedicationOrder> MedicationOrders { get; set; } = new List<MedicationOrder>();
        public ICollection<Intervention> Interventions { get; set; } = new List<Intervention>();
    }

      public class PsyDiagnosisList
    {
        public int PsyDiagnosisListId { get; set; } // Primary Key
        public int PsychiatricAssessmentId { get; set; } // Foreign Key
        public string? Diagnosis { get; set; }

        // Navigation property
        public PsychiatricAssessment? PsychiatricAssessment { get; set; }

    }
}
