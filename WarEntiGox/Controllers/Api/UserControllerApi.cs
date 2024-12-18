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

        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetAllUsers()
        {
            var users = await _userService.GetUsersAsync();
            return Ok(users);
        }

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

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateUser(string id, [FromBody] User user)
        {
            if (!ObjectId.TryParse(id, out var objectId))
                return BadRequest("Invalid ID format.");

            user.UpdateDate = DateTime.Now;

            await _userService.UpdateUserAsync(objectId, user);

            return NoContent();
        }

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
