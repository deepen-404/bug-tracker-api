using System.ComponentModel.DataAnnotations;
using BugTrackerApi.Models.Enums;

namespace BugTrackerApi.Models.DTOs.Bug;

public class UpdateBugDto
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    public Severity Severity { get; set; }

    public string? ReproductionSteps { get; set; }
}
