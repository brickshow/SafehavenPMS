using SafehavenPMS.Models;
using System.Text.Json;

namespace SafehavenPMS.Data
{
    public class DataSeeder
    {
        /// This method is used to seed initial data into the database.
        //Seed Data for naltionalities
        public static async Task SeedNationalitiesAsync(SafehavenPMSContext context, string jsonFilePath)
        {
            // Check if the Nationalities table is empty
            if (!context.Nationalities.Any())
            {
                //Read the JSON file
                var json = await File.ReadAllTextAsync(jsonFilePath);
                var nationalities = JsonSerializer.Deserialize<List<Nationality>>(json);


                //Check if the deserialization was successful
                if (nationalities != null)
                {
                    // Add the nationalities to the context
                    context.Nationalities.AddRange(nationalities);
                    // Save changes to the database
                    await context.SaveChangesAsync();
                }
            }
        }

        // Seed Data for Religions
        public static async Task SeedReligionsAsync(SafehavenPMSContext context, string jsonFilePath)
        {
            // Check if the Religions table is empty
            if (!context.Religions.Any())
            {
                //Read the JSON file
                var json = await File.ReadAllTextAsync(jsonFilePath);
                var religions = JsonSerializer.Deserialize<List<Religion>>(json);
                //Check if the deserialization was successful
                if (religions != null)
                {
                    // Add the religions to the context
                    context.Religions.AddRange(religions);
                    // Save changes to the database
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}
