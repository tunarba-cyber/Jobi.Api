using Microsoft.AspNetCore.Mvc;

namespace YourProjectName.Controllers
{
    public class JobController : Controller
    {
        public IActionResult JobList()
        {
            return View(); // Opens Views/Job/JobList.cshtml
        }

        public IActionResult JobGrid()
        {
            return View(); // Opens Views/Job/JobGrid.cshtml
        }

        public IActionResult JobDetails()
        {
            return View(); // Opens Views/Job/JobDetails.cshtml
        }
    }
}