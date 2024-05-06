using BlogDomain.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BlogInfrastructure.Controllers;

namespace BlogInfrastructure
{
    public class RoleInitializer
    {
        public static async Task InitializeAsync(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, WritersController writersController, ReadersController readersController, LldbContext context)
        {
            string adminEmail = "admin@gmail.com";
            string password = "Qwerty_1";
            string adminUserName = "Toretto";
            
            List<string> roleNames = new List<string> { "admin", "reader", "writer" };

            foreach (var roleName in roleNames)
            {
                if (await roleManager.FindByNameAsync(roleName) == null)
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
            
            if (await userManager.FindByNameAsync(adminEmail) == null)
            {
                User admin = new User { Email = adminEmail, UserName = adminUserName, Bio = "generated" };
                IdentityResult result = await userManager.CreateAsync(admin, password);

                if (result.Succeeded)
                {
                    foreach (var roleName in roleNames)
                    {
                        await userManager.AddToRoleAsync(admin, roleName);
                    }
                    
                    context.Users.Add(admin);
                    await context.SaveChangesAsync();

                    await writersController.AddWriter(admin.UserName, admin.Id);
                    
                    await readersController.AddReader(admin.UserName, admin.Id);
                    
                }
            }
        }
    }
}
