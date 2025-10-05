using Microsoft.AspNetCore.Authorization;
namespace SafehavenPMS.ViewModel.ServicesType

{
[Authorize]
    public class ServicesTypeViewModel
    {
        public int ServicesTypeId { get; set; }
        public string ServiceName { get; set; }
        public string Description { get; set; }
    }
}
