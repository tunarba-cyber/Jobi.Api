using LinkedIn.MVC.Models;
using LinkedIn.MVC.Models.Api;
using LinkedIn.MVC.Models.ViewModels;
using LinkedIn.MVC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkedIn.MVC.Controllers;

[Authorize]
public class PostsController : Controller
{
    private readonly IJobiApiClient _api;
    public PostsController(IJobiApiClient api) => _api = api;

    public async Task<IActionResult> Index(string? scope, string? authorId, int page = 1, CancellationToken ct = default)
    {
        scope = scope == "following" ? "following" : "all";
        page = page < 1 ? 1 : page;

        var posts = _api.GetPostsAsync(scope, authorId, page, ct);
        var suggestions = User.IsInRole("Admin")
            ? Task.FromResult<IReadOnlyList<SuggestionDto>>(Array.Empty<SuggestionDto>())
            : _api.GetUserSuggestionsAsync(5, ct);
        await Task.WhenAll(posts, suggestions);

        return View(new PostsFeedViewModel { Posts = await posts, Scope = scope, AuthorId = authorId, Suggestions = await suggestions });
    }

    public async Task<IActionResult> Details(long id, CancellationToken ct)
    {
        var post = await _api.GetPostAsync(id, ct);
        if (post is null) return NotFound();
        return View(new PostDetailsViewModel { Post = post, Comments = await _api.GetCommentsAsync(id, ct) });
    }

    [Authorize(Roles = "Candidate,Employer"), HttpPost]
    public async Task<IActionResult> Create(PostFormModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            TempData[FlashKeys.Error] = ModelState.Values.SelectMany(v => v.Errors).First().ErrorMessage;
            return RedirectToAction(nameof(Index));
        }

        string? imageUrl = null;
        if (model.Image is { Length: > 0 })
        {
            await using var stream = model.Image.OpenReadStream();
            var upload = await _api.UploadPostImageAsync(stream, model.Image.FileName, model.Image.ContentType, ct);
            if (!upload.Success) { TempData[FlashKeys.Error] = upload.ErrorMessage; return RedirectToAction(nameof(Index)); }
            imageUrl = upload.Value;
        }

        var result = await _api.CreatePostAsync(new CreatePostRequest(model.Caption, imageUrl), ct);
        if (result.Success) TempData[FlashKeys.Success] = "Posted.";
        else TempData[FlashKeys.Error] = result.ErrorMessage;
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Candidate,Employer"), HttpPost]
    public async Task<IActionResult> Like(long id, string? returnUrl, CancellationToken ct)
    {
        var r = await _api.LikePostAsync(id, ct);
        if (!r.Success) TempData[FlashKeys.Error] = r.ErrorMessage;
        return Back(returnUrl);
    }

    [HttpPost]
    public async Task<IActionResult> Unlike(long id, string? returnUrl, CancellationToken ct)
    {
        var r = await _api.UnlikePostAsync(id, ct);
        if (!r.Success) TempData[FlashKeys.Error] = r.ErrorMessage;
        return Back(returnUrl);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(long id, string? returnUrl, CancellationToken ct)
    {
        var r = await _api.DeletePostAsync(id, ct);
        if (r.Success) TempData[FlashKeys.Success] = "Post deleted.";
        else TempData[FlashKeys.Error] = r.ErrorMessage;
        return Back(returnUrl);
    }

    [Authorize(Roles = "Candidate,Employer"), HttpPost]
    public async Task<IActionResult> Comment(long id, string? text, CancellationToken ct)
    {
        var r = await _api.AddCommentAsync(id, text ?? "", ct);
        if (!r.Success) TempData[FlashKeys.Error] = r.ErrorMessage;
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    public async Task<IActionResult> DeleteComment(long commentId, long postId, CancellationToken ct)
    {
        var r = await _api.DeleteCommentAsync(commentId, ct);
        if (!r.Success) TempData[FlashKeys.Error] = r.ErrorMessage;
        return RedirectToAction(nameof(Details), new { id = postId });
    }

    // Local URLs only. A deleted post's own page would 404, so those go back to the feed.
    private IActionResult Back(string? returnUrl) =>
        Url.IsLocalUrl(returnUrl) && !returnUrl!.StartsWith("/Posts/Details", StringComparison.OrdinalIgnoreCase)
            ? LocalRedirect(returnUrl)
            : RedirectToAction(nameof(Index));
}