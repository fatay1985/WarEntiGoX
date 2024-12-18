using Microsoft.AspNetCore.Mvc;

namespace WarEntiGox.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            // Giriş yapılmamışsa Login sayfasına yönlendir
            if (HttpContext.Session.GetString("UserId") == null)
            {
                return RedirectToAction("Index", "Login");
            }
            return View();
        }
    }
}