using BugTrackerApi.Models.DTOs.Bug;
using BugTrackerApi.Models.DTOs.Common;
using BugTrackerApi.Models.Enums;

namespace BugTrackerApi.Services.Interfaces;

public interface IBugService
{
    Task<BugResponseDto> CreateBugAsync(CreateBugDto createBugDto, string userId);
    Task<BugResponseDto> GetBugByIdAsync(int id);
    Task<PaginatedResponse<BugListDto>> GetBugsByReporterAsync(string userId, PaginationParams pagination);
    Task<PaginatedResponse<BugListDto>> GetAssignedBugsAsync(string developerId, PaginationParams pagination);
    Task<PaginatedResponse<BugListDto>> GetUnassignedBugsAsync(PaginationParams pagination, string? searchTerm = null);
    Task<BugResponseDto> AssignBugAsync(int bugId, string developerId);
    Task<BugResponseDto> UnassignBugAsync(int bugId);
    Task<BugResponseDto> UpdateBugStatusAsync(int bugId, BugStatus status, string developerId);
    Task<BugResponseDto> UpdateBugAsync(int bugId, UpdateBugDto updateBugDto);
    Task DeleteBugAsync(int bugId);
    Task<PaginatedResponse<BugListDto>> SearchBugsAsync(PaginationParams pagination, string? searchTerm, Severity? severity, BugStatus? status);
}
