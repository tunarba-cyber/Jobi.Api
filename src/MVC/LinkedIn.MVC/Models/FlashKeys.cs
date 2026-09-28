namespace LinkedIn.MVC.Models;

/// <summary>TempData keys for one-shot messages that must survive a redirect.</summary>
public static class FlashKeys
{
    public const string Success = "FlashSuccess";
    public const string Error = "FlashError";
}