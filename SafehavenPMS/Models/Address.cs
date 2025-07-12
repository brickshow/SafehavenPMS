namespace SafehavenPMS.Models
{
    public class Address
    {
        //Address Fields
        public int AddressID { get; set; }
        public string Street { get; set; }
        public string Barangay { get; set; }
        public string City { get; set; }
        public string Province { get; set; }
        public string Country { get; set; }
        public string PostalCode { get; set; }


        public ICollection<Patient> Patients { get; set; }// Collection of patients associated with this Model
    }
}
