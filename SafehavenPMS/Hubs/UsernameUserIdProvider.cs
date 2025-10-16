using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace SafehavenPMS.Hubs
{
    public class UsernameUserIdProvider : IUserIdProvider
    {
        public string GetUserId(HubConnectionContext connection)
        {
            // Use the authenticated username as the SignalR user identifier
            return connection.User?.Identity?.Name
                ?? connection.User?.FindFirstValue(ClaimTypes.Name)
                ?? connection.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        }
    }
}


