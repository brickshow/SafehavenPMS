using System.ComponentModel.DataAnnotations;

namespace SafehavenPMS.ViewModel
{
    public class AvailabilityViewModel
    {
        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Start Date is required")]

        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }//Not required
    }
}
