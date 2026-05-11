using EventsAndPolls.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace EventsAndPolls.Infrastructure.Identity;

// Role name constants — use these everywhere instead of magic strings
public static class Roles
{
     public const string Organizer = "Organizer";
     public const string User = "User";
}

// Seeds roles and a default organizer account on startup
public static class IdentitySeeder
{
     public static async Task SeedAsync(
         UserManager<ApplicationUser> userManager,
         RoleManager<IdentityRole> roleManager)
     {
          // 1. Create roles if they don't exist
          foreach (var role in new[] { Roles.Organizer, Roles.User })
          {
               if (!await roleManager.RoleExistsAsync(role))
               {
                    await roleManager.CreateAsync(new IdentityRole(role));
               }
          }

          // 2. Create default organizer/admin account
          const string adminEmail = "admin@eventhub.com";
          const string adminPassword = "Admin123!";

          if (await userManager.FindByEmailAsync(adminEmail) == null)
          {
               var admin = new ApplicationUser
               {
                    UserName = adminEmail,
                    Email = adminEmail,
                    DisplayName = "EventHub Admin",
                    EmailConfirmed = true
               };

               var result = await userManager.CreateAsync(admin, adminPassword);
               if (result.Succeeded)
               {
                    await userManager.AddToRoleAsync(admin, Roles.Organizer);
                    await userManager.AddClaimAsync(admin,
                         new System.Security.Claims.Claim("DisplayName", admin.DisplayName));
               }
          }
     }
}
