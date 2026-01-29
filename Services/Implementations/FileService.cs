using BugTrackerApi.Data;
using BugTrackerApi.Exceptions;
using BugTrackerApi.Models.DTOs.Bug;
using BugTrackerApi.Models.Entities;
using BugTrackerApi.Services.Interfaces;

namespace BugTrackerApi.Services.Implementations;

public class FileService(
    ApplicationDbContext context,
    IConfiguration configuration,
    IWebHostEnvironment environment) : IFileService
{
    public async Task<AttachmentDto> UploadAttachmentAsync(int bugId, IFormFile file)
    {
        var bug = await context.Bugs.FindAsync(bugId);
        if (bug == null)
        {
            throw new NotFoundException($"Bug with ID {bugId} not found.");
        }

        var maxSizeInMB = configuration.GetValue<int>("FileStorage:MaxFileSizeInMB");
        var maxSizeInBytes = maxSizeInMB * 1024 * 1024;
        if (file.Length > maxSizeInBytes)
        {
            throw new BadRequestException($"File size exceeds the maximum allowed size of {maxSizeInMB}MB.");
        }

        var allowedExtensions = configuration.GetSection("FileStorage:AllowedExtensions").Get<string[]>();
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (allowedExtensions != null && !allowedExtensions.Contains(extension))
        {
            throw new BadRequestException($"File type '{extension}' is not allowed.");
        }

        var uploadPath = configuration["FileStorage:UploadPath"] ?? "wwwroot/uploads";
        var fullUploadPath = Path.Combine(environment.ContentRootPath, uploadPath, bugId.ToString());
        Directory.CreateDirectory(fullUploadPath);

        var uniqueFileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(fullUploadPath, uniqueFileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var attachment = new BugAttachment
        {
            BugId = bugId,
            FileName = file.FileName,
            FilePath = Path.Combine(uploadPath, bugId.ToString(), uniqueFileName),
            ContentType = file.ContentType,
            FileSize = file.Length,
            UploadedAt = DateTime.UtcNow
        };

        context.BugAttachments.Add(attachment);
        await context.SaveChangesAsync();

        return new AttachmentDto
        {
            Id = attachment.Id,
            FileName = attachment.FileName,
            ContentType = attachment.ContentType,
            FileSize = attachment.FileSize,
            UploadedAt = attachment.UploadedAt
        };
    }

    public async Task<(Stream FileStream, string ContentType, string FileName)> GetAttachmentAsync(int attachmentId)
    {
        var attachment = await context.BugAttachments.FindAsync(attachmentId);
        if (attachment == null)
        {
            throw new NotFoundException($"Attachment with ID {attachmentId} not found.");
        }

        var fullPath = Path.Combine(environment.ContentRootPath, attachment.FilePath);
        if (!File.Exists(fullPath))
        {
            throw new NotFoundException("File not found on disk.");
        }

        var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
        return (stream, attachment.ContentType, attachment.FileName);
    }

    public async Task DeleteAttachmentAsync(int attachmentId)
    {
        var attachment = await context.BugAttachments.FindAsync(attachmentId);
        if (attachment == null)
        {
            throw new NotFoundException($"Attachment with ID {attachmentId} not found.");
        }

        var fullPath = Path.Combine(environment.ContentRootPath, attachment.FilePath);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        context.BugAttachments.Remove(attachment);
        await context.SaveChangesAsync();
    }
}
