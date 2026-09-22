using Microsoft.AspNetCore.Mvc;

namespace YourProjectName.Controllers
{
    public class CompanyController : Controller
    {
        public IActionResult Index()
        {
            return View(); // Opens Views/Company/Index.cshtml
        }

        public IActionResult Details()
        {
            return View(); // Opens Views/Company/Details.cshtml
        }
    }
}