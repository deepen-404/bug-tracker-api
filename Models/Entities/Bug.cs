using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BugTrackerApi.Models.Enums;

namespace BugTrackerApi.Models.Entities;

public class Bug
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    public Severity Severity { get; set; } = Severity.Medium;

    public BugStatus Status { get; set; } = BugStatus.Open;

    public string? ReproductionSteps { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    [Required]
    public string ReporterId { get; set; } = string.Empty;

    [ForeignKey(nameof(ReporterId))]
    public ApplicationUser ReporterInfo { get; set; } = null!;

    public string? AssignedDeveloperId { get; set; }

    [ForeignKey(nameof(AssignedDeveloperId))]
    public ApplicationUser? AssignedDeveloperInfo { get; set; }

    public ICollection<BugAttachment> Attachments { get; set; } = [];
}
