using Isopoh.Cryptography.Argon2;
using LiPi.Master;
using LiPi.Master.Entities;
using Microsoft.EntityFrameworkCore;

namespace LiPi.Web.Services;

/// <summary>
/// One-time CLI bootstrap: dotnet run --project LiPi.Web -- --setup-global-admin
/// Creates the first Global Admin in master.platform_users.
/// </summary>
public static class GlobalAdminBootstrap
{
    public static async Task RunAsync(IServiceProvider services)
    {
        Console.WriteLine();
        Console.WriteLine("  ╔══════════════════════════════════════════════╗");
        Console.WriteLine("  ║   LiPi HIS — Global Admin Bootstrap          ║");
        Console.WriteLine("  ╚══════════════════════════════════════════════╝");
        Console.WriteLine();

        var factory = services.GetRequiredService<IDbContextFactory<MasterDbContext>>();
        await using var db = await factory.CreateDbContextAsync();

        // Check if any global_admin already exists
        var existingAdmin = await db.PlatformUsers
            .FirstOrDefaultAsync(u => u.UserType == "global_admin" && u.DeletedAt == null);

        if (existingAdmin != null)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"  ⚠  Global Admin already exists: '{existingAdmin.Username}'");
            Console.WriteLine("     Bootstrap aborted — use the application to manage admins.");
            Console.ResetColor();
            Console.WriteLine();
            return;
        }

        Console.WriteLine("  No Global Admin found. Creating first Global Admin.");
        Console.WriteLine();

        Console.Write("  First Name: ");
        var firstName = Console.ReadLine()?.Trim() ?? "Global";

        Console.Write("  Last Name: ");
        var lastName = Console.ReadLine()?.Trim() ?? "Admin";

        Console.Write("  Username [Admin]: ");
        var username = Console.ReadLine()?.Trim();
        if (string.IsNullOrEmpty(username)) username = "Admin";

        Console.Write("  Email: ");
        var email = Console.ReadLine()?.Trim() ?? string.Empty;

        Console.Write("  Password (min 8 chars): ");
        var password = ReadPassword();

        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n  ✗ Password too short. Aborted.");
            Console.ResetColor();
            return;
        }

        var hash = Argon2.Hash(password);

        var user = new PlatformUser
        {
            Id                = Guid.NewGuid(),
            Username          = username,
            PasswordHash      = hash,
            FirstName         = firstName,
            LastName          = lastName,
            DisplayName       = $"{firstName} {lastName}".Trim(),
            Email             = email,
            UserType          = "global_admin",
            Status            = "active",
            MustChangePassword= true,
            CreatedAt         = DateTime.UtcNow,
            UpdatedAt         = DateTime.UtcNow,
        };

        db.PlatformUsers.Add(user);
        await db.SaveChangesAsync();

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"  ✓ Global Admin created: '{username}'");
        Console.WriteLine($"    ID: {user.Id}");
        Console.WriteLine($"    Must change password on first login: YES");
        Console.ResetColor();
        Console.WriteLine();
        Console.WriteLine("  ⚠  Store the password securely.");
        Console.WriteLine();
    }

    private static string ReadPassword()
    {
        var pwd = new System.Text.StringBuilder();
        while (true)
        {
            var key = Console.ReadKey(intercept: true);
            if (key.Key == ConsoleKey.Enter) { Console.WriteLine(); break; }
            if (key.Key == ConsoleKey.Backspace && pwd.Length > 0)
            { pwd.Remove(pwd.Length - 1, 1); Console.Write("\b \b"); }
            else if (key.Key != ConsoleKey.Backspace)
            { pwd.Append(key.KeyChar); Console.Write("*"); }
        }
        return pwd.ToString();
    }
}
