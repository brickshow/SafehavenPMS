namespace SafehavenPMS.Models
{
    public class PatientCase
    {
        public int PatientCaseId { get; set; }
        public int PatientId { get; set; }
        public string CaseNumber { get; set; }
        public DateTime DateOfIntake { get; set; }
        public DateTime? DateOfReferral { get; set; }
        public string ReferredBy { get; set; } 
        public string AccompaniedBy { get; set; } 
        public string Affilation { get; set; } 
        public int StaffId { get; set; }// ID of the staff member handling the case
        public string CaseStatus { get; set; } // e.g., Active, Closed

        //Navigation Properties
        //One -to-Many Relationship
        public Patient Patient { get; set; } // Navigation property to the associated patient
       
    }
}
