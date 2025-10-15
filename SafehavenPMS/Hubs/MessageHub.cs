using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace SafehavenPMS.Hubs
{
    public class MessageHub : Hub
    {
        public async Task SendMessage(string toUser, string fromUser, string subject, string body)
        {
            await Clients.User(toUser).SendAsync("ReceiveMessage", fromUser, subject, body);
        }
    }
}