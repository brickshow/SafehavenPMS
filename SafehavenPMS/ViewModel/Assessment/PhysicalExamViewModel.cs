namespace SafehavenPMS.ViewModel.Assessment
{
    public class PhysicalExamViewModel
    {
        // Vital Signs
        // Blood pressure measurement in mmHg
        public string? BP { get; set; }

        // Heart rate measurement in beats per minute
        public string? HR { get; set; }

        // Respiratory rate measurement in cycles per minute
        public string? RR { get; set; }

        // Body temperature measurement in Celsius
        public string? Temperature { get; set; }

        // Oxygen saturation measurement in percentage
        public string? O2 { get; set; }

        // System Examination - Skin
        public bool SkinNormal { get; set; }
        public string? SkinFindings { get; set; }

        // System Examination - ENT (Ear, Nose, Throat)
        public bool ENTNormal { get; set; }
        public string? ENTFindings { get; set; }

        // System Examination - Chest
        public bool ChestNormal { get; set; }
        public string? ChestFindings { get; set; }

        // System Examination - Lungs
        public bool LungsNormal { get; set; }
        public string? LungsFindings { get; set; }

        // System Examination - CVS (Cardiovascular System)
        public bool CVSNormal { get; set; }
        public string? CVSFindings { get; set; }

        // System Examination - Abdomen
        public bool AbdomenNormal { get; set; }
        public string? AbdomenFindings { get; set; }

        // System Examination - GUT (Genitourinary Tract)
        public bool GUTNormal { get; set; }
        public string? GUTFindings { get; set; }

        // System Examination - Extremities
        public bool ExtremitiesNormal { get; set; }
        public string? ExtremitiesFindings { get; set; }
    }
}
