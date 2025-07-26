using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using InnovationPlatform.Data;
using InnovationPlatform.Models;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=innovation_platform.db";
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

// Add simple cookie authentication as the default and only scheme
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/Login";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
    });

// Add authorization services
builder.Services.AddAuthorization();

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Initialize database and seed data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    // Ensure database is created
    context.Database.EnsureCreated();

    // Seed simple users table
    await SeedSimpleUsers(context);
}

async Task SeedSimpleUsers(ApplicationDbContext context)
{
    // Check if simple users already exist
    if (!await context.SimpleUsers.AnyAsync())
    {
        var users = new List<User>
        {
            new User
            {
                Username = "admin",
                Email = "admin@aie.gov.al",
                Password = "admin123",
                Role = "Executive",
                IsActive = true
            },
            new User
            {
                Username = "expert",
                Email = "expert@aie.gov.al",
                Password = "expert123",
                Role = "Expert",
                IsActive = true
            },
             new User
            {
                Username = "applicant",
                Email = "applicant@aie.gov.al",
                Password = "applicant123",
                Role = "Applicant",
                IsActive = true
            }
        };

        context.SimpleUsers.AddRange(users);
        await context.SaveChangesAsync();
        Console.WriteLine("Simple users seeded successfully");
    }
    else
    {
        Console.WriteLine("Simple users already exist");
    }
}

app.Run();
