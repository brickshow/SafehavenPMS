namespace SafehavenPMS.Models
{
    public class Address
    {
        //Address Fields
        public int AddressID { get; set; }
        public string House_Unit { get; set; }
        public string Street { get; set; }
        public string Subdivision_Village { get; set; }
        public string Barangay { get; set; }
        public string City { get; set; }
        public string Province { get; set; }


        public ICollection<Patient> Patients { get; set; }// Collection of patients associated with this Model
        public ICollection<ClinicalStaff> ClinicalStaffs { get; set; }// Collection of Clinical Staff associated with this Model
    }
}
