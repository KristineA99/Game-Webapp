using Microsoft.AspNetCore.Identity;

namespace Subapp1.DAL;

// Creates the "Admin" role and an admin user when the app starts,
// so the examiner can log in as admin without registering.
public static class IdentitySeeder
{
    public const string AdminRole = "Admin";

    public static async Task SeedAsync(IServiceProvider services, IConfiguration configuration)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("IdentitySeeder");

        // 1. Create the Admin role if it does not exist
        if (!await roleManager.RoleExistsAsync(AdminRole))
        {
            await roleManager.CreateAsync(new IdentityRole(AdminRole));
            logger.LogInformation("[IdentitySeeder] Role {Role} created.", AdminRole);
        }

        // 2. Read the admin login from appsettings.json
        var email = configuration["AdminUser:Email"];
        var nickname = configuration["AdminUser:Nickname"];
        var password = configuration["AdminUser:Password"];

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(nickname) || string.IsNullOrEmpty(password))
        {
            logger.LogWarning("[IdentitySeeder] AdminUser is missing in appsettings.json. No admin user was created.");
            return;
        }

        // 3. Create the admin user if it does not exist
        var admin = await userManager.FindByEmailAsync(email);
        if (admin == null)
        {
            admin = new IdentityUser { UserName = nickname, Email = email };
            var result = await userManager.CreateAsync(admin, password);

            if (!result.Succeeded)
            {
                logger.LogError("[IdentitySeeder] Could not create admin user: {Errors}",
                    string.Join(", ", result.Errors.Select(e => e.Description)));
                return;
            }
            logger.LogInformation("[IdentitySeeder] Admin user {Email} created.", email);
        }

        // 4. Give the user the Admin role
        if (!await userManager.IsInRoleAsync(admin, AdminRole))
        {
            await userManager.AddToRoleAsync(admin, AdminRole);
            logger.LogInformation("[IdentitySeeder] {Email} added to role {Role}.", email, AdminRole);
        }
    }
}