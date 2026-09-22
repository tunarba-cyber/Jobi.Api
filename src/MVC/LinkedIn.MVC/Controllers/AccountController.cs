using Microsoft.AspNetCore.Mvc;

namespace YourProjectName.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult SignUp()
        {
            return View(); // Opens Views/Account/SignUp.cshtml
        }
    }
}