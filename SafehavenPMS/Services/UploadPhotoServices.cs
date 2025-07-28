namespace SafehavenPMS.Services
{
    //This class is for temporary storage of uploaded photos for clinical staff
    public class UploadPhotoServices
    {
        string tempPath = string.Empty;

        //Method to upload a photo
        public string UploadPhoto(IFormFile ProfileImage)
        {
            var tempDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "TempPhotos");
            // Ensure the TempPhotos directory exists
            if (!Directory.Exists(tempDirectory))
            {
                Directory.CreateDirectory(tempDirectory);
            }

            // Generate a unique filename for the uploaded photo
            var fileName = $"{Guid.NewGuid()}.jpg"; // Use GUID to ensure uniqueness
            tempPath = Path.Combine(tempDirectory, fileName);

            // Save the uploaded photo to the TempPhotos directory
            using (var fileStream = new FileStream(tempPath, FileMode.Create))
            {
                //Copy the uploaded file to the temp path   
                ProfileImage.CopyTo(fileStream);
            }

            //Return the filename for further processing or storage in the database
            return "TempPhotos/" + fileName;
        }

        //Method to delete the uploaded photo
        public void DeletePhoto(string filename)
        {
            //Add Directory check to ensure the tempPath is not empty
            var tempDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "TempPhotos");
            var tempPath = Path.Combine(tempDirectory,filename); // Example path, replace with actual logic

            //Check if the file exists before attempting to delete it
            if (File.Exists(tempPath))
            {
                try
                {
                    File.Delete(tempPath); // Delete the file
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error deleting file: {ex.Message}"); // Log error if deletion fails
                }
            }
            else
            {
                Console.WriteLine("File does not exist."); // Log if file does not exist
            }
        }
    }
}
