using SafehavenPMS.Models;
using System.Text.Json;

namespace SafehavenPMS.Data
{
    public class DataSeeder
    {
        public static async Task SeedNationalitiesAsync(SafehavenPMSContext context, string jsonFilePath)
        {
            try
            {
                if (!context.Nationalities.Any())
                {
                    var jsonString = await File.ReadAllTextAsync(jsonFilePath);
                    var nationalities = JsonSerializer.Deserialize<List<Nationality>>(jsonString, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (nationalities != null)
                    {
                        foreach (var nationality in nationalities)
                        {
                            nationality.Patients = new List<Patient>(); // Initialize the navigation property
                            context.Nationalities.Add(nationality);
                        }
                        await context.SaveChangesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error seeding nationalities: {ex.Message}");
            }
        }

        public static async Task SeedReligionsAsync(SafehavenPMSContext context, string jsonFilePath)
        {
            try
            {
                if (!context.Religions.Any())
                {
                    var jsonString = await File.ReadAllTextAsync(jsonFilePath);
                    var religions = JsonSerializer.Deserialize<List<Religion>>(jsonString, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (religions != null)
                    {
                        foreach (var religion in religions)
                        {
                            religion.Patients = new List<Patient>(); // Initialize the navigation property
                            context.Religions.Add(religion);
                        }
                        await context.SaveChangesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error seeding religions: {ex.Message}");
            }
        }
    }
}
