using LinkedIn.Shared.Abstractions.Primitives;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace LinkedIn.Modules.Jobs.Features.Uploads;

internal static class FileUploadService
{
    public static async Task<Result> SaveAsync(
        IFormFile? file,
        string userId,
        string subfolder,
        HashSet<string> allowedExtensions,
        long maxSizeBytes,
        IWebHostEnvironment env,
        CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return Result.Fail("Upload.Empty", "No file was uploaded.");

        if (file.Length > maxSizeBytes)
            return Result.Fail("Upload.TooLarge", $"File must be {maxSizeBytes / (1024 * 1024)} MB or smaller.");

        var extension = Path.GetExtension(file.FileName);
        if (!allowedExtensions.Contains(extension))
            return Result.Fail("Upload.InvalidType", $"Allowed types: {string.Join(", ", allowedExtensions)}.");

        var head = new byte[12];
        await using (var probe = file.OpenReadStream())
        {
            var read = await probe.ReadAsync(head, ct);
            if (!HasValidSignature(extension, head.AsSpan(0, read)))
                return Result.Fail("Upload.InvalidContent", "The file content doesn't match its extension.");
        }

        // wwwroot/uploads/{subfolder}/{userId}-{guid}.ext - userId prefix makes
        // it easy to spot whose file is whose on disk during development.
        var uploadsFolder = Path.Combine(env.WebRootPath ?? env.ContentRootPath, "uploads", subfolder);
        Directory.CreateDirectory(uploadsFolder);

        var fileName = $"{userId}-{Guid.NewGuid()}{extension}";
        var fullPath = Path.Combine(uploadsFolder, fileName);

        await using (var stream = File.Create(fullPath))
        {
            await file.CopyToAsync(stream, ct);
        }

        return Result.Ok($"/uploads/{subfolder}/{fileName}");
    }

    private static bool HasValidSignature(string ext, ReadOnlySpan<byte> h) => ext.ToLowerInvariant() switch
    {
        ".pdf" => h.StartsWith("%PDF"u8),
        ".png" => h.StartsWith<byte>(new byte[] { 0x89, 0x50, 0x4E, 0x47 }),
        ".jpg" or ".jpeg" => h.StartsWith<byte>(new byte[] { 0xFF, 0xD8, 0xFF }),
        ".webp" => h.Length >= 12 && h[..4].SequenceEqual("RIFF"u8) && h[8..12].SequenceEqual("WEBP"u8),
        ".docx" => h.StartsWith<byte>(new byte[] { 0x50, 0x4B, 0x03, 0x04 }),
        ".doc" => h.StartsWith<byte>(new byte[] { 0xD0, 0xCF, 0x11, 0xE0 }),
        _ => false
    };

    // Tiny local Result type - kept private to this file so it doesn't collide
    // with your existing Shared.Abstractions.Primitives.Result, which carries
    // richer ErrorType info than a simple file-upload check needs.
    internal readonly record struct Result(bool Success, string? Value, string? Code, string? Message)
    {
        public static Result Ok(string value) => new(true, value, null, null);
        public static Result Fail(string code, string message) => new(false, null, code, message);
    }
}