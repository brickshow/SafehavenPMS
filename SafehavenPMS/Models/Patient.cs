namespace SafehavenPMS.Models
{
    public class Patient
    {
        //Personal Information
        public int PatientId { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string MiddleName { get; set; }
        public string ContactNumber { get; set; }
        public string Religion { get; set; }
        public string Sex { get; set; }
        public int MaritalStatusID { get; set; }// Assuming maritalStatusID is an integer that references a MaritalStatus entity
        public string DateOfBirth { get; set; }
        public string PatienStatus { get; set; }
        public string Address { get; set; }
        public string Occupation { get; set; }
        public int EducationLevelID { get; set; }// Assuming EducationLevelID is an integer that references an EducationLevel entity
        public string PhotoUrl { get; set; }


        //Navigation Properties
        //One -to-Many Relationships
        public MaritalStatus MaritalStatus { get; set; } // Navigation property to the associated marital status
        //One-to-May Relationship
        public EducationLevel EducationLevel { get; set; } // Navigation property to the associated education level

        //Many-to-one Relationships
        public ICollection<PatientCase> PatientCases { get; set; } // Collection of patient cases associated with this patient
    }
}
