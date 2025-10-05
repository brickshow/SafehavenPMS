using Microsoft.AspNetCore.Authorization;
namespace SafehavenPMS.ViewModel
{
[Authorize]
    public class ClinicalStaffAvailability
    {
        public int Id { get; set; }
        public int ClinicalStaffID { get; set; }
        public string DayName { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string? Title { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}

