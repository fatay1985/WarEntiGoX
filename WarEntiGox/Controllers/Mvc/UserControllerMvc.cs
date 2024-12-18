using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using WarEntiGox.Models;
using WarEntiGox.Services;

[Route("mvc/[controller]")]
public class UserControllerMvc : Controller
{
    private readonly UserService _userService;

    public UserControllerMvc(UserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var companyId = HttpContext.Session.GetInt32("CompanyId");
        if (companyId == null)
        {
            return RedirectToAction("Index", "Login"); // Kullanıcı oturum açmamışsa giriş sayfasına yönlendir
        }

        var users = await _userService.GetUsersAsync();

        // Sadece oturum açan kullanıcının şirketine ait veriler
        users = users.Where(u => u.CompanyId == companyId).ToList();

        return View(users);
    }

    [HttpGet("Create")]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost("Create")]
    public async Task<IActionResult> Create(User user)
    {
        if (ModelState.IsValid)
        {
            user.CreateDate = DateTime.Now;
            user.IsDeleted = false;
            user.CompanyId = HttpContext.Session.GetInt32("CompanyId").Value; // Oturumdaki CompanyId'yi al
            await _userService.CreateUserAsync(user);
            return RedirectToAction(nameof(Index));
        }
        return View(user);
    }

    [HttpGet("Edit/{id}")]
    public async Task<IActionResult> Edit(string id)
    {
        if (!ObjectId.TryParse(id, out var objectId))
            return NotFound();

        var user = await _userService.GetUserByIdAsync(objectId);
        if (user == null)
            return NotFound();

        return View(user);
    }

    [HttpPost("Edit/{id}")]
    public async Task<IActionResult> Edit(string id, User user)
    {
        if (!ObjectId.TryParse(id, out var objectId))
            return NotFound();

        if (ModelState.IsValid)
        {
            await _userService.UpdateUserAsync(objectId, user);
            return RedirectToAction(nameof(Index));
        }

        return View(user);
    }

    [HttpGet("Delete/{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        if (!ObjectId.TryParse(id, out var objectId))
            return NotFound();

        var user = await _userService.GetUserByIdAsync(objectId);
        if (user == null)
            return NotFound();

        return View(user);
    }

    [HttpPost("Delete/{id}")]
    public async Task<IActionResult> DeleteConfirmed(string id)
    {
        if (!ObjectId.TryParse(id, out var objectId))
            return NotFound();

        await _userService.SoftDeleteUserAsync(objectId);
        return RedirectToAction(nameof(Index));
    }
}
