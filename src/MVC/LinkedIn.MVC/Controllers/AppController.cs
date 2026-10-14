using LinkedIn.MVC.Models;
using LinkedIn.MVC.Models.Api;
using Microsoft.AspNetCore.Mvc;

namespace LinkedIn.MVC.Controllers;

public abstract class AppController : Controller
{
    protected IActionResult LocalReturn(string? returnUrl, string fallbackAction, string? fallbackController = null) =>
        Url.IsLocalUrl(returnUrl) ? LocalRedirect(returnUrl!) : RedirectToAction(fallbackAction, fallbackController);

    protected IActionResult FlashAndRedirect(ApiCallResult<bool> result, string successMessage, string action, string? controller = null)
    {
        if (result.Success) TempData[FlashKeys.Success] = successMessage;
        else TempData[FlashKeys.Error] = result.ErrorMessage;
        return RedirectToAction(action, controller);
    }
}