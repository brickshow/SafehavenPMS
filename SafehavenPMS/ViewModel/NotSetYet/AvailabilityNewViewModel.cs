namespace SafehavenPMS.ViewModel
{
    public class DayAvailabilityViewModel
    {
        public DayOfWeek Day { get; set; }

        // multiple slots for this day
        public List<AvailabilityViewModel> Slots { get; set; } = new();
    }
}
