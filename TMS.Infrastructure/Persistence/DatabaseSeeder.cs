using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TMS.Domain.Entities;
using TMS.Domain.Enums;

namespace TMS.Infrastructure.Persistence;

/// <summary>
/// Seeds the database with roles and a default Admin account on first run.
/// Call from Program.cs after EF migrations have been applied.
/// </summary>
public class DatabaseSeeder
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly AppDbContext _context;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        AppDbContext context,
        ILogger<DatabaseSeeder> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        try
        {
            // Run any pending migrations automatically
            await _context.Database.MigrateAsync();

            await SeedRolesAsync();
            await SeedDefaultAdminAsync();

            _logger.LogInformation("Database seeding completed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    // ── Roles ─────────────────────────────────────────────────────────────────

    private async Task SeedRolesAsync()
    {
        var roles = Enum.GetNames<UserRole>();
        foreach (var role in roles)
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new IdentityRole(role));
                _logger.LogInformation("Created role: {Role}", role);
            }
        }
    }

    // ── Default Admin ─────────────────────────────────────────────────────────

    private async Task SeedDefaultAdminAsync()
    {
        const string adminEmail = "admin@tms.local";

        if (await _userManager.FindByEmailAsync(adminEmail) is not null)
            return;  // Already seeded

        var admin = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FirstName = "System",
            LastName = "Admin",
            Role = UserRole.Admin,
            IsActive = true,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(admin, "Admin@12345!");

        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(admin, UserRole.Admin.ToString());
            _logger.LogInformation(
                "Default admin created. Email: {Email}  Password: Admin@12345!", adminEmail);
        }
        else
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogError("Failed to create default admin: {Errors}", errors);
        }
    }
}
