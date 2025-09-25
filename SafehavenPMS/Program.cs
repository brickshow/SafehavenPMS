using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SafehavenPMS.Data;
using SafehavenPMS.Services;
using System;
using System.Linq;
using Microsoft.AspNetCore.Identity;    
using SafehavenPMS.Models;
using SafehavenPMS.Data;

var builder = WebApplication.CreateBuilder(args);

// Register DbContext (update connection string name as needed)
builder.Services.AddDbContext<SafehavenPMSContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register email service if used by controllers
builder.Services.AddScoped<IEmailService, EmailService>();

// Add MVC
builder.Services.AddControllersWithViews();

// Enable session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// 🔐 Add cookie authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login/Login";      // redirect here if not logged in
        options.LogoutPath = "/Login/Logout";    // optional logout path
        options.AccessDeniedPath = "/Login/AccessDenied"; // if role restricted
        options.Cookie.Name = "Safehaven.Auth";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    });

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Middleware order matters 
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // must be before UseAuthorization
app.UseAuthorization();

app.UseSession();


// Default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Login}/{id?}");

app.Run();

