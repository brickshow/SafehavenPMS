namespace SafehavenPMS.Models
{
    public class Service
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; }
        public string Description { get; set; }
        public int ServiceTypeId { get; set; }
        public ServiceType? ServiceType { get; set; }
        public string AssignedRole { get; set; }

        //Audit fields
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
    }
}