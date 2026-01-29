using BugTrackerApi.Models.Enums;

namespace BugTrackerApi.Models.DTOs.Bug;

public class BugResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Severity Severity { get; set; }
    public BugStatus Status { get; set; }
    public string? ReproductionSteps { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public string ReporterId { get; set; } = string.Empty;
    public string ReporterName { get; set; } = string.Empty;
    public string ReporterEmail { get; set; } = string.Empty;

    public string? AssignedDeveloperId { get; set; }
    public string? AssignedDeveloperName { get; set; }
    public string? AssignedDeveloperEmail { get; set; }

    public List<AttachmentDto> Attachments { get; set; } = new();
}

public class AttachmentDto
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime UploadedAt { get; set; }
}
