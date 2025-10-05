// ...new file...
using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;


namespace SafehavenPMS.Models
{
[Authorize]
    public class DischargedPatient
    {
        //Primary Key
        [Key]
        public int DischargeId { get; set; }
        public int PatientId { get; set; }            // FK to admission/patient
        public Patient? Patient { get; set; }
        public string? ProgramType { get; set; }
        public string? Reason { get; set; }
        public string? Status { get; set; }           // e.g. Pending, Completed, Cancelled
        public string? CreatedBy { get; set; }         // user who initiated transfer
        public DateTime? DischargeDate { get; set; }   // date of transfer if
    }
}
