using BestStoreMVC.Models;
using Microsoft.AspNetCore.Identity;

namespace BestStoreMVC.Services
{
    public class DatabaseInitializer
    {
        public static async Task SeedDataAsync(UserManager<ApplicationUser>?userManager,RoleManager<IdentityRole>?roleManager)
        {
            if(userManager == null || roleManager ==null)
            {
                var msg= "user manager or role manager is null";
                return;
            }
            var exists = await roleManager.RoleExistsAsync("admin");
            if(!exists)
            {
                var msg = "Admin role is not defined and will created";
                await roleManager.CreateAsync(new IdentityRole("admin"));
            }
            exists=await roleManager.RoleExistsAsync("seller");
            if(!exists)
            {
                var msg = "Seller role is not defined and will created";
                await roleManager.CreateAsync(new IdentityRole("seller"));
            }
            exists = await roleManager.RoleExistsAsync("client");
            if(!exists)
            {
                var msg = "Client role is not defined and will created";
                await roleManager.CreateAsync(new IdentityRole("client"));
            }
            //check if admin exists
            var adminUser=await userManager.GetUsersInRoleAsync("admin");
            if(adminUser.Any())
            {
                Console.WriteLine("Admin User already exist=>exit");
                return;
            }
            //create the admin user
            var user = new ApplicationUser
            {
                FirstName = "Admin",
                LastName = "Admin",
                UserName = "admin@admin.com",
                Email = "admin@admin.com",
                CreatedAt = DateTime.Now,
            };
            string initialPassword = "admin123";

            var result = await userManager.CreateAsync(user, initialPassword);
            if (result.Succeeded)
            {
                //set the user role
                await userManager.AddToRoleAsync(user, "admin");

            }

        }
    }
}
