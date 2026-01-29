using BugTrackerApi.Models.Enums;

namespace BugTrackerApi.Models.DTOs.Bug;

public class BugListDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public Severity Severity { get; set; }
    public BugStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public string ReporterName { get; set; } = string.Empty;
    public string? AssignedDeveloperName { get; set; }
    public int AttachmentCount { get; set; }
}
