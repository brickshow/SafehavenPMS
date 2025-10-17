using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SafehavenPMS.Data;
using SafehavenPMS.Hubs;
using SafehavenPMS.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SafehavenPMS.Controllers
{
    [Authorize]
    public class EmailController : Controller
    {
        private readonly SafehavenPMSContext _context;

        public EmailController(SafehavenPMSContext context)
        {
            _context = context;
        }

        public IActionResult Index(string section)
        {
            var user = User.Identity.Name;
            IQueryable<Message> messages = _context.Messages;

            switch ((section ?? "inbox").ToLower())
            {
                case "sent":
                    messages = messages.Where(m => m.FromUser == user);
                    break;
                case "archive":
                    messages = messages.Where(m => m.ToUser == user && m.IsArchived);
                    break;
                case "inbox":
                default:
                    messages = messages.Where(m => m.ToUser == user && !m.IsArchived);
                    break;
            }

            var messageList = messages.OrderByDescending(m => m.SentAt).ToList();

            // ✅ Replace usernames with full names dynamically
            foreach (var msg in messageList)
            {
                var fromFullName = _context.Users
                    .Where(u => u.Username == msg.FromUser)
                    .Select(u => u.Fullname)
                    .FirstOrDefault();

                var toFullName = _context.Users
                    .Where(u => u.Username == msg.ToUser)
                    .Select(u => u.Fullname)
                    .FirstOrDefault();

                if (!string.IsNullOrEmpty(fromFullName))
                    msg.FromUser = fromFullName;

                if (!string.IsNullOrEmpty(toFullName))
                    msg.ToUser = toFullName;
            }

            ViewBag.Section = section;
            ViewBag.Messages = messageList;
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMessage(MessageDto dto)

        {
            var fromUser = User.Identity.Name;

            // Validate recipient exists and is active
            var recipient = _context.Users
                            .FirstOrDefault(u =>
                                 u.IsActive &&
                                (u.Username == dto.To || u.Fullname == dto.To));

            if (recipient == null)
            {
                TempData["ToastMessage"] = "Recipient not found or inactive.";
                TempData["ToastType"] = "error";
                return RedirectToAction("Index");
            }

            var message = new Message
            {
                ToUser = recipient.Username, // always store username
                FromUser = fromUser,
                Subject = dto.Subject,
                Body = dto.Body,
                SentAt = DateTime.Now,
                IsArchived = false
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            //// This is correct and triggers the SignalR client event
            //await _hubContext.Clients.User(dto.To).SendAsync("ReceiveMessage", fromUser, dto.Subject, dto.Body);


            return Redirect("Index?section=inbox");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArchiveMessage(int id)
        {
            var message = await _context.Messages.FindAsync(id);
            if (message != null && message.ToUser == User.Identity.Name)
            {
                message.IsArchived = true;
                await _context.SaveChangesAsync();
                TempData["ToastMessage"] = "Message archived successfully.";
                TempData["ToastType"] = "success";
            }
            else
            {
                TempData["ToastMessage"] = "Unable to archive message.";
                TempData["ToastType"] = "error";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMessage(int id)
        {
            var message = await _context.Messages.FindAsync(id);
            if (message != null && (message.ToUser == User.Identity.Name || message.FromUser == User.Identity.Name))
            {
                _context.Messages.Remove(message);
                await _context.SaveChangesAsync();
                TempData["ToastMessage"] = "Message deleted successfully.";
                TempData["ToastType"] = "success";
            }
            else
            {
                TempData["ToastMessage"] = "Unable to delete message.";
                TempData["ToastType"] = "error";
            }

            return RedirectToAction("Index");
        }


        [HttpGet]
        public IActionResult GetUnreadCount()
        {
            var username = User.Identity.Name;
            var unreadCount = _context.Messages
                .Count(e => e.ToUser == username && !e.IsRead);

            return Json(new { unreadCount });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var message = await _context.Messages.FindAsync(id);
            var currentUser = User.Identity.Name;
            
            // Only the recipient can mark a message as read
            if (message != null && message.ToUser == currentUser)
            {
                // Only mark as read if it is currently unread
                if (!message.IsRead)
                {
                    message.IsRead = true;
                    await _context.SaveChangesAsync();
                    TempData["ToastMessage"] = "Message marked as read.";
                    TempData["ToastType"] = "success";
                }
                else
                {
                    TempData["ToastMessage"] = "Message is already read.";
                    TempData["ToastType"] = "info";
                }

                return RedirectToAction("Index");
            }

            TempData["ToastMessage"] = "Unable to mark message as read.";
            TempData["ToastType"] = "error";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult SearchUsers(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return Json(new { results = new string[0] });

            var matches = _context.Users
                .Where(u => u.IsActive &&
                           (u.Username.Contains(term) ||
                            (u.Fullname != null && u.Fullname.Contains(term))))
                .Select(u => new
                {
                    label = $"{u.Fullname} ({u.Username})",
                    value = u.Username
                })
                .Take(10)
                .ToList();

            return Json(new { results = matches });
        }

    }

    // Add this DTO class inside the controller or in a shared location
    public class MessageDto
    {
        [Required]
        public string To { get; set; }
        [Required]
        public string Subject { get; set; }
        [Required]
        public string Body { get; set; }
    }
}

