namespace SafehavenPMS.Models
{
    public class InitialAssessmentForm
    {
        public int InitialAssessmentFormId { get; set; }
        public int PatientId { get; set; }

        // Navigation properties
        public Patient? Patient { get; set; }
        public HistoryPresent? HistoryPresent { get; set; }
        public ICollection<DrugUse>? DrugUses { get; set; }
        public MedicalHistory? MedicalHistory { get; set; }
        public ICollection<MedicalAllergy>? MedicalAllergies { get; set; }
        public ICollection<SurgicalHistory>? SurgicalHistories { get; set; }

        // Audit fields
        public DateTime? CreatedAt { get; set; } = DateTime.Now;
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }

    public class HistoryPresent
    {
        public int HistoryPresentId { get; set; }
        public int InitialAssessmentFormId { get; set; }

        // Medical History
        public string? OnsetOfDrugUse { get; set; }
        public string? ReasonForFirstUse { get; set; }
        public string? HistoryOfImprisonment { get; set; }
        public string? PreviousDrugRehab { get; set; } // Added missing field
        public string? WhoInvitedFirstUse { get; set; }
        public int? NumberOfPeopleFirstUse { get; set; }
        public string? LastUseOfSubstance { get; set; }
        public string? AmountConsumedFirstUse { get; set; } // Changed to string to allow units

        public string? Status { get; set; } // New field for status
        // Navigation property
        public InitialAssessmentForm? InitialAssessmentForm { get; set; }

        // Audit fields
        public DateTime? CreatedAt { get; set; } = DateTime.Now;
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }
    
     public class DrugUse
    {
        public int DrugUseId { get; set; }
        public int InitialAssessmentFormId { get; set; }

        // Drug use details
        public string? SubstanceName { get; set; }
        public string? Route { get; set; }
        public string? QuantityPerDay { get; set; }
        public string? Frequency { get; set; }
        public string? FirstUse { get; set; }
        public string? EffectsWhenHigh { get; set; }
        public string? EffectsWhenWanes { get; set; }

        // Navigation property
        public InitialAssessmentForm? InitialAssessmentForm { get; set; }

        // Audit fields
        public DateTime? CreatedAt { get; set; } = DateTime.Now;
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }

    public class MedicalHistory
    {
        public int MedicalHistoryId { get; set; }
        public int InitialAssessmentFormId { get; set; }

        // Medical conditions
        public bool IsHypertensive { get; set; }
        public bool IsDiabetic { get; set; }
        public bool IsAsthmatic { get; set; }
        public string? OtherConditions { get; set; }

        // Heredofamilial diseases
        public bool MaternalHypertension { get; set; }
        public bool MaternalDiabetic { get; set; }
        public bool MaternalNone { get; set; }

        public bool PaternalHypertension { get; set; }
        public bool PaternalDiabetic { get; set; }
        public bool PaternalNone { get; set; }

        // Navigation property
        public InitialAssessmentForm? InitialAssessmentForm { get; set; }

        // Audit fields
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }

    public class MedicalAllergy
    {
        public int MedicalAllergyId { get; set; }
        public int InitialAssessmentFormId { get; set; }
        
        public string? AllergyType { get; set; } // "Food" or "Drug"
        public string? AllergyName { get; set; }
        
        // Navigation property
        public InitialAssessmentForm? InitialAssessmentForm { get; set; }

        // Audit fields
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }

    public class SurgicalHistory
    {
        public int SurgicalHistoryId { get; set; }
        public int InitialAssessmentFormId { get; set; }

        public string? Year { get; set; }
        public string? Duration { get; set; }
        public string? Hospital { get; set; }
        public string? Operation { get; set; }

        // Navigation property
        public InitialAssessmentForm? InitialAssessmentForm { get; set; }

        // Audit fields
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }
}