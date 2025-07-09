namespace SafehavenPMS.Models
{
    public class MaritalStatus
    {
        public int MaritalStatusId { get; set; } // Unique identifier for the marital status
        public string MaritalStatusType { get; set; } // e.g., Single, Married, Divorced, Widowed, etc.
        

        //Navigation Proterties
        public ICollection<Patient> Patients { get; set; } // Collection of patients associated with this marital status
    }
}
