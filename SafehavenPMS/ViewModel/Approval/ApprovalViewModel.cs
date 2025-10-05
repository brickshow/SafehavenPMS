using SafehavenPMS.Enum;
using SafehavenPMS.Models;
using Microsoft.AspNetCore.Authorization;


namespace SafehavenPMS.ViewModel.Approval
{
[Authorize]
    public class ApprovalViewModel
    {
        public int PatientId { get; set; }
        public Patient? Patient { get; set; }
        //Attributes to be displayed in the view
        public string? PatientName { get; set; }
        public DateTime? CompletedAt { get; set; }
        public bool? IsDrugDependent { get; set; }
        public string? Diagnosis { get; set; }
        public string? Recommendation { get; set; }
        public string? Status { get; set; }

        //Audit Fields
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
