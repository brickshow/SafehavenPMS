namespace SafehavenPMS.Models
{
    public class Patient
    {
        //Personal Information
        public int PatientId { get; set; }

        //Foreign Keys
        public int EducationLevelID { get; set; }// Assuming EducationLevelID is an integer that references an EducationLevel entity
        public int ReligionID { get; set; } //Foreign key from Religion Table
        public int MaritalStatusID { get; set; }// Assuming maritalStatusID is an integer that references a MaritalStatus entity
        public int AddressID { get; set; }//Foreign key from Address
        public int NationalityID { get; set; } //Foreign key from Nationality


        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string MiddleName { get; set; }
        public string ContactNumber { get; set; }
        public string Sex { get; set; }
        public string DateOfBirth { get; set; }
        public string PatienStatus { get; set; }
        public string Occupation { get; set; }
        public string PhotoUrl { get; set; }


        //Navigation Properties
        //One -to-Many Relationships
        public Religion Religion { get; set; }// Navigation property to the associated Religion
        public Nationality Nationality { get; set; }
        public Address Address { get; set; }
        public MaritalStatus MaritalStatus { get; set; } // Navigation property to the associated marital status
        public EducationLevel EducationLevel { get; set; } // Navigation property to the associated education level
        public ICollection<PatientCase> PatientCases { get; set; } // Collection of patient cases associated with this patient
    }
}
