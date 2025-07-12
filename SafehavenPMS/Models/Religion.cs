namespace SafehavenPMS.Models
{
    public class Religion
    {
        //Add Religion fields
        public int ReligionID { get; set; }
        public string ReligionName { get; set; }

        public ICollection<Patient> Patients { get; set; }// Collection of patients associated with this Model
    }
}
