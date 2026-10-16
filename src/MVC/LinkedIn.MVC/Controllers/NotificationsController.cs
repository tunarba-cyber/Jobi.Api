using LinkedIn.MVC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkedIn.MVC.Controllers;

[Authorize]
public class NotificationsController : Controller
{
    private readonly IJobiApiClient _api;
    public NotificationsController(IJobiApiClient api) => _api = api;

    public async Task<IActionResult> Index(int page = 1, CancellationToken ct = default) =>
        View(await _api.GetNotificationsAsync(page < 1 ? 1 : page, ct));

    [HttpGet, ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> Count(CancellationToken ct) =>
        Json(new { count = await _api.GetUnreadNotificationCountAsync(ct) });

    // Marks one as read, then follows its link. A POST so that opening a page can't change state by accident.
    [HttpPost]
    public async Task<IActionResult> Open(long id, string? link, CancellationToken ct)
    {
        await _api.MarkNotificationReadAsync(id, ct);
        return Url.IsLocalUrl(link) ? LocalRedirect(link!) : RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> MarkAllRead(CancellationToken ct)
    {
        await _api.MarkAllNotificationsReadAsync(ct);
        return RedirectToAction(nameof(Index));
    }
}