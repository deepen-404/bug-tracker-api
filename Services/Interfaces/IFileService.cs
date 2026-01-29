using BugTrackerApi.Models.DTOs.Bug;

namespace BugTrackerApi.Services.Interfaces;

public interface IFileService
{
    Task<AttachmentDto> UploadAttachmentAsync(int bugId, IFormFile file);
    Task<(Stream FileStream, string ContentType, string FileName)> GetAttachmentAsync(int attachmentId);
    Task DeleteAttachmentAsync(int attachmentId);
}

