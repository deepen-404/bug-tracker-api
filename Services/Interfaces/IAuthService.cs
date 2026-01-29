using BugTrackerApi.Models.DTOs.Auth;

namespace BugTrackerApi.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto);
    Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
    Task<UserDto> GetCurrentUserAsync(string userId);
}
