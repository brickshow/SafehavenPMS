using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;
using System; // add if not already present


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

        // NEW generic upload (any file type)
        public async Task<string> UploadFileAsync(Stream stream, string fileName, string folder = "SafehavenPMS/PatientDocuments")
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            if (string.IsNullOrWhiteSpace(fileName)) fileName = Guid.NewGuid().ToString("N");

            try { if (stream.CanSeek) stream.Position = 0; } catch { }

            var lower = fileName.ToLowerInvariant();
            bool isImage = lower.EndsWith(".png") || lower.EndsWith(".jpg") || lower.EndsWith(".jpeg") ||
                           lower.EndsWith(".gif") || lower.EndsWith(".webp") || lower.EndsWith(".bmp");

            if (isImage)
            {
                var imgParams = new ImageUploadParams
                {
                    File = new FileDescription(fileName, stream),
                    Folder = folder,
                    UseFilename = true,
                    UniqueFilename = true,
                    Overwrite = false
                };
                var result = await _cloudinary.UploadAsync(imgParams);
                return result?.SecureUrl?.ToString() ?? "";
            }
            else
            {
                var rawParams = new RawUploadParams
                {
                    File = new FileDescription(fileName, stream),
                    Folder = folder,
                    UseFilename = true,
                    UniqueFilename = true,
                    Overwrite = false
                };
                var result = await _cloudinary.UploadAsync(rawParams);
                return result?.SecureUrl?.ToString() ?? "";
            }
        }

        // NEW: patient document upload (separate folder per patient like other methods)
        public async Task<string> UploadPatientDocumentAsync(Stream stream, string fileName, int patientId, string? subFolder = null)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            if (string.IsNullOrWhiteSpace(fileName)) fileName = Guid.NewGuid().ToString("N");

            try { if (stream.CanSeek) stream.Position = 0; } catch { }

            // Base folder structure: SafehavenPMS/PatientDocuments/{patientId}/{optional subfolder}
            var folder = $"SafehavenPMS/PatientDocuments/{patientId}";
            if (!string.IsNullOrWhiteSpace(subFolder))
                folder += "/" + subFolder.Trim().Replace("\\", "/");

            var lower = fileName.ToLowerInvariant();
            bool isImage = lower.EndsWith(".png") || lower.EndsWith(".jpg") || lower.EndsWith(".jpeg")
                        || lower.EndsWith(".gif") || lower.EndsWith(".webp") || lower.EndsWith(".bmp");

            if (isImage)
            {
                var imgParams = new ImageUploadParams
                {
                    File = new FileDescription(fileName, stream),
                    Folder = folder,
                    UseFilename = true,
                    UniqueFilename = true,
                    Overwrite = false
                };
                var imgResult = await _cloudinary.UploadAsync(imgParams);
                return imgResult?.SecureUrl?.ToString() ?? "";
            }
            else
            {
                var rawParams = new RawUploadParams
                {
                    File = new FileDescription(fileName, stream),
                    Folder = folder,
                    UseFilename = true,
                    UniqueFilename = true,
                    Overwrite = false
                };
                var rawResult = await _cloudinary.UploadAsync(rawParams);
                return rawResult?.SecureUrl?.ToString() ?? "";
            }
        }
    }
}

