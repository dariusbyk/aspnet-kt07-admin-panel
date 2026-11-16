using Microsoft.AspNetCore.Identity;

namespace AdminPanel.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider services)
        {
            AppDbContext context = services.GetRequiredService<AppDbContext>();
            await context.Database.EnsureCreatedAsync();

            RoleManager<IdentityRole> roles = services.GetRequiredService<RoleManager<IdentityRole>>();
            UserManager<IdentityUser> users = services.GetRequiredService<UserManager<IdentityUser>>();

            string[] names = { "Admin", "User" };
            foreach (string name in names)
            {
                if (!await roles.RoleExistsAsync(name))
                {
                    Check(await roles.CreateAsync(new IdentityRole(name)));
                }
            }

            // Начальные учебные аккаунты создаются только в пустой базе.
            if (!users.Users.Any())
            {
                IdentityUser admin = new IdentityUser();
                admin.UserName = "admin";
                admin.Email = "admin@example.com";
                Check(await users.CreateAsync(admin, "Admin123!"));
                Check(await users.AddToRoleAsync(admin, "Admin"));

                IdentityUser user = new IdentityUser();
                user.UserName = "user";
                user.Email = "user@example.com";
                Check(await users.CreateAsync(user, "User123!"));
                Check(await users.AddToRoleAsync(user, "User"));
            }
        }

        private static void Check(IdentityResult result)
        {
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));
            }
        }
    }
}
