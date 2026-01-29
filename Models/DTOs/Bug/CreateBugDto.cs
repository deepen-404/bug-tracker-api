using System.ComponentModel.DataAnnotations;
using BugTrackerApi.Models.Enums;

namespace BugTrackerApi.Models.DTOs.Bug;

public class CreateBugDto
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    public Severity Severity { get; set; } = Severity.Medium;

    public string? ReproductionSteps { get; set; }
}
