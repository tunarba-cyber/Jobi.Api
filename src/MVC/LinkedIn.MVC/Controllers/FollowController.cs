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

    public async Task<IActionResult> Users(string? q, int page = 1, CancellationToken ct = default)
    {
        page = page < 1 ? 1 : page;
        var users = _api.ExploreUsersAsync(q, page, ct);
        var suggestions = string.IsNullOrWhiteSpace(q) && page == 1
            ? _api.GetUserSuggestionsAsync(6, ct)
            : Task.FromResult<IReadOnlyList<SuggestionDto>>(Array.Empty<SuggestionDto>());
        await Task.WhenAll(users, suggestions);
        return View(new ExploreUsersViewModel { Users = await users, Search = q, Suggestions = await suggestions });
    }

    public async Task<IActionResult> Companies(string? q, int page = 1, CancellationToken ct = default)
    {
        page = page < 1 ? 1 : page;
        var companies = _api.ExploreCompaniesAsync(q, page, ct);
        var suggestions = string.IsNullOrWhiteSpace(q) && page == 1
            ? _api.GetCompanySuggestionsAsync(6, ct)
            : Task.FromResult<IReadOnlyList<CompanySuggestionDto>>(Array.Empty<CompanySuggestionDto>());
        await Task.WhenAll(companies, suggestions);
        return View(new ExploreCompaniesViewModel { Companies = await companies, Search = q, Suggestions = await suggestions });
    }

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