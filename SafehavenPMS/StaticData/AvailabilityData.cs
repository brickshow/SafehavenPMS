using SafehavenPMS.Enum;
using SafehavenPMS.Models;

namespace SafehavenPMS.StaticData
{
    public static class AvailabilityGenerator
    {
        public static List<Availability> GenerateForDoctor(int clinicalStaffId)
        {
            var availabilities = new List<Availability>();

            // Generate slots Monday → Saturday
            for (DayOfWeek day = DayOfWeek.Monday; day <= DayOfWeek.Saturday; day++)
            {
                for (int hour = 8; hour < 17; hour++) // 8 AM → 5 PM
                {
                    if (hour == 12) continue; // Skip 12 PM → 1 PM lunch

                    availabilities.Add(new Availability
                    {
                        ClinicalStaffID = clinicalStaffId,
                        Day = day,
                        StartTime = new TimeSpan(hour, 0, 0),
                        EndTime = new TimeSpan(hour + 1, 0, 0),
                        Status = AvailabilityStatus.Available.ToString()
                    });
                }
            }

            return availabilities;
        }
    }
}
