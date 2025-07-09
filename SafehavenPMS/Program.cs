
using Microsoft.EntityFrameworkCore;

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
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
