using Microsoft.AspNetCore.Identity;

namespace Subapp2.DAL;

// Ensure the role used to protect management endpoints exists.
public static class IdentitySeeder
{
    public const string AdminRole = "Admin";

    public static async Task SeedAsync(IServiceProvider services)
    {
        var roleManager =
            services.GetRequiredService<RoleManager<IdentityRole>>();

        var logger = services
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("IdentitySeeder");

        // Reuse the existing role when the application restarts.
        if (!await roleManager.RoleExistsAsync(AdminRole))
        {
            var result = await roleManager.CreateAsync(
                new IdentityRole(AdminRole));

            if (!result.Succeeded)
            {
                logger.LogError(
                    "Could not create role {Role}: {Errors}",
                    AdminRole,
                    string.Join(", ", result.Errors.Select(e => e.Description)));

                return;
            }

            logger.LogInformation(
                "Role {Role} created successfully.",
                AdminRole);
        }
    }
}