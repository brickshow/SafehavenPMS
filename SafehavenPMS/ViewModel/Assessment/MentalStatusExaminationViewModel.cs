using Microsoft.AspNetCore.Authorization;
namespace SafehavenPMS.ViewModel.Assessment
{
[Authorize]
    public class MentalStatusExaminationViewModel
    {
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
    }
}
