using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ToolTrackingSystem.API.Models.Entities;
using ToolTrackingSystem.API.Repositories;
using ToolTrackingSystem.API.Models.DTOs.Users;
using Microsoft.AspNetCore.Authorization;

namespace ToolTrackingSystem.API.Controllers
{
    //[Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<UsersController> _logger;

        public UsersController(
            IUserRepository userRepository,
            UserManager<ApplicationUser> userManager,
            ILogger<UsersController> logger)
        {
            _userRepository = userRepository;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: api/users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers()
        {
            try
            {
                var users = await _userRepository.GetAllAsync();
                var userDtos = users.Select(u => new UserDto
                {
                    Id = u.Id.ToString(),
                    Email = u.Email,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    UserName = u.UserName
                });

                return Ok(userDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all users");
                return StatusCode(500, "Internal server error");
            }
        }

        // GET: api/users/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDetailDto>> GetUserById(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                var roles = await _userManager.GetRolesAsync(user);

                var userDetailDto = new UserDetailDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    UserName = user.UserName,
                    PhoneNumber = user.PhoneNumber,
                    Roles = roles.ToList()
                };

                return Ok(userDetailDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting user with ID {id}");
                return StatusCode(500, "Internal server error");
            }
        }

        // POST: api/users
        [HttpPost]
        public async Task<ActionResult<UserDto>> CreateUser([FromBody] CreateUserDto createUserDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var user = new ApplicationUser
                {
                    UserName = createUserDto.Email,
                    Email = createUserDto.Email,
                    FirstName = createUserDto.FirstName,
                    LastName = createUserDto.LastName
                };

                var result = await _userManager.CreateAsync(user, createUserDto.Password);

                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(error.Code, error.Description);
                    }
                    return BadRequest(ModelState);
                }

                // Add user to roles if specified
                if (createUserDto.Roles != null && createUserDto.Roles.Any())
                {
                    foreach (var role in createUserDto.Roles)
                    {
                        if (!await _userRepository.RoleExistsAsync(role))
                        {
                            return BadRequest($"Role '{role}' does not exist");
                        }
                    }

                    var roleResult = await _userManager.AddToRolesAsync(user, createUserDto.Roles);
                    if (!roleResult.Succeeded)
                    {
                        _logger.LogWarning($"Failed to add user {user.Id} to roles");
                    }
                }


                var userDto = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    UserName = user.UserName
                };

                return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, userDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                return StatusCode(500, "Internal server error");
            }
        }

        // PUT: api/users/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserDto updateUserDto)
        {
           

            try
            {
                if (id != updateUserDto.Id)
                {
                    return BadRequest("ID mismatch");
                }

                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                // Update basic user properties
                user.Email = updateUserDto.Email;
                user.UserName = updateUserDto.Email;
                user.FirstName = updateUserDto.FirstName;
                user.LastName = updateUserDto.LastName;
                user.PhoneNumber = updateUserDto.PhoneNumber;

                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    foreach (var error in updateResult.Errors)
                    {
                        ModelState.AddModelError(error.Code, error.Description);
                    }
                    return BadRequest(ModelState);
                }

                // Handle role updates
                if (updateUserDto.Roles != null)
                {
                    var currentRoles = await _userManager.GetRolesAsync(user);

                    // Remove roles not in the updated list
                    var rolesToRemove = currentRoles.Except(updateUserDto.Roles);
                    await _userManager.RemoveFromRolesAsync(user, rolesToRemove);

                    // Add new roles
                    var rolesToAdd = updateUserDto.Roles.Except(currentRoles);
                    await _userManager.AddToRolesAsync(user, rolesToAdd);
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating user with ID {id}");
                return StatusCode(500, "Internal server error");
            }
        }

        // DELETE: api/users/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                var result = await _userManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(error.Code, error.Description);
                    }
                    return BadRequest(ModelState);
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting user with ID {id}");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}