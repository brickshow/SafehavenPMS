using System;
using Microsoft.AspNetCore.Authorization;


namespace SafehavenPMS.ViewModel
{
[Authorize]
    public class PendingAssessmentViewModel
    {
        public int AssessmentId { get; set; }
        public int PatientId { get; set; }
        public string PatientName { get; set; }
        public int PhysicianId { get; set; }
        public string PhysicianName { get; set; }
        public string Type { get; set; }
        public DateTime? Date { get; set; }
        public string? Time { get; set; }
        public DateTime? CompletedDate { get; set; }
        public string Status { get; set; }

    }
}
