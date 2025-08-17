using Microsoft.EntityFrameworkCore;
using SafehavenPMS.Controllers;
using SafehavenPMS.Data;

namespace SafehavenPMS.Services
{
    // This class runs in the background (separate from controllers).
    // Its job is to check all "Confirmed" appointments and see
    // if their end time has already passed. If so, mark them as "Done".
    public class AppointmentStatusUpdater : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        // Constructor - gets access to IServiceScopeFactory so we can
        // create a new database context every time the job runs.
        public AppointmentStatusUpdater(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        // This is the main loop that runs continuously in the background.
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Keep looping until the app shuts down
            while (!stoppingToken.IsCancellationRequested)
            {
                // Create a scoped service provider so we can get a DbContext safely
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<SafehavenPMSContext>();

                // Get the current date and time
                var now = DateTime.Now;

                // Get all appointments that are still "Confirmed"
                var expiredAppointments = await context.Appointments
                    .Include(a => a.TimeSlot) // Load related time slot info
                    .Include(a => a.Day)      // Load related day info
                    .Where(a => a.Status == Enum.AppointmentEnum.Confirmed.ToString())
                    .ToListAsync();

                // Go through each confirmed appointment
                foreach (var appt in expiredAppointments)
                {
                    // Calculate the actual end date & time of this appointment:
                    // 1. Convert "DayName" (e.g. Monday) into a real calendar date
                    // 2. Add the EndTime of the timeslot
                    var endDateTime = AppointmentController.NextDayOfWeek(
                        System.Enum.Parse<DayOfWeek>(appt.Day.DayName)
                    ).Date + appt.TimeSlot.EndTime;

                    // If the current time has passed the appointment's end time
                    if (now > endDateTime)
                    {
                        // Mark the appointment as "Done"
                        appt.Status = Enum.AppointmentEnum.Done.ToString();
                    }
                }

                // Save any updates back to the database
                await context.SaveChangesAsync();

                // Wait for 1 hour before checking again
                // (You can change this to minutes if you want it to run more often)
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }
}
