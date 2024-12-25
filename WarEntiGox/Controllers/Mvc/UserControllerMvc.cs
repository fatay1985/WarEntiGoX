using Microsoft.AspNetCore.Mvc;
using WarEntiGox.Models;
using WarEntiGox.Services;
using MongoDB.Bson;
using System;
using System.Linq;
using System.Threading.Tasks;

[Route("users")]
public class UserControllerMvc : Controller
{
    private readonly UserService _userService;

    public UserControllerMvc(UserService userService)
    {
        _userService = userService;
    }

    // Kullanıcı Girişi
    [HttpPost("Login")]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        var user = await _userService.ValidateUserAsync(model.UserName, model.Password);

        if (user != null)
        {
            // Kullanıcı doğrulandıysa, CompanyId'yi session'a kaydediyoruz
            HttpContext.Session.SetString("UserId", user.Id.ToString());
            HttpContext.Session.SetInt32("CompanyId", user.CompanyId);

            return RedirectToAction("Index", "Home"); // Ana sayfaya yönlendir
        }

        ModelState.AddModelError(string.Empty, "Geçersiz kullanıcı adı veya şifre.");
        return View(model);
    }
}