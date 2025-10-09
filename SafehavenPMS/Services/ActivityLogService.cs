using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SafehavenPMS.Data;
using SafehavenPMS.Models;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;

namespace SafehavenPMS.Services
{
    public class ActivityLogService
    {
        private readonly SafehavenPMSContext _ctx;
        public ActivityLogService(SafehavenPMSContext ctx) => _ctx = ctx;

        public async Task LogAsync(string user, string action, string? desc = null,
            string category = "General", string severity = "Info", int? patientId = null)
        {
            _ctx.ActivityLogs.Add(new ActivityLog
            {
                UserName = string.IsNullOrWhiteSpace(user) ? "System" : user,
                Action = action,
                Description = desc,
                Category = category,
                Severity = severity,
                PatientId = patientId
            });
            await _ctx.SaveChangesAsync();
        }

        public async Task<List<ActivityLog>> GetPatientLogsAsync(int patientId, int page = 1, int pageSize = 25,
            string? search = null, string? category = null)
        {
            var q = _ctx.ActivityLogs.Where(l => l.PatientId == patientId);
            if (!string.IsNullOrWhiteSpace(search))
                q = q.Where(l => l.Action.Contains(search) || l.Description!.Contains(search));
            if (!string.IsNullOrWhiteSpace(category))
                q = q.Where(l => l.Category == category);
            return await q.OrderByDescending(l => l.CreatedAt)
                          .Skip((page - 1) * pageSize)
                          .Take(pageSize)
                          .ToListAsync();
        }

        // Notifications
        public async Task<int> NotifyAsync(string user, string message, string type = "Info", string? link = null)
        {
            var n = new Notification
            {
                UserName = user,
                Message = message,
                Type = type,
                LinkUrl = link
            };
            _ctx.Notifications.Add(n);
            await _ctx.SaveChangesAsync();
            return n.NotificationId;
        }

        public async Task<List<Notification>> GetUnreadAsync(string user, int max = 10)
        {
            try
            {
                return await _ctx.Notifications
                    .Where(n => n.UserName == user && !n.IsRead)
                    .OrderByDescending(n => n.CreatedAt)
                    .Take(max)
                    .ToListAsync();
            }
            catch (SqlException)
            {
                // Table missing (migration not applied) -> return empty
                return new List<Notification>();
            }
        }

        public async Task MarkReadAsync(int id, string user)
        {
            var n = await _ctx.Notifications.FirstOrDefaultAsync(x => x.NotificationId == id && x.UserName == user);
            if (n == null) return;
            n.IsRead = true;
            await _ctx.SaveChangesAsync();
        }
    }
}