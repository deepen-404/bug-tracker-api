using BugTrackerApi.Models.DTOs.User;

namespace BugTrackerApi.Services.Interfaces;

public interface IUserService
{
    Task<IEnumerable<DeveloperDto>> GetDevelopersAsync();
    Task<bool> IsDeveloperAsync(string userId);
}
