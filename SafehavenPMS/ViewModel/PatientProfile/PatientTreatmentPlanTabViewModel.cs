using System;
using System.Collections.Generic;

namespace SafehavenPMS.ViewModel.PatientProfile
{
    public class PatientTreatmentPlanTabViewModel
    {
        public List<ProblemViewModel> Problems { get; set; } = new List<ProblemViewModel>();
    }

    public class ProblemViewModel
    {
        public int? InitialAssessmentFormId { get; set; }
        public int PsyProblemListId { get; set; }
        public string? Problems { get; set; }
        public string? Status { get; set; }
        public List<GoalViewModel> Goals { get; set; } = new List<GoalViewModel>();
        public List<InterventionViewModel> Interventions { get; set; } = new List<InterventionViewModel>();
    }

    public class GoalViewModel
    {
        public int GoalId { get; set; }
        public string? Description { get; set; }
        public string? Status { get; set; } // e.g. In Progress, Completed, Discontinued
        public string? NotedBy { get; set; }
        public DateTime? TargetDate { get; set; }
    }

    public class InterventionViewModel
    {
        public int InterventionId { get; set; }
        public string? ServiceTypeName { get; set; }
        public string? ServiceModalityName { get; set; }
        public string? Description { get; set; }
        public string? Frequency { get; set; }
        public string? Status { get; set; }
        public string? NotedBy { get; set; }
        public DateTime? DateAdded { get; set; }

        // Added fields for medication display
        public int? MedicationOrderId { get; set; }
        public int? MedicineId { get; set; }
        public string? ScheduledType { get; set; }
        
        public string? MedicationName { get; set; }
        public string? UnitPerDose { get; set; }
    }
}