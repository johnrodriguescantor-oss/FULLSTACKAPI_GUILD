using BCrypt.Net;
using FullStackAPI_Guild.Api.Data;
using FullStackAPI_Guild.Api.Entities;
using FullStackAPI_Guild.Api.Enums;
using Microsoft.EntityFrameworkCore;

namespace FullStackAPI_Guild.Api.Services;

public static class DevUserSeedService
{
    public static async Task SeedAsync(AppDbContext context)
    {
        var devUserExists = await context.Users.AnyAsync(x => x.Role == UserRole.DEV);

        if (devUserExists)
        {
            return;
        }

        var devUser = new User
        {
            Id = Guid.NewGuid(),
            Username = "devmaster",
            Email = "dev@guild.local",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Dev@123"),
            DisplayName = "DEV Master",
            Role = UserRole.DEV,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        context.Users.Add(devUser);
        await context.SaveChangesAsync();
    }
}
