// Data/DbInitializer.cs
using Microsoft.AspNetCore.Identity;
using MyEBookLibrary.Models;

namespace MyEBookLibrary.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(ApplicationDbContext context,
            UserManager<User> userManager, RoleManager<IdentityRole<int>> roleManager)
        {
            context.Database.EnsureCreated();

            // יצירת תפקידים אם לא קיימים
            var roles = new[] { "Admin", "User" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole<int>(role));
                }
            }

            // יצירת משתמש מנהל אם לא קיים
            var adminEmail = "admin@ebooklibrary.com";
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var admin = new User
                {
                    UserName = "admin",
                    Email = adminEmail,
                    EmailConfirmed = true,

                };

                var result = await userManager.CreateAsync(admin, "Admin123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Admin");
                }

            }
        }

    }
}