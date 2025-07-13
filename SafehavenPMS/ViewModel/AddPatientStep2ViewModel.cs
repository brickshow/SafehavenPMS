using Microsoft.AspNetCore.Http;
using System.Text.Json.Serialization;

namespace SafehavenPMS.ViewModel
{
    public class AddPatientStep2ViewModel
    {
        // For receiving the uploaded image
        [JsonIgnore] // Optional, if you're serializing the object later
        public IFormFile? ProfileImage { get; set; }

        //Adding Temp File path
        public string? TempFilePath { get; set; }
    }
}