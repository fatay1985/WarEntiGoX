using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using WarEntiGox.Models;
using WarEntiGox.Services;

namespace WarEntiGox.Controllers.MVC
{
    [Route("mvc/[controller]")]
    public class LoginController : Controller
    {
        private readonly UserService _userService;

        public LoginController(UserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            // Kullanıcı giriş yapmışsa, Home sayfasına yönlendir
            if (HttpContext.Session.GetString("UserId") != null)
            {
                return RedirectToAction("Index", "Home");
            }

            var model = new LoginViewModel();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Index(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Kullanıcı doğrulama işlemi
                var user = await _userService.ValidateUserAsync(model.UserName, model.Password);

                if (user != null)
                {
                    HttpContext.Session.SetString("UserId", user.UserName);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Geçersiz kullanıcı adı veya şifre");
                }
            }

            return View(model); // Modeli tekrar döndür
        }

        [HttpGet("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("UserId");
            return RedirectToAction("Index", "Login");
        }
    }
}
