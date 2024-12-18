using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using WarEntiGox.Models;
using WarEntiGox.Services;  // Kullanıcı doğrulama işlemleri için Service sınıfı ekleyelim.

namespace WarEntiGox.Controllers.MVC
{
    [Route("mvc/[controller]")]
    public class LoginController : Controller
    {
        private readonly UserService _userService;

        // Constructor ile UserService'i alıyoruz
        public LoginController(UserService userService)
        {
            _userService = userService;
        }

        // GET: /mvc/login
        [HttpGet]
        public IActionResult Index()
        {
            // Kullanıcı giriş yapmışsa ana sayfaya yönlendir
            if (HttpContext.Session.GetString("UserId") != null)
            {
                return RedirectToAction("Index", "Home");
            }

            return View(new LoginViewModel());
        }

        // POST: /mvc/login
        [HttpPost]
        public async Task<IActionResult> Index(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Kullanıcı adı ve şifreyi doğrulamak için veritabanı kontrolü yapılabilir
                var user = await _userService.ValidateUserAsync(model.UserName, model.Password);

                if (user != null)
                {
                    // Başarılı giriş sonrası Session'a kullanıcı bilgisini ekleyin
                    HttpContext.Session.SetString("UserId", user.UserName);
                    
                    // Kullanıcı bilgilerini diğer session bilgileriyle de güncelleyebilirsiniz
                    // Örneğin: HttpContext.Session.SetString("Role", user.Role);
                    
                    // Ana sayfaya yönlendirme
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    // Eğer kullanıcı adı veya şifre hatalıysa, hata mesajı ekleyin
                    ModelState.AddModelError(string.Empty, "Geçersiz kullanıcı adı veya şifre");
                }
            }

            return View(model); // Hata varsa tekrar giriş sayfası gösterilecek
        }

        // Logout işlemi ekleyelim
        [HttpGet("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("UserId");  // Kullanıcıyı session'dan çıkartıyoruz
            return RedirectToAction("Index", "Login");  // Login sayfasına yönlendiriyoruz
        }
    }
}
