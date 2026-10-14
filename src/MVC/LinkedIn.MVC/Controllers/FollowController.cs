using LinkedIn.MVC.Models;
using LinkedIn.MVC.Models.Api;
using LinkedIn.MVC.Models.ViewModels;
using LinkedIn.MVC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkedIn.MVC.Controllers;

[Authorize(Roles = "Candidate,Employer")]
public class FollowController : AppController
{
    private readonly IJobiApiClient _api;
    public FollowController(IJobiApiClient api) => _api = api;

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var summary = _api.GetFollowSummaryAsync(ct);
        var followers = _api.GetFollowersAsync(ct);
        var following = _api.GetFollowingAsync(ct);
        var companies = _api.GetFollowedCompaniesAsync(ct);
        await Task.WhenAll(summary, followers, following, companies);

        return View(new FollowHubViewModel
        {
            Summary = await summary,
            Followers = await followers,
            Following = await following,
            Companies = await companies
        });
    }

    public async Task<IActionResult> Users(string? q, int page = 1, CancellationToken ct = default) =>
        View(new ExploreUsersViewModel { Users = await _api.ExploreUsersAsync(q, page < 1 ? 1 : page, ct), Search = q });

    public async Task<IActionResult> Companies(string? q, int page = 1, CancellationToken ct = default) =>
        View(new ExploreCompaniesViewModel { Companies = await _api.ExploreCompaniesAsync(q, page < 1 ? 1 : page, ct), Search = q });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> FollowUser(string id, string? returnUrl, CancellationToken ct)
    {
        var result = await _api.FollowUserAsync(id, ct);
        if (!result.Success) TempData[FlashKeys.Error] = result.ErrorMessage;
        return LocalReturn(returnUrl, nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UnfollowUser(string id, string? returnUrl, CancellationToken ct)
    {
        var result = await _api.UnfollowUserAsync(id, ct);
        if (!result.Success) TempData[FlashKeys.Error] = result.ErrorMessage;
        return LocalReturn(returnUrl, nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> FollowCompany(long id, string? returnUrl, CancellationToken ct)
    {
        var result = await _api.FollowCompanyAsync(id, ct);
        if (!result.Success) TempData[FlashKeys.Error] = result.ErrorMessage;
        return LocalReturn(returnUrl, nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UnfollowCompany(long id, string? returnUrl, CancellationToken ct)
    {
        var result = await _api.UnfollowCompanyAsync(id, ct);
        if (!result.Success) TempData[FlashKeys.Error] = result.ErrorMessage;
        return LocalReturn(returnUrl, nameof(Index));
    }
}