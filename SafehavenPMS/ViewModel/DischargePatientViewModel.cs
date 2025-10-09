using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace SafehavenPMS.ViewModel
{
    public class DischargePatientViewModel
    {
        public int PatientId { get; set; }
        public string? PatientNumber { get; set; }
        public string? PatientName { get; set; }
        public string? PhotoUrl { get; set; }
        public int? Age { get; set; }
        public string? Sex { get; set; }
        public string? Address { get; set; }

        [Display(Name = "Admission Date")]
        [DataType(DataType.Date)]
        public DateTime AdmissionDate { get; set; }

        [Display(Name = "Discharge Date")]
        [DataType(DataType.Date)]
        public DateTime DischargeDate { get; set; }

        [Required]
        [Display(Name = "Reason for discharge")]
        public string? Reason { get; set; }

        [Display(Name = "Discharge Notes")]
        public string? Notes { get; set; }

        public bool HasUnpaidInvoices { get; set; }
        public bool ProceedAnyway { get; set; }

        public IEnumerable<SelectListItem>? PatientOptions { get; set; }
    }
}