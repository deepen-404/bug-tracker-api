using BugTrackerApi.Data;
using BugTrackerApi.Exceptions;
using BugTrackerApi.Models.DTOs.Bug;
using BugTrackerApi.Models.DTOs.Common;
using BugTrackerApi.Models.Entities;
using BugTrackerApi.Models.Enums;
using BugTrackerApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BugTrackerApi.Services.Implementations;

public class BugService(ApplicationDbContext context) : IBugService
{
    public async Task<BugResponseDto> CreateBugAsync(CreateBugDto createBugDto, string userId)
    {
        var bug = new Bug
        {
            Title = createBugDto.Title,
            Description = createBugDto.Description,
            Severity = createBugDto.Severity,
            ReproductionSteps = createBugDto.ReproductionSteps,
            ReporterId = userId,
            Status = BugStatus.Open,
            CreatedAt = DateTime.UtcNow
        };

        context.Bugs.Add(bug);
        await context.SaveChangesAsync();

        return await GetBugByIdAsync(bug.Id);
    }

    public async Task<BugResponseDto> GetBugByIdAsync(int id)
    {
        var bug = await context.Bugs
            .Include(b => b.ReporterInfo)
            .Include(b => b.AssignedDeveloperInfo)
            .Include(b => b.Attachments)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (bug == null)
        {
            throw new NotFoundException($"Bug with ID {id} not found.");
        }

        return MapToBugResponseDto(bug);
    }

    public async Task<PaginatedResponse<BugListDto>> GetBugsByReporterAsync(string userId, PaginationParams pagination)
    {
        var query = context.Bugs
            .Include(b => b.ReporterInfo)
            .Include(b => b.AssignedDeveloperInfo)
            .Include(b => b.Attachments)
            .Where(b => b.ReporterId == userId)
            .OrderByDescending(b => b.CreatedAt);

        var totalCount = await query.CountAsync();

        var bugs = await query
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync();

        return new PaginatedResponse<BugListDto>
        {
            Items = bugs.Select(MapToBugListDto),
            PageNumber = pagination.PageNumber,
            PageSize = pagination.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<PaginatedResponse<BugListDto>> GetAssignedBugsAsync(string developerId, PaginationParams pagination)
    {
        var query = context.Bugs
            .Include(b => b.ReporterInfo)
            .Include(b => b.AssignedDeveloperInfo)
            .Include(b => b.Attachments)
            .Where(b => b.AssignedDeveloperId == developerId)
            .OrderByDescending(b => b.CreatedAt);

        var totalCount = await query.CountAsync();

        var bugs = await query
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync();

        return new PaginatedResponse<BugListDto>
        {
            Items = bugs.Select(MapToBugListDto),
            PageNumber = pagination.PageNumber,
            PageSize = pagination.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<PaginatedResponse<BugListDto>> GetUnassignedBugsAsync(PaginationParams pagination, string? searchTerm = null)
    {
        var query = context.Bugs
            .Include(b => b.ReporterInfo)
            .Include(b => b.Attachments)
            .Where(b => b.AssignedDeveloperId == null);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(b =>
                b.Title.Contains(searchTerm) ||
                b.Description.Contains(searchTerm));
        }

        var orderedQuery = query.OrderByDescending(b => b.CreatedAt);
        var totalCount = await query.CountAsync();

        var bugs = await orderedQuery
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync();

        return new PaginatedResponse<BugListDto>
        {
            Items = bugs.Select(MapToBugListDto),
            PageNumber = pagination.PageNumber,
            PageSize = pagination.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<BugResponseDto> AssignBugAsync(int bugId, string developerId)
    {
        var bug = await context.Bugs.FindAsync(bugId);
        if (bug == null)
        {
            throw new NotFoundException($"Bug with ID {bugId} not found.");
        }

        bug.AssignedDeveloperId = developerId;

        if (bug.Status == BugStatus.Open)
        {
            bug.Status = BugStatus.InProgress;
        }

        bug.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();

        return await GetBugByIdAsync(bugId);
    }

    public async Task<BugResponseDto> UnassignBugAsync(int bugId)
    {
        var bug = await context.Bugs.FindAsync(bugId);
        if (bug == null)
        {
            throw new NotFoundException($"Bug with ID {bugId} not found.");
        }

        bug.AssignedDeveloperId = null;
        bug.Status = BugStatus.Open;
        bug.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();

        return await GetBugByIdAsync(bugId);
    }

    public async Task<BugResponseDto> UpdateBugStatusAsync(int bugId, BugStatus status, string developerId)
    {
        var bug = await context.Bugs.FindAsync(bugId);
        if (bug == null)
        {
            throw new NotFoundException($"Bug with ID {bugId} not found.");
        }

        if (bug.AssignedDeveloperId != developerId)
        {
            throw new ForbiddenException("You can only update the status of bugs assigned to you.");
        }

        bug.Status = status;
        bug.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();

        return await GetBugByIdAsync(bugId);
    }

    public async Task<BugResponseDto> UpdateBugAsync(int bugId, UpdateBugDto updateBugDto)
    {
        var bug = await context.Bugs.FindAsync(bugId);
        if (bug == null)
        {
            throw new NotFoundException($"Bug with ID {bugId} not found.");
        }

        bug.Title = updateBugDto.Title;
        bug.Description = updateBugDto.Description;
        bug.Severity = updateBugDto.Severity;
        bug.ReproductionSteps = updateBugDto.ReproductionSteps;
        bug.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();

        return await GetBugByIdAsync(bugId);
    }

    public async Task DeleteBugAsync(int bugId)
    {
        var bug = await context.Bugs.FindAsync(bugId);
        if (bug == null)
        {
            throw new NotFoundException($"Bug with ID {bugId} not found.");
        }

        context.Bugs.Remove(bug);
        await context.SaveChangesAsync();
    }

    public async Task<PaginatedResponse<BugListDto>> SearchBugsAsync(
        PaginationParams pagination,
        string? searchTerm,
        Severity? severity,
        BugStatus? status)
    {
        var query = context.Bugs
            .Include(b => b.ReporterInfo)
            .Include(b => b.AssignedDeveloperInfo)
            .Include(b => b.Attachments)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(b =>
                b.Title.Contains(searchTerm) ||
                b.Description.Contains(searchTerm));
        }

        if (severity.HasValue)
        {
            query = query.Where(b => b.Severity == severity.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(b => b.Status == status.Value);
        }

        var totalCount = await query.CountAsync();

        var bugs = await query
            .OrderByDescending(b => b.CreatedAt)
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync();

        return new PaginatedResponse<BugListDto>
        {
            Items = bugs.Select(MapToBugListDto),
            PageNumber = pagination.PageNumber,
            PageSize = pagination.PageSize,
            TotalCount = totalCount
        };
    }

    private static BugResponseDto MapToBugResponseDto(Bug bug)
    {
        return new BugResponseDto
        {
            Id = bug.Id,
            Title = bug.Title,
            Description = bug.Description,
            Severity = bug.Severity,
            Status = bug.Status,
            ReproductionSteps = bug.ReproductionSteps,
            CreatedAt = bug.CreatedAt,
            UpdatedAt = bug.UpdatedAt,
            ReporterId = bug.ReporterId,
            ReporterName = bug.ReporterInfo.FullName,
            ReporterEmail = bug.ReporterInfo.Email!,
            AssignedDeveloperId = bug.AssignedDeveloperId,
            AssignedDeveloperName = bug.AssignedDeveloperInfo?.FullName,
            AssignedDeveloperEmail = bug.AssignedDeveloperInfo?.Email,
            Attachments = bug.Attachments.Select(a => new AttachmentDto
            {
                Id = a.Id,
                FileName = a.FileName,
                ContentType = a.ContentType,
                FileSize = a.FileSize,
                UploadedAt = a.UploadedAt
            }).ToList()
        };
    }

    private static BugListDto MapToBugListDto(Bug bug)
    {
        return new BugListDto
        {
            Id = bug.Id,
            Title = bug.Title,
            Severity = bug.Severity,
            Status = bug.Status,
            CreatedAt = bug.CreatedAt,
            ReporterName = bug.ReporterInfo.FullName,
            AssignedDeveloperName = bug.AssignedDeveloperInfo?.FullName,
            AttachmentCount = bug.Attachments.Count
        };
    }
}
