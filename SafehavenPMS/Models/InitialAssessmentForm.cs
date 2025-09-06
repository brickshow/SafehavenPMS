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
        public Recommendation? Recommendation { get; set; }
        public MentalStatusExamination? MentalStatusExamination { get; set; }


        // Audit fields
        public DateTime? CreatedAt { get; set; } = DateTime.Now;
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime CompletedAt { get; set; }
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

    public class Recommendation
    {
        public int RecommendationId { get; set; }
        public int InitialAssessmentFormId { get; set; }

        public string? ProgramType { get; set; }
        public string? ExpectedDuration { get; set; }
        public string? Reason { get; set; }

        // Navigation property
        public InitialAssessmentForm? InitialAssessmentForm { get; set; }

        // Audit fields
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }

    public class MentalStatusExamination
    {
        public int MentalStatusExaminationId { get; set; }

        // Foreign key to InitialAssessmentForm
        public int InitialAssessmentFormId { get; set; }
        public InitialAssessmentForm? InitialAssessmentForm { get; set; }

        // General Appearance
        public bool GeneralAppearanceNeat { get; set; }
        public bool GeneralAppearanceDishevelled { get; set; }
        public bool GeneralAppearanceInappropriate { get; set; }
        public string? GeneralAppearanceOthers { get; set; }

        // Speech
        public bool SpeechNormal { get; set; }
        public bool SpeechRapid { get; set; }
        public bool SpeechSlow { get; set; }
        public bool SpeechIncoherent { get; set; }
        public string? SpeechOthers { get; set; }

        // Behavior
        public bool BehaviorRelaxed { get; set; }
        public bool BehaviorCooperative { get; set; }
        public bool BehaviorSuspicious { get; set; }
        public bool BehaviorPreoccupied { get; set; }
        public string? BehaviorOthers { get; set; }

        // Signs of Impending Violence
        public bool ViolenceRelaxed { get; set; }
        public bool ViolenceRestless { get; set; }
        public bool ViolenceClenchedFist { get; set; }
        public bool ViolenceRaisedVoice { get; set; }
        public string? ViolenceOthers { get; set; }

        // Mood
        public bool MoodSad { get; set; }
        public bool MoodAnxious { get; set; }
        public bool MoodHappy { get; set; }
        public bool MoodFearful { get; set; }
        public bool MoodHelpless { get; set; }
        public bool MoodHopeless { get; set; }
        public bool MoodAngry { get; set; }
        public string? MoodOthers { get; set; }

        // Affect
        public bool AffectAppropriate { get; set; }
        public bool AffectInappropriate { get; set; }
        public bool AffectFlat { get; set; }
        public bool AffectBlunted { get; set; }
        public string? AffectOthers { get; set; }

        // Thoughts
        public bool ThoughtsNormal { get; set; }
        public bool ThoughtsFlightOfIdeas { get; set; }
        public bool ThoughtsPreoccupied { get; set; }
        public string? ThoughtsOthers { get; set; }

        // Cognition
        public bool CognitionConscious { get; set; }
        public bool CognitionConfused { get; set; }
        public bool CognitionDrowsy { get; set; }
        public string? CognitionOthers { get; set; }

        // Perceptions
        public bool PerceptionsIllusions { get; set; }
        public bool PerceptionsAuditoryHallucinations { get; set; }
        public bool PerceptionsVisualHallucinations { get; set; }
        public bool PerceptionsDelusions { get; set; }
        public bool PerceptionsParanoia { get; set; }
        public bool PerceptionsSuicidalAttempt { get; set; }
        public bool PerceptionsSuicidalIdeations { get; set; }
        public string? PerceptionsOthers { get; set; }

        // Memory Impairment
        public bool MemoryShortTerm { get; set; }
        public bool MemoryLongTerm { get; set; }
        public string? MemoryOthers { get; set; }

        // Orientation
        public bool OrientationOrientedToTime { get; set; }
        public bool OrientationOrientedToPerson { get; set; }
        public bool OrientationOrientedToPlace { get; set; }
        public bool OrientationDisorientedToTime { get; set; }
        public bool OrientationDisorientedToPerson { get; set; }
        public bool OrientationDisorientedToPlace { get; set; }
        public string? OrientationOthers { get; set; }

        // Judgement
        public bool JudgementGood { get; set; }
        public bool JudgementFair { get; set; }
        public bool JudgementPoor { get; set; }

        // Insight
        public bool InsightGood { get; set; }
        public bool InsightFair { get; set; }
        public bool InsightPoor { get; set; }

        // Audit fields (optional)
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }

}