namespace SafehavenPMS.ViewModel
{
    public class GetTimeSlotViewModel
    {
        public int AvailabilityId { get; set; }
        public int TimeSlotId { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }
}
