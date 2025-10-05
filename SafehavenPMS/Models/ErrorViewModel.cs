using Microsoft.AspNetCore.Authorization;
namespace SafehavenPMS.Models
{
[Authorize]
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}

