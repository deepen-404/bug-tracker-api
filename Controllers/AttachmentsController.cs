using BugTrackerApi.Models.DTOs.Bug;
using BugTrackerApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BugTrackerApi.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class AttachmentsController(IFileService fileService) : ControllerBase
{
    [HttpPost("bugs/{bugId}/attachments")]
    public async Task<ActionResult<AttachmentDto>> UploadAttachment(int bugId, IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file provided.");
        }

        var result = await fileService.UploadAttachmentAsync(bugId, file);
        return CreatedAtAction(nameof(DownloadAttachment), new { id = result.Id }, result);
    }

    [HttpGet("attachments/{id}")]
    public async Task<ActionResult> DownloadAttachment(int id)
    {
        var (stream, contentType, fileName) = await fileService.GetAttachmentAsync(id);
        return File(stream, contentType, fileName);
    }

    [HttpDelete("attachments/{id}")]
    public async Task<ActionResult> DeleteAttachment(int id)
    {
        await fileService.DeleteAttachmentAsync(id);
        return NoContent();
    }
}
