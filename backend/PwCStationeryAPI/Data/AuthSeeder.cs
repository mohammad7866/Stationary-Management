using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using PwCStationeryAPI.Models;

namespace PwCStationeryAPI.Data
{
    public static class AuthSeeder
    {
        public static async Task SeedAsync(IServiceProvider sp)
        {
            using var scope = sp.CreateScope();
            var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // roles
            foreach (var r in new[] { "User", "Admin", "SuperAdmin" })
                if (!await roleMgr.RoleExistsAsync(r))
                    await roleMgr.CreateAsync(new IdentityRole(r));

            async Task<ApplicationUser> EnsureUser(string email, string role)
            {
                var u = await userMgr.FindByEmailAsync(email);
                if (u == null)
                {
                    u = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true };
                    await userMgr.CreateAsync(u, "P@ssw0rd!");
                }
                if (!await userMgr.IsInRoleAsync(u, role))
                    await userMgr.AddToRoleAsync(u, role);
                return u;
            }

            await EnsureUser("user@demo.local", "User");
            await EnsureUser("admin@demo.local", "Admin");
            await EnsureUser("super@demo.local", "SuperAdmin");
        }
    }
}
