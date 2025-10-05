using CloudinaryDotNet;
using System;
using System.IO;
using System.Threading.Tasks;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;


namespace SafehavenPMS.Services
{
[Authorize]
    public class CloudinaryServices
    {
        //Configuration for Cloudinary
        private readonly Cloudinary _cloudinary;

        public CloudinaryServices(IConfiguration configuration)
        {
            var CloudName = configuration["Cloudinary:CloudName"];
            var ApiKey = configuration["Cloudinary:ApiKey"];
            var ApiSecret = configuration["Cloudinary:ApiSecret"];

            //Check if the configuration values are set
            if (string.IsNullOrEmpty(CloudName) || string.IsNullOrEmpty(ApiKey) || string.IsNullOrEmpty(ApiSecret))
            {
                throw new ArgumentException("Cloudinary configuration is not set properly.");
            }

            //Initialize Cloudinary with the provided configuration
            _cloudinary = new Cloudinary(new Account
            {
                Cloud = CloudName,
                ApiKey = ApiKey,
                ApiSecret = ApiSecret
            });
        }

        //Method to upload an image to Cloudinary
        public async Task<string> UploadImageAsync(Stream fileStream, string fileName)
        {
            //Check if the file stream is null or empty
            var uploadParams = new CloudinaryDotNet.Actions.ImageUploadParams
            {
                File = new FileDescription(fileName, fileStream),// Create a FileDescription with the file name and stream
                Transformation = new Transformation().Width(500).Height(500).Crop("fill"),// Resize the image to 500x500 pixels
                Folder = "SafehavenPMS/PatientProfileImages", // Specify the folder in Cloudinary where the image will be stored
            };

            //Check if the file stream is null or empty
            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            //Check if the upload was successful and return the secure URL of the uploaded image
            return uploadResult.SecureUrl?.ToString() ?? string.Empty;
        }

        // NEW: Upload receipt images (keeps original aspect, larger limit, stores under Payments/Receipts)
        public async Task<string> UploadReceiptAsync(Stream fileStream, string fileName)
        {
            if (fileStream == null) throw new ArgumentNullException(nameof(fileStream));
            if (string.IsNullOrEmpty(fileName)) fileName = $"{Guid.NewGuid():N}";

            // ensure stream is at start
            try { fileStream.Position = 0; } catch { /* ignore if stream is not seekable */ }

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(fileName, fileStream),
                Folder = "SafehavenPMS/Payments/Receipts",
                UseFilename = true,
                UniqueFilename = true,
                Overwrite = false,
                Transformation = new Transformation().Width(1200).Crop("limit") // no crop, just limit size
            };

            var result = await _cloudinary.UploadAsync(uploadParams);
            return result.SecureUrl?.ToString() ?? string.Empty;
        }
    }
}

