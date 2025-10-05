using Microsoft.AspNetCore.Authorization;
namespace SafehavenPMS.Models
{
    //Joined Entity Between Patient and Clinical Staff
[Authorize]
    public class ClinicalStaffPatient
    {
        //Many to many relation
        //Patient Foreign key
        public int PatientId { get; set; }
        public Patient Patient { get; set; }

        //Clinical Staff Foreign key
        public int ClinicalStaffId { get; set; }
        public ClinicalStaff ClinicalStaff { get; set; }
    }
}

