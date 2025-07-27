namespace SafehavenPMS.Models
{
    public class Patient
    {
        //Personal Information
        public int PatientId { get; set; }

        //Foreign Keys
        public int AddressID { get; set; }//Foreign key from Address
        public int ClinicalStaffID { get; set; }//Foreign key from Address
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string MiddleName { get; set; }
        public string PhoneNumber { get; set; }
        public string Sex { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string PatientStatus { get; set; }
        public string Occupation { get; set; }
        public string Education { get; set; }
        public string Religion { get; set; }
        public string MaritalStatus { get; set; }
        public DateTime DateOfReferral { get; set; }
        public string ReferredBy { get; set; }
        public string Affiliation { get; set; }
        public string PhotoUrl { get; set; }


        //Navigation Properties
        //One -to-Many Relationships
        public Address Address { get; set; }
        public IEnumerable<Models.ClinicalStaff> ClinicalStaffs { get; set; }
    }
}
