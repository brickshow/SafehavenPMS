using SafehavenPMS.ViewModel;

namespace SafehavenPMS.ViewModel
{
    public class PsychiatricAssessmentViewModel
    {
        public int PsychiatricAssessmentId { get; set; } // Primary Key

        //Tabs
        public string? ChiefComplaint { get; set; }
        public string? HistoryOfPresentIllness { get; set; }
        public string? PersonalAndFamilyHistory { get; set; }
        public string? MentalStatusExamination { get; set; }
        public string? Impression { get; set; }
        public string? Diagnosis { get; set; }

        public int PatientId { get; set; }

        // Patient Info
        public string? FullName { get; set; }
        public int? Age { get; set; }
        public string? Sex { get; set; }
        public string? Occupation { get; set; }
        public string? Address { get; set; }

        public string? Type { get; set; }
        public DateTime? Date { get; set; }
        public DateTime? Time { get; set; }
        public DateTime? CompletedDate { get; set; }
        public string? Status { get; set; }

        // Problem list used by the Diagnosis partial
        public PsyProblemListViewModel ProblemList { get; set; } = new PsyProblemListViewModel();
        public PsyDiagnosisListViewModel PsyDiagnosisList { get; set; } = new PsyDiagnosisListViewModel();
    }

    public class PsyProblemListViewModel
    {
        public List<string> Problems { get; set; } = new List<string>();
    }

    public class PsyDiagnosisListViewModel
    {
        public List<string> Diagnosis { get; set; } = new List<string>();
    }
}
