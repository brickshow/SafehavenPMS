namespace SafehavenPMS.Models
{
    public class ClinicalStaff
    {
        //Personal Information
        public int ClinicalStaffID { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string? MiddleName { get; set; }//Accept null
        public string Sex { get; set; }
        public string PhoneNumber { get; set; }
        public string Position { get; set; }
        public string AsignPatient { get; set; }
        public string ProfilePictureURL { get; set; }
        public string PRC_Licensed { get; set; }

        //User Account information
        public string UserID { get; set; }
        public string Email { get; set; }
        public DateTime HireDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }

    } 
}
