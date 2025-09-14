using System;

namespace SafehavenPMS.ViewModel.PatientProfile
{
    public class PatientPersonalInfoTabViewModel
    {
        // Add properties relevant to personal info 
        public int? PatientId { get; set; }
        public string FirstName { get; set; } = "-";
        public string LastName { get; set; } = "-";
        public string MiddleName { get; set; } = "-";
        public string PhotoUrl { get; set; } = "-"; 
        public DateTime? DateOfBirth { get; set; }
        public string Age { get; set; } = "-";
        public string MaritalStatus { get; set; } = "-";
        public string Occupation { get; set; } = "-";
        public string Religion { get; set; } = "-";
        public string Sex { get; set; } = "-";
        public string PhoneNumber { get; set; } = "-";
        public string Address { get; set; } = "-";

        public List<FamilyConstellationViewModel> FamilyConstellation { get; set; } = new List<FamilyConstellationViewModel>();
    }

    public class FamilyConstellationViewModel
    {
        public string Name { get; set; } = "-";
        public string Relationship { get; set; } = "-";
        public string Age { get; set; } = "-";
        public string Comments { get; set; } = "-";
    }
}
