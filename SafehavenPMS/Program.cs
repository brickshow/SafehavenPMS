
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SafehavenPMS.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

//Adding sessions
builder.Services.AddSession(options =>
{
    //Sessions will expires within 30 minutes
    options.IdleTimeout = TimeSpan.FromMinutes(30);

    // Make the session cookie HTTP only for security
    options.Cookie.HttpOnly = true;

    // Mark the session cookie as essential for GDPR compliance
    options.Cookie.IsEssential = true;
});

//Configure Entity Framework Core with SQL Server
builder.Services.AddDbContext<SafehavenPMS.Data.SafehavenPMSContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Enable session middleware in the request pipeline
app.UseSession();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=ClinicalStaff}/{action=Index}/{id?}")
    .WithStaticAssets();


using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<SafehavenPMSContext>();
        context.Database.Migrate();


        // Seed Religions
        var religionPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "SeedData", "Religion.json");
        await DataSeeder.SeedReligionsAsync(context, religionPath);

        // Seed Nationalities
        var nationalityPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "SeedData", "Nationality.json");
        await DataSeeder.SeedNationalitiesAsync(context, nationalityPath);

    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.ToString());
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}



app.Run();
