using Microsoft.EntityFrameworkCore;
using SafehavenPMS.Data;
using SafehavenPMS.Models;
using SafehavenPMS.Enum;

namespace SafehavenPMS.Services
{
    public class MedicationResetService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<MedicationResetService> _logger;

        public MedicationResetService(IServiceProvider serviceProvider, ILogger<MedicationResetService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var now = DateTime.Now;
                    var nextMidnight = now.Date.AddDays(1);
                    var delay = nextMidnight - now;

                    _logger.LogInformation($"MedicationResetService: Next reset scheduled at {nextMidnight:yyyy-MM-dd HH:mm:ss} (in {delay.TotalHours:F2} hours)");

                    // Wait until midnight
                    await Task.Delay(delay, stoppingToken);

                    if (!stoppingToken.IsCancellationRequested)
                    {
                        await ResetMedicationLogs();
                    }
                }
                catch (OperationCanceledException)
                {
                    // Expected when cancellation is requested
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in MedicationResetService");
                    // Wait 1 hour before retrying on error
                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                }
            }
        }

        private async Task ResetMedicationLogs()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<SafehavenPMSContext>();

            try
            {
                _logger.LogInformation("Starting medication administration log reset at midnight");

                var today = DateTime.Today;
                var tomorrow = today.AddDays(1);

                _logger.LogInformation($"Querying for medication orders. Today: {today:yyyy-MM-dd}, Tomorrow: {tomorrow:yyyy-MM-dd}");

                // First, check all medication orders to see what we have
                var allOrdersCount = await context.MedicationOrders.CountAsync();
                _logger.LogInformation($"Total medication orders in database: {allOrdersCount}");

                // Get all medication orders (without status filter first)
                var allOrders = await context.MedicationOrders
                    .Include(mo => mo.Patient)
                    .Include(mo => mo.Medicine)
                    .ToListAsync();

                _logger.LogInformation($"Found {allOrders.Count} medication orders");

                // Filter manually to see what's happening
                var activeMedicationOrders = allOrders
                    .Where(mo => mo.StartDate.Date <= tomorrow)
                    .Where(mo => mo.NoDiscontinueDate || 
                                 (mo.DiscontinueDate.HasValue && mo.DiscontinueDate.Value.Date >= tomorrow))
                    .Where(mo => mo.Status == MedicationOrderStatus.Active.ToString() ||
                                 mo.Status == MedicationOrderStatus.InProgress.ToString() ||
                                 mo.Status == MedicationOrderStatus.NotStarted.ToString())
                    .ToList();

                _logger.LogInformation($"Found {activeMedicationOrders.Count} active medication orders after filtering");

                if (activeMedicationOrders.Any())
                {
                    foreach (var order in activeMedicationOrders)
                    {
                        _logger.LogInformation($"Order {order.MedicationOrderId}: Status={order.Status}, StartDate={order.StartDate:yyyy-MM-dd}, NoDiscontinue={order.NoDiscontinueDate}");
                    }
                }

                var newAdministrationLogs = new List<AdministrationLog>();
                var notificationsToAdd = new List<Notification>();
                var activitiesToAdd = new List<ActivityLog>();

                foreach (var order in activeMedicationOrders)
                {
                    // Check if this order applies for tomorrow (handles NonDaily scheduling)
                    if (!IsOrderActiveForDate(order, tomorrow))
                        continue;

                    // Check if administration log already exists for tomorrow
                    var existingLog = await context.AdministrationLogs
                        .FirstOrDefaultAsync(al => al.MedicationOrderId == order.MedicationOrderId &&
                                                   al.AdministrationDate.Date == tomorrow);

                    if (existingLog == null)
                    {
                        // Create new administration log for tomorrow
                        var newLog = new AdministrationLog
                        {
                            MedicationOrderId = order.MedicationOrderId,
                            PatientId = order.PatientId,
                            AdministrationDate = tomorrow,
                            BreakfastTaken = false,
                            LunchTaken = false,
                            DinnerTaken = false,
                            BedtimeTaken = false,
                            RecordedBy = "System",
                            CreatedAt = DateTime.Now
                        };

                        newAdministrationLogs.Add(newLog);

                        // Create notification for staff about new medication schedule
                        var medicineName = order.Medicine?.GenericName ?? order.Medicine?.BrandName ?? "Medication";
                        var patientName = $"{order.Patient?.Firstname} {order.Patient?.Lastname}";
                        
                        notificationsToAdd.Add(new Notification
                        {
                            UserName = "System",
                            Message = $"New medication schedule ready for {patientName}: {medicineName}",
                            Type = "Info",
                            IsRead = false,
                            CreatedAt = DateTime.Now,
                            LinkUrl = $"/Medication/Index"
                        });

                        // Create activity log
                        activitiesToAdd.Add(new ActivityLog
                        {
                            PatientId = order.PatientId,
                            UserName = "System",
                            Action = "MedicationScheduleCreated",
                            Description = $"New medication schedule created for {patientName}: {medicineName}",
                            Category = "Clinical",
                            Severity = "Info",
                            CreatedAt = DateTime.Now
                        });
                    }
                }

                // Save all new administration logs
                if (newAdministrationLogs.Any())
                {
                    await context.AdministrationLogs.AddRangeAsync(newAdministrationLogs);
                    _logger.LogInformation($"Created {newAdministrationLogs.Count} new administration logs for tomorrow");
                }

                // Save notifications and activity logs
                if (notificationsToAdd.Any())
                {
                    await context.Notifications.AddRangeAsync(notificationsToAdd);
                }

                if (activitiesToAdd.Any())
                {
                    await context.ActivityLogs.AddRangeAsync(activitiesToAdd);
                }

                await context.SaveChangesAsync();

                _logger.LogInformation($"Medication administration log reset completed successfully. Created {newAdministrationLogs.Count} new logs for {tomorrow:yyyy-MM-dd}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during medication administration log reset");
                throw;
            }
        }

        private bool IsOrderActiveForDate(MedicationOrder order, DateTime date)
        {
            // Check if order starts on or before the target date
            if (order.StartDate.Date > date)
                return false;

            // Check if order is discontinued before the target date
            if (!order.NoDiscontinueDate && order.DiscontinueDate.HasValue && order.DiscontinueDate.Value.Date < date)
                return false;

            // Handle NonDaily scheduling
            if (order.ScheduledType == "NonDaily" && order.DaysInterval.HasValue && order.DaysInterval.Value > 0)
            {
                var daysSinceStart = (date - order.StartDate.Date).Days;
                return daysSinceStart % order.DaysInterval.Value == 0;
            }

            return true;
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("MedicationResetService is stopping");
            await base.StopAsync(cancellationToken);
        }
    }
}
