using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using MyEBookLibrary.Data;
using MyEBookLibrary.Models;
using MyEBookLibrary.Services;
using MyEBookLibrary.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<User, IdentityRole<int>>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>();

// Adding services
builder.Services.AddScoped<ILibraryService, LibraryService>();
builder.Services.AddScoped<IEmailNotificationService, EmailNotificationService>();
builder.Services.AddControllersWithViews();
builder.Services.AddApplicationInsightsTelemetry();
builder.Services.AddScoped<ICartService, CartService>();


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Default routing changed to login
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

// Initialize Database and Check Data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<User>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole<int>>>();

        // Initialize database
        await DbInitializer.Initialize(context, userManager, roleManager);

        // Optionally check database contents in a development environment
        if (app.Environment.IsDevelopment())
        {
            var books = await context.Books.ToListAsync();
            Console.WriteLine("\nChecking Books table:");
            foreach (var book in books)
            {
                Console.WriteLine($"Book: {book.Title} by {book.Author}");
            }

            var users = await context.Users.ToListAsync();
            Console.WriteLine("\nChecking Users table:");
            foreach (var user in users)
            {
                Console.WriteLine($"User: {user.UserName} ({user.Email})");
            }
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while initializing the database.");
    }
}

// Add global authorization policy
//app.MapControllers().RequireAuthorization();

app.Run();
