using BugTrackerApi.Models.DTOs.User;
using BugTrackerApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BugTrackerApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController(IUserService userService) : ControllerBase
{
    [HttpGet("developers")]
    public async Task<ActionResult<IEnumerable<DeveloperDto>>> GetDevelopers()
    {
        var developers = await userService.GetDevelopersAsync();
        return Ok(developers);
    }
}
