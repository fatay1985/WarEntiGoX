using Microsoft.AspNetCore.Mvc;
using WarEntiGox.Models;
using WarEntiGox.Services;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
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

    // User listing with search functionality
    [HttpGet]
    public async Task<IActionResult> Index(string searchTerm)
    {
        ViewData["CurrentFilter"] = searchTerm;

        var users = await _userService.GetAllUsersAsync() ?? new List<User>();

        if (!string.IsNullOrEmpty(searchTerm))
        {
            users = users.Where(u => u.UserName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        return View("~/Views/User/Index.cshtml", users); // Pass users list to the view
    }

    // User creation page
    [HttpGet("Create")]
    public IActionResult Create()
    {
        return View("~/Views/User/Create.cshtml", new User()); // Pass a new empty user model to the view
    }

    // User creation action
    [HttpPost("Create")]
    public async Task<IActionResult> Create(User user)
    {
        if (!ModelState.IsValid)
        {
            ModelState.AddModelError(string.Empty, "Please ensure all fields are filled correctly.");
            return View("~/Views/User/Create.cshtml", user);
        }

        try
        {
            user.CreateDate = DateTime.Now;
            user.UpdateDate = DateTime.Now;
            user.IsDeleted = false;

            await _userService.CreateUserAsync(user);
            return RedirectToAction(nameof(Index)); // Redirect to the user listing page
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
            return View("~/Views/User/Create.cshtml", user); // Return to the create view with error message
        }
    }

    // Edit user page
    [HttpGet("Edit/{id}")]
    public async Task<IActionResult> Edit(string id)
    {
        if (!ObjectId.TryParse(id, out var objectId))
        {
            return NotFound(); // Return NotFound if id is invalid
        }

        var user = await _userService.GetUserByIdAsync(objectId);
        if (user == null)
        {
            return NotFound(); // Return NotFound if user doesn't exist
        }

        return View("~/Views/User/Edit.cshtml", user); // Pass the user details to the view
    }

    // Edit user action
    [HttpPost("Edit/{id}")]
    public async Task<IActionResult> Edit(string id, User user)
    {
        if (!ObjectId.TryParse(id, out var objectId))
        {
            return NotFound(); // Return NotFound if id is invalid
        }

        if (objectId != user.Id)
        {
            return NotFound(); // Return NotFound if user ID doesn't match
        }

        if (!ModelState.IsValid)
        {
            ModelState.AddModelError(string.Empty, "Please ensure all fields are filled correctly.");
            return View("~/Views/User/Edit.cshtml", user); // Return to the edit view with error message
        }

        try
        {
            user.UpdateDate = DateTime.Now;
            await _userService.UpdateUserAsync(objectId, user);
            return RedirectToAction(nameof(Index)); // Redirect to the user listing page after successful update
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
            return View("~/Views/User/Edit.cshtml", user); // Return to the edit view with error message
        }
    }

    // Delete user page
    [HttpGet("Delete/{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        if (!ObjectId.TryParse(id, out var objectId))
        {
            return NotFound(); // Return NotFound if id is invalid
        }

        var user = await _userService.GetUserByIdAsync(objectId);
        if (user == null)
        {
            return NotFound(); // Return NotFound if user doesn't exist
        }

        return View("~/Views/User/Delete.cshtml", user); // Pass the user details to the delete confirmation view
    }

    // Confirm delete user action
    [HttpPost("Delete/{id}")]
    [ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(string id)
    {
        if (!ObjectId.TryParse(id, out var objectId))
        {
            return NotFound(); // Return NotFound if id is invalid
        }

        try
        {
            await _userService.SoftDeleteUserAsync(objectId);
            return RedirectToAction(nameof(Index)); // Redirect to the user listing page after successful deletion
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
            return RedirectToAction(nameof(Index)); // Redirect to the user listing page in case of error
        }
    }
}
