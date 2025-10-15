using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SafehavenPMS.Hubs;
using SafehavenPMS.Models;
using SafehavenPMS.Data;
using System.Threading.Tasks;
using System;
using System.ComponentModel.DataAnnotations;

namespace SafehavenPMS.Controllers
{
    public class EmailController : Controller
    {
        private readonly SafehavenPMSContext _context;
        private readonly IHubContext<MessageHub> _hubContext;

        public EmailController(SafehavenPMSContext context, IHubContext<MessageHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
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

            ViewBag.Section = section;
            ViewBag.Messages = messages.OrderByDescending(m => m.SentAt).ToList();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] MessageDto dto)

        {
            var fromUser = User.Identity.Name;

            // Validate recipient exists and is active
            var recipient = _context.Users.FirstOrDefault(u => u.Username == dto.To && u.IsActive);
            if (recipient == null)
            {
                return BadRequest("Recipient does not exist or is not active.");
            }

            var message = new Message
            {
                ToUser = dto.To,
                FromUser = fromUser,
                Subject = dto.Subject,
                Body = dto.Body,
                SentAt = DateTime.Now,
                IsArchived = false
            };
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            await _hubContext.Clients.User(dto.To).SendAsync("ReceiveMessage", fromUser, dto.Subject, dto.Body);

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> ArchiveMessage(int id)
        {
            var message = await _context.Messages.FindAsync(id);
            if (message != null && message.ToUser == User.Identity.Name)
            {
                message.IsArchived = true;
                await _context.SaveChangesAsync();
            }
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> DeleteMessage(int id)
        {
            var message = await _context.Messages.FindAsync(id);
            if (message != null && (message.ToUser == User.Identity.Name || message.FromUser == User.Identity.Name))
            {
                _context.Messages.Remove(message);
                await _context.SaveChangesAsync();
            }
            return Ok();
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

