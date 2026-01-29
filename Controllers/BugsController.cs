using System.Security.Claims;
using BugTrackerApi.Models.DTOs.Bug;
using BugTrackerApi.Models.DTOs.Common;
using BugTrackerApi.Models.Enums;
using BugTrackerApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BugTrackerApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BugsController(IBugService bugService, IUserService userService) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "User,Developer")]
    public async Task<ActionResult<BugResponseDto>> CreateBug([FromBody] CreateBugDto createBugDto)
    {
        var userId = GetUserId();
        var result = await bugService.CreateBugAsync(createBugDto, userId);
        return CreatedAtAction(nameof(GetBugById), new { id = result.Id }, result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<BugResponseDto>> GetBugById(int id)
    {
        var result = await bugService.GetBugByIdAsync(id);
        return Ok(result);
    }

    [HttpGet("my-bugs")]
    [Authorize(Roles = "User,Developer")]
    public async Task<ActionResult<PaginatedResponse<BugListDto>>> GetMyBugs(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var userId = GetUserId();
        var pagination = new PaginationParams { PageNumber = pageNumber, PageSize = pageSize };
        var result = await bugService.GetBugsByReporterAsync(userId, pagination);
        return Ok(result);
    }

    [HttpGet("assigned")]
    [Authorize(Roles = "Developer")]
    public async Task<ActionResult<PaginatedResponse<BugListDto>>> GetAssignedBugs(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var developerId = GetUserId();
        var pagination = new PaginationParams { PageNumber = pageNumber, PageSize = pageSize };
        var result = await bugService.GetAssignedBugsAsync(developerId, pagination);
        return Ok(result);
    }

    [HttpGet("unassigned")]
    [Authorize(Roles = "Developer")]
    public async Task<ActionResult<PaginatedResponse<BugListDto>>> GetUnassignedBugs(
        [FromQuery] string? search,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var pagination = new PaginationParams { PageNumber = pageNumber, PageSize = pageSize };
        var result = await bugService.GetUnassignedBugsAsync(pagination, search);
        return Ok(result);
    }

    [HttpGet("search")]
    public async Task<ActionResult<PaginatedResponse<BugListDto>>> SearchBugs(
        [FromQuery] string? search,
        [FromQuery] Severity? severity,
        [FromQuery] BugStatus? status,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var pagination = new PaginationParams { PageNumber = pageNumber, PageSize = pageSize };
        var result = await bugService.SearchBugsAsync(pagination, search, severity, status);
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<BugResponseDto>> UpdateBug(int id, [FromBody] UpdateBugDto updateBugDto)
    {
        var result = await bugService.UpdateBugAsync(id, updateBugDto);
        return Ok(result);
    }

    [HttpPatch("{id}/status")]
    [Authorize(Roles = "Developer")]
    public async Task<ActionResult<BugResponseDto>> UpdateBugStatus(
        int id,
        [FromBody] UpdateBugStatusDto updateStatusDto)
    {
        var developerId = GetUserId();
        var result = await bugService.UpdateBugStatusAsync(id, updateStatusDto.Status, developerId);
        return Ok(result);
    }

    [HttpPatch("{id}/assign")]
    [Authorize(Roles = "User,Developer")]
    public async Task<ActionResult<BugResponseDto>> AssignBug(int id, [FromBody] AssignBugDto? assignBugDto = null)
    {
        var currentUserId = GetUserId();
        var currentUserIsDeveloper = await userService.IsDeveloperAsync(currentUserId);

        string developerId;
        if (!string.IsNullOrEmpty(assignBugDto?.DeveloperId))
        {
            developerId = assignBugDto.DeveloperId;
        }
        else if (currentUserIsDeveloper)
        {
            developerId = currentUserId;
        }
        else
        {
            return BadRequest("Please select a developer to assign this bug to.");
        }

        if (!await userService.IsDeveloperAsync(developerId))
        {
            return BadRequest("The specified user is not a developer.");
        }

        var result = await bugService.AssignBugAsync(id, developerId);
        return Ok(result);
    }

    [HttpPatch("{id}/unassign")]
    [Authorize(Roles = "User,Developer")]
    public async Task<ActionResult<BugResponseDto>> UnassignBug(int id)
    {
        var result = await bugService.UnassignBugAsync(id);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteBug(int id)
    {
        await bugService.DeleteBugAsync(id);
        return NoContent();
    }

    private string GetUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User ID not found in token.");
    }
}
