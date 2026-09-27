using System.Security.Claims;
using LinkedIn.MVC.Models.Api;
using LinkedIn.MVC.Models.ViewModels;
using LinkedIn.MVC.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace LinkedIn.MVC.Controllers;

public class AccountController : Controller
{
    private readonly IJobiApiClient _api;

    public AccountController(IJobiApiClient api) => _api = api;

    public IActionResult SignUp() => View(new RegisterViewModel());

    [HttpPost]
    public async Task<IActionResult> SignUp(RegisterViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _api.RegisterAsync(new RegisterRequest(
            model.Email, model.Password, model.FirstName, model.LastName, model.Role), ct);

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage!);
            return View(model);
        }

        // Post-Redirect-Get: avoids the "resubmit form?" browser warning that
        // returning View() directly from a POST causes on refresh.
        return RedirectToAction(nameof(RegisterConfirmation));
    }

    public IActionResult RegisterConfirmation() => View();

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model, CancellationToken ct)
    {
        var returnUrl = string.IsNullOrWhiteSpace(model.ReturnUrl) ? "/" : model.ReturnUrl;

        var result = await _api.LoginAsync(new LoginRequest(model.Email, model.Password), ct);
        if (!result.Success)
        {
            // TempData, not ViewBag/ViewData - this is the one mechanism built
            // to survive a redirect, which a login form always does (PRG pattern).
            TempData["LoginError"] = result.ErrorMessage;
            return Redirect(returnUrl);
        }

        var auth = result.Value!;
        var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, auth.UserId));
        identity.AddClaim(new Claim(ClaimTypes.Email, auth.Email));
        identity.AddClaim(new Claim(ClaimTypes.Name, $"{auth.FirstName} {auth.LastName}"));
        identity.AddClaim(new Claim(ClaimTypes.Role, auth.Role.ToString()));

        var properties = new AuthenticationProperties { IsPersistent = model.RememberMe };
        properties.StoreTokens(new[]
        {
            new AuthenticationToken { Name = "access_token", Value = auth.AccessToken },
            new AuthenticationToken { Name = "refresh_token", Value = auth.RefreshToken },
            new AuthenticationToken { Name = "expires_at", Value = auth.AccessTokenExpiresAtUtc.ToString("o") }
        });

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity), properties);
        return Redirect(returnUrl);
    }

    [HttpPost]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        var authResult = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        var refreshToken = authResult.Properties?.GetTokenValue("refresh_token");

        if (refreshToken is not null)
        {
            await _api.LogoutAsync(refreshToken, ct);
        }

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }
    public async Task<IActionResult> ConfirmEmail(string userId, string token, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
        {
            return View(new ConfirmEmailResultViewModel(false, "This confirmation link is invalid."));
        }

        var result = await _api.ConfirmEmailAsync(new ConfirmEmailRequest(userId, token), ct);
        return View(new ConfirmEmailResultViewModel(result.Success, result.ErrorMessage));
    }
}