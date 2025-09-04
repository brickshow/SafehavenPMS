using SafehavenPMS.ViewModel.Assessment;

namespace SafehavenPMS.ViewModel
{
    public class AssessmentFormViewModel
    {
        // Patient Information
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
        public DiagnosisViewModel? Diagnosis { get; set; }
        public ProblemListViewModel? ProblemList { get; set; }
        public RecommendationViewModel? Recommendation { get; set; }

        public AssessmentFormViewModel()
        {
            HistoryPresent = new HistoryPresentViewModel();
            DrugUseHistory = new DrugUseHistoryViewModel();
            MedicalHistory = new MedicalHistoryViewModel();
            PhysicalExam = new PhysicalExamViewModel();
            Diagnosis = new DiagnosisViewModel();
            ProblemList = new ProblemListViewModel();
            Recommendation = new RecommendationViewModel();

        }
    }
}