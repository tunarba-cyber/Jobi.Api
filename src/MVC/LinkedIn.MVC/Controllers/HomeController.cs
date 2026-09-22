using Microsoft.AspNetCore.Mvc;

namespace YourProjectName.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View(); // Opens Views/Home/Index.cshtml
        }

        public IActionResult AboutUs()
        {
            return View(); // Opens Views/Home/AboutUs.cshtml
        }

        public IActionResult ContactUs()
        {
            return View(); // Opens Views/Home/ContactUs.cshtml
        }

        public IActionResult Faq()
        {
            return View(); // Opens Views/Home/Faq.cshtml
        }
    }
}