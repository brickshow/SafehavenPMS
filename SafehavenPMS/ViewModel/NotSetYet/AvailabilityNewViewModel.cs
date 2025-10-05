using Microsoft.AspNetCore.Authorization;
namespace SafehavenPMS.ViewModel
{
[Authorize]
    public class DayAvailabilityViewModel
    {
        public DayOfWeek Day { get; set; }

        // multiple slots for this day
        public List<AvailabilityViewModel> Slots { get; set; } = new();
    }
}

