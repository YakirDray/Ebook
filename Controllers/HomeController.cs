// Controllers/HomeController.cs
using Microsoft.AspNetCore.Mvc;
using MyEBookLibrary.ViewModels;
using System.Diagnostics;

namespace MyEBookLibrary.Controllers
{
    public class HomeController(ILogger<HomeController> logger) : Controller
    {
        private readonly ILogger<HomeController> _logger = logger;

        public IActionResult Index()
        {
            return View();
        }
        public ActionResult New()
        {
            // Fetch new books logic
            return View();
        }

        public ActionResult Popular()
        {
            // Fetch popular books logic
            return View();
        }

        public ActionResult Genres()
        {
            // Fetch or display genres logic
            return View();
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}