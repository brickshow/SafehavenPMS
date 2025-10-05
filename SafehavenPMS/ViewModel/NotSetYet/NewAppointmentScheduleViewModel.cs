using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;


namespace SafehavenPMS.ViewModel
{
[Authorize]
    public class NewAppointmentScheduleViewModel
    {
        public int AppointmentID { get; set; }

      
       
       
       
      

        
        public string? Description { get; set; }
    }
}

