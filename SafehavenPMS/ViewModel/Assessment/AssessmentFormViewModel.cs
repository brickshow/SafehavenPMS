using SafehavenPMS.ViewModel.Assessment;
using SafehavenPMS.ViewModel.Assessment.SafehavenPMS.ViewModel.Assessment;

namespace SafehavenPMS.ViewModel
{
    public class AssessmentFormViewModel
    {
        // Patient Information
        public int AssessmentId { get; set; }
        public int? PatientId { get; set; }
        public string? FullName { get; set; }
        public int? Age { get; set; }
        public string? Sex { get; set; }
        public string? Occupation { get; set; }
        public string? Address { get; set; }

        // Assessment Sections
        public HistoryPresentViewModel? HistoryPresent { get; set; }
        public DrugUseHistoryViewModel? DrugUseHistory { get; set; }
        public MedicalHistoryViewModel? MedicalHistory { get; set; }
        public PhysicalExamViewModel? PhysicalExam { get; set; }

        public MentalStatusExaminationViewModel? MentalStatusExamination { get; set; }
        public DiagnosisViewModel? Diagnosis { get; set; }
        public ProblemListViewModel? ProblemList { get; set; }
        public RecommendationViewModel? Recommendation { get; set; }

        public AssessmentFormViewModel()
        {
            HistoryPresent = new HistoryPresentViewModel();
            DrugUseHistory = new DrugUseHistoryViewModel();
            MedicalHistory = new MedicalHistoryViewModel();
            PhysicalExam = new PhysicalExamViewModel();
            MentalStatusExamination = new MentalStatusExaminationViewModel();
            Diagnosis = new DiagnosisViewModel();
            ProblemList = new ProblemListViewModel();
            Recommendation = new RecommendationViewModel();

        }
    }
}