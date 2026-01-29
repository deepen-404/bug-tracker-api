using BugTrackerApi.Models.DTOs.User;
using BugTrackerApi.Models.Entities;
using BugTrackerApi.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace BugTrackerApi.Services.Implementations;

public class UserService(UserManager<ApplicationUser> userManager) : IUserService
{
    public async Task<IEnumerable<DeveloperDto>> GetDevelopersAsync()
    {
        var developers = await userManager.GetUsersInRoleAsync("Developer");

        return developers.Select(d => new DeveloperDto
        {
            Id = d.Id,
            FullName = d.FullName,
            Email = d.Email ?? string.Empty
        }).OrderBy(d => d.FullName);
    }

    public async Task<bool> IsDeveloperAsync(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user == null) return false;

        return await userManager.IsInRoleAsync(user, "Developer");
    }
}
