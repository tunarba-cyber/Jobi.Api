using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Security.Claims;

namespace LinkedIn.Modules.Jobs.Features.Uploads;

internal static class UploadEndpoints
{
    private static readonly HashSet<string> DocumentExtensions = new(StringComparer.OrdinalIgnoreCase) { ".pdf", ".doc", ".docx" };
    private static readonly HashSet<string> ImageExtensions = new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".webp" };

    private const long ResumeMaxBytes = 5 * 1024 * 1024; // 5 MB
    private const long ImageMaxBytes = 2 * 1024 * 1024;  // 2 MB

    public static void MapUploadEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/uploads").WithTags("Uploads");

        group.MapPost("/resume", UploadResume)
            .WithName("UploadResume")
            .Accepts<IFormFile>("multipart/form-data")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .RequireAuthorization("RequireCandidate")
            .DisableAntiforgery();

        group.MapPost("/photo", UploadPhoto)
            .WithName("UploadCandidatePhoto")
            .Accepts<IFormFile>("multipart/form-data")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .RequireAuthorization("RequireCandidate")
            .DisableAntiforgery();

        group.MapPost("/logo", UploadLogo)
            .WithName("UploadCompanyLogo")
            .Accepts<IFormFile>("multipart/form-data")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .RequireAuthorization("RequireEmployer")
            .DisableAntiforgery();
    }

    private static async Task<IResult> UploadResume(IFormFile? file, ClaimsPrincipal user, IWebHostEnvironment env, CancellationToken ct) =>
        await HandleUpload(file, user, "resumes", DocumentExtensions, ResumeMaxBytes, env, ct);

    private static async Task<IResult> UploadPhoto(IFormFile? file, ClaimsPrincipal user, IWebHostEnvironment env, CancellationToken ct) =>
        await HandleUpload(file, user, "photos", ImageExtensions, ImageMaxBytes, env, ct);

    private static async Task<IResult> UploadLogo(IFormFile? file, ClaimsPrincipal user, IWebHostEnvironment env, CancellationToken ct) =>
        await HandleUpload(file, user, "logos", ImageExtensions, ImageMaxBytes, env, ct);

    private static async Task<IResult> HandleUpload(
        IFormFile? file, ClaimsPrincipal user, string subfolder,
        HashSet<string> allowedExtensions, long maxBytes, IWebHostEnvironment env, CancellationToken ct)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await FileUploadService.SaveAsync(file, userId, subfolder, allowedExtensions, maxBytes, env, ct);

        return result.Success
            ? Results.Ok(new { url = result.Value })
            : Results.Problem(title: result.Code, detail: result.Message, statusCode: StatusCodes.Status400BadRequest);
    }
}