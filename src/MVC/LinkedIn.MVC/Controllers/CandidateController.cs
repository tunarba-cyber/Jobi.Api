using Microsoft.AspNetCore.Mvc;

namespace YourProjectName.Controllers
{
    public class CandidateController : Controller
    {
        public IActionResult Index()
        {
            return View(); // Opens Views/Candidate/Index.cshtml
        }

        public IActionResult Profile()
        {
            return View(); // Opens Views/Candidate/Profile.cshtml
        }
    }
}