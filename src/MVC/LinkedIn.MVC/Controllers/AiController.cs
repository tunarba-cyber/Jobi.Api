using LinkedIn.MVC.Models;
using LinkedIn.MVC.Models.Api;
using LinkedIn.MVC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkedIn.MVC.Controllers;

[Authorize]
public class AiController : Controller
{
    private readonly IJobiApiClient _api;
    public AiController(IJobiApiClient api) => _api = api;

    public sealed record CoverLetterInput(string JobTitle, string? CompanyName, string? JobDescription);

    [Authorize(Roles = AppRoles.Employer), HttpPost]
    public async Task<IActionResult> JobDescription([FromBody] AiJobDescriptionRequest request, CancellationToken ct) =>
        Reply(await _api.GenerateJobDescriptionAsync(request, ct));

    [Authorize(Roles = AppRoles.Candidate), HttpPost]
    public async Task<IActionResult> ImproveBio([FromBody] AiImproveBioRequest request, CancellationToken ct) =>
        Reply(await _api.ImproveBioAsync(request, ct));

    [Authorize(Roles = AppRoles.Candidate), HttpPost]
    public async Task<IActionResult> CoverLetter([FromBody] CoverLetterInput input, CancellationToken ct)
    {
        // The background comes from the candidate's own saved profile, never from the browser.
        var profile = await _api.GetMyCandidateProfileAsync(ct);
        if (profile is null)
            return BadRequest(new { error = "Complete your profile first so the letter can use your background." });

        var background = $"Name: {profile.FullName}. Headline: {profile.Headline}. Experience: {profile.ExperienceLevel}. " +
                         $"Skills: {profile.Skills}. About: {profile.Bio}";

        var description = input.JobDescription is { Length: > 4000 } d ? d[..4000] : input.JobDescription;
        var background2000 = background.Length > 2000 ? background[..2000] : background;

        return Reply(await _api.GenerateCoverLetterAsync(
            new AiCoverLetterRequest(input.JobTitle, input.CompanyName, description, background2000, null), ct));
    }

    private IActionResult Reply(ApiCallResult<AiTextDto> result) =>
        result.Success ? Json(new { text = result.Value!.Text }) : BadRequest(new { error = result.ErrorMessage });
}