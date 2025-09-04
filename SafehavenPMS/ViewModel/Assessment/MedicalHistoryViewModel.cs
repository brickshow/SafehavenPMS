namespace SafehavenPMS.ViewModel.Assessment
{
    public class MedicalHistoryViewModel
    {
        public bool IsHypertensive { get; set; }
        public bool IsDiabetic { get; set; }
        public bool IsAsthmatic { get; set; }
        public string? OtherConditions { get; set; }

        public bool MaternalHypertension { get; set; }
        public bool MaternalDiabetic { get; set; }
        public bool MaternalNone { get; set; }

        public bool PaternalHypertension { get; set; }
        public bool PaternalDiabetic { get; set; }
        public bool PaternalNone { get; set; }

        public List<string> FoodAllergies { get; set; } = new();
        public List<string> DrugAllergies { get; set; } = new();
        public List<SurgicalOperation> SurgicalOperations { get; set; } = new();
    }

    public class SurgicalOperation
    {
        public string? Year { get; set; }
        public string? Duration { get; set; }
        public string? Hospital { get; set; }
        public string? Operation { get; set; }
    }
}