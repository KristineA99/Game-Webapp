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

        var configuration = services.GetRequiredService<IConfiguration>();
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

        var email = configuration["AdminUser:Email"];
        var nickname = configuration["AdminUser:Nickname"];
        var password = configuration["AdminUser:Password"];

        if (string.IsNullOrWhiteSpace(email) ||
        string.IsNullOrWhiteSpace(nickname) ||
        string.IsNullOrWhiteSpace(password))
        {
            logger.LogWarning(
                "Admin account configuration is incomplete; account seeding was skipped.");
            return;
        }

        // Preserve the existing account and password when the application restarts.
        var admin = await userManager.FindByEmailAsync(email);

        if (admin == null)
        {
            admin = new IdentityUser
            {
                UserName = nickname,
                Email = email
            };

            // Identity hashes the password before storing it.
            var result = await userManager.CreateAsync(admin, password);

            if (!result.Succeeded)
            {
                logger.LogError(
                    "Could not create the demo admin account: {Errors}",
                    string.Join(", ", result.Errors.Select(e => e.Description)));
                return;
            }

            logger.LogInformation("Demo admin account created successfully.");
        }

        // Ensure both new and existing demo accounts have the required role.
        if (!await userManager.IsInRoleAsync(admin, AdminRole))
        {
            var result = await userManager.AddToRoleAsync(admin, AdminRole);

            if (!result.Succeeded)
            {
                logger.LogError(
                    "Could not assign role {Role} to the demo admin account: {Errors}",
                    AdminRole,
                    string.Join(", ", result.Errors.Select(e => e.Description)));

                return;
            }

            logger.LogInformation(
                "Role {Role} assigned to the demo admin account.",
                AdminRole);
        }
    }
}