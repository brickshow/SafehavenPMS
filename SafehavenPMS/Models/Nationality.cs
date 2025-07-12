namespace SafehavenPMS.Models
{
    public class Nationality
    {
        //Nationality Fields
        public int NationalityID { get; set; }
        public string NationalityName { get; set; }


        //Navigation Properties
        public ICollection<Patient> Patients { get; set; }// Collection of patients associated with this Model
    }
}
