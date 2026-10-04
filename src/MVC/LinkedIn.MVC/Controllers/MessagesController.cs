using LinkedIn.MVC.Models.Api;
using LinkedIn.MVC.Models.ViewModels;
using LinkedIn.MVC.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace LinkedIn.MVC.Controllers;

[Authorize]
public class MessagesController : Controller
{
    private readonly IJobiApiClient _api;
    private readonly IOptions<JobiApiOptions> _apiOptions;
    public MessagesController(IJobiApiClient api, IOptions<JobiApiOptions> apiOptions)
    {
        _api = api;
        _apiOptions = apiOptions;
    }

    public async Task<IActionResult> Index(CancellationToken ct) =>
        View(new ConversationsViewModel { Items = await _api.GetMyConversationsAsync(ct) });

    public async Task<IActionResult> Thread(long id, CancellationToken ct)
    {
        var messages = await _api.GetConversationMessagesAsync(id, ct);
        var other = (await _api.GetMyConversationsAsync(ct)).FirstOrDefault(c => c.Id == id)?.OtherUserId ?? "";
        ViewBag.ApiBaseUrl = _apiOptions.Value.BaseUrl.TrimEnd('/');
        ViewBag.AccessToken = HttpContext.GetTokenAsync("access_token").Result;
        return View(new ConversationViewModel { ConversationId = id, OtherUserId = other, Messages = messages });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Send(string recipientUserId, string content, CancellationToken ct)
    {
        var result = await _api.SendMessageAsync(new SendMessageRequest(recipientUserId, content), ct);
        if (result.Success)
        {
            var conversations = await _api.GetMyConversationsAsync(ct);
            var convoId = conversations.FirstOrDefault(c => c.OtherUserId == recipientUserId)?.Id;
            if (convoId is not null) return RedirectToAction(nameof(Thread), new { id = convoId });
        }
        return RedirectToAction(nameof(Index));
    }
    [HttpGet]
    public async Task<IActionResult> SearchUsers(string q, CancellationToken ct) =>
    Json(await _api.SearchUsersAsync(q, ct));
}