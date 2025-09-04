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
        public PhysicalExam? PhysicalExam { get; set; }
        public Diagnosis? Diagnosis { get; set; }
        public ICollection<ProblemList>? Problems { get; set; }

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

    public class PhysicalExam
    {
        public int PhysicalExamId { get; set; }
        public int InitialAssessmentFormId { get; set; }

        // Vital Signs
        public string? BP { get; set; }  // Blood pressure in mmHg
        public string? HR { get; set; }  // Heart rate in beats per minute
        public string? RR { get; set; }  // Respiratory rate in cycles per minute
        public string? Temperature { get; set; }  // Body temperature in Celsius
        public string? O2 { get; set; }  // Oxygen saturation in percentage

        // System Examination
        public bool SkinNormal { get; set; }
        public string? SkinFindings { get; set; }

        public bool ENTNormal { get; set; }
        public string? ENTFindings { get; set; }

        public bool ChestNormal { get; set; }
        public string? ChestFindings { get; set; }

        public bool LungsNormal { get; set; }
        public string? LungsFindings { get; set; }

        public bool CVSNormal { get; set; }
        public string? CVSFindings { get; set; }

        public bool AbdomenNormal { get; set; }
        public string? AbdomenFindings { get; set; }

        public bool GUTNormal { get; set; }
        public string? GUTFindings { get; set; }

        public bool ExtremitiesNormal { get; set; }
        public string? ExtremitiesFindings { get; set; }

        // Navigation property
        public InitialAssessmentForm? InitialAssessmentForm { get; set; }

        // Audit fields
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }

    public class Diagnosis
    {
        public Diagnosis()
        {
            SubstanceUseEntries = new List<SubstanceUseEntry>();
            CreatedAt = DateTime.Now;
        }

        public int DiagnosisId { get; set; }
        public int InitialAssessmentFormId { get; set; }

        // Collection of substance use entries
        public ICollection<SubstanceUseEntry> SubstanceUseEntries { get; set; }

        // Navigation property
        public InitialAssessmentForm? InitialAssessmentForm { get; set; }

        // Audit fields
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }

    public class SubstanceUseEntry
    {
        public SubstanceUseEntry()
        {
            SubstanceName = string.Empty;
            Severity = string.Empty;
            CreatedAt = DateTime.Now;
        }

        public int SubstanceUseEntryId { get; set; }
        public int DiagnosisId { get; set; }

        // Substance Use Details
        public string SubstanceName { get; set; }
        public string Severity { get; set; }  // Mild, Moderate, Severe

        // Navigation property
        public Diagnosis? Diagnosis { get; set; }

        // Audit fields
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }

    public class ProblemList
{
    public int ProblemListId { get; set; }
    public int InitialAssessmentFormId { get; set; }

    // Problem details
    public string? Problem { get; set; }

    // Navigation property
    public InitialAssessmentForm? InitialAssessmentForm { get; set; }

    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}
}