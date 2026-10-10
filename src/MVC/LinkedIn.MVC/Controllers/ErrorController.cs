using LinkedIn.MVC.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace LinkedIn.MVC.Controllers;

public class ErrorController : Controller
{
    // Hit for 404/403/etc. via UseStatusCodePagesWithReExecute
    [Route("Error/{code:int}")]
    public IActionResult Status(int code)
    {
        Response.StatusCode = code;

        var (title, message) = code switch
        {
            404 => ("Page Not Found", "We can't find what you're looking for. Try the homepage or search for jobs."),
            403 => ("Access Denied", "You don't have permission to view this page."),
            401 => ("Please Log In", "You need to log in to continue."),
            _ => ("Something Went Wrong", "An unexpected error occurred. Please try again.")
        };

        return View("Status", new StatusErrorViewModel(code, title, message));
    }

    // Hit for unhandled exceptions via UseExceptionHandler
    [Route("Error")]
    public IActionResult Index()
    {
        Response.StatusCode = 500;
        return View("Status", new StatusErrorViewModel(500, "Server Error",
            "Something broke on our side. We've logged it, please try again in a moment."));
    }
}