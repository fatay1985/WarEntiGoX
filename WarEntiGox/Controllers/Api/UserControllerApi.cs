using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using WarEntiGox.Models;
using WarEntiGox.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WarEntiGox.Controllers.API
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserControllerApi : ControllerBase
    {
        private readonly UserService _userService;

        public UserControllerApi(UserService userService)
        {
            _userService = userService;
        }

        // Get all users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetAllUsers()
        {
            // Correct method name: GetAllUsersAsync
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        // Get a single user by ID
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUserById(string id)
        {
            if (!ObjectId.TryParse(id, out var objectId))
                return BadRequest("Invalid ID format.");

            var user = await _userService.GetUserByIdAsync(objectId);
            if (user == null)
                return NotFound();

            return Ok(user);
        }

        // Create a new user
        [HttpPost]
        public async Task<ActionResult> CreateUser([FromBody] User user)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            user.CreateDate = DateTime.Now;
            user.IsDeleted = false;

            await _userService.CreateUserAsync(user);

            return CreatedAtAction(nameof(GetUserById), new { id = user.Id.ToString() }, user);
        }

        // Update an existing user
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateUser(string id, [FromBody] User user)
        {
            if (!ObjectId.TryParse(id, out var objectId))
                return BadRequest("Invalid ID format.");

            user.UpdateDate = DateTime.Now;

            await _userService.UpdateUserAsync(objectId, user);

            return NoContent();
        }

        // Soft delete a user
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            if (!ObjectId.TryParse(id, out var objectId))
                return NotFound();

            await _userService.SoftDeleteUserAsync(objectId);
            return NoContent();
        }
    }
}
