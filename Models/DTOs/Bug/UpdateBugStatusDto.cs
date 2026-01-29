using System.ComponentModel.DataAnnotations;
using BugTrackerApi.Models.Enums;

namespace BugTrackerApi.Models.DTOs.Bug;

public class UpdateBugStatusDto
{
    [Required]
    public BugStatus Status { get; set; }
}
