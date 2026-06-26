using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using TeenNovel_WED.Models;

namespace TeenNovel_WED.Controllers
{
    public class Login_RegisterController : Controller
    {
        public IActionResult Index()
        {
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
