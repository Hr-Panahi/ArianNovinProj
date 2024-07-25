using Microsoft.AspNetCore.Identity;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        await SeedRoles(roleManager);
        await SeedAdminUser(userManager);
    }

    private static async Task SeedRoles(RoleManager<IdentityRole> roleManager)
    {
        var roles = new[] { "Admin", "User" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    private static async Task SeedAdminUser(UserManager<IdentityUser> userManager)
    {
        var adminEmail = "admin@example.com";
        var adminPassword = "Admin@123";

        if (await userManager.FindByEmailAsync(adminEmail) == null)
        {
            var adminUser = new IdentityUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(adminUser, adminPassword);

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
    }
}
