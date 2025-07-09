namespace SafehavenPMS.Models
{
    public class EducationLevel
    {
        public int EducationLevelId { get; set; }
        public string EducationLevelName { get; set; } // e.g., Primary, Secondary, Tertiary, etc.

        //Navigation Properties
        public ICollection<Patient> Patients { get; set; } // Collection of patients associated with this marital status
    }
}
