using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WED_DOCTRUYEN.Models;

namespace WED_DOCTRUYEN.Controllers
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
