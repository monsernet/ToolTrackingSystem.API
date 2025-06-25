using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ToolTrackingSystem.API.Models.Entities;
using ToolTrackingSystem.API.Repositories;
using ToolTrackingSystem.API.Models.DTOs.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ToolTrackingSystem.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<UsersController> _logger;

        public UsersController(
            IUserRepository userRepository,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<UsersController> logger)
        {
            _userRepository = userRepository;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        // GET: api/users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers()
        {
            try
            {
                _logger.LogInformation("Fetching all users with roles");
                var users = await _userRepository.GetAllAsync();
                var userDtos = new List<UserDto>();

                foreach (var user in users)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    _logger.LogInformation($"User {user.Id} has roles: {string.Join(", ", roles)}");
                    userDtos.Add(new UserDto
                    {
                        Id = user.Id.ToString(),
                        Email = user.Email,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        UserName = user.UserName,
                        Roles = roles.ToList() 
                    });
                }

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
                    return BadRequest(new
                    {
                        Message = "Validation failed",
                        Errors = ModelState.ToDictionary(
                            k => k.Key,
                            v => v.Value.Errors.Select(e => e.ErrorMessage).ToArray())
                    });
                }

                // Check if email exists first
                var emailUser = await _userManager.FindByEmailAsync(createUserDto.Email);
                if (emailUser != null)
                {
                    return Conflict(new
                    {
                        Message = "Email already exists",
                        Field = "email"
                    });
                }

                // Validate roles before creating user
                if (createUserDto.Roles != null && createUserDto.Roles.Any())
                {
                    var invalidRoles = new List<string>();
                    foreach (var role in createUserDto.Roles)
                    {
                        if (!await _roleManager.RoleExistsAsync(role))
                        {
                            invalidRoles.Add(role);
                        }
                    }

                    if (invalidRoles.Any())
                    {
                        return BadRequest(new
                        {
                            Message = "Invalid roles specified",
                            InvalidRoles = invalidRoles
                        });
                    }
                }

                var user = new ApplicationUser
                {
                    UserName = createUserDto.Email,
                    Email = createUserDto.Email,
                    FirstName = createUserDto.FirstName,
                    LastName = createUserDto.LastName,
                    PhoneNumber = createUserDto.PhoneNumber
                };

                var result = await _userManager.CreateAsync(user, createUserDto.Password);

                if (!result.Succeeded)
                {
                    var errorDetails = result.Errors.ToDictionary(
                        e => e.Code,
                        e => e.Description
                    );

                    return BadRequest(new
                    {
                        Message = "User creation failed",
                        Errors = errorDetails
                    });
                }

                // Add user to validated roles
                if (createUserDto.Roles != null && createUserDto.Roles.Any())
                {
                    var roleResult = await _userManager.AddToRolesAsync(user, createUserDto.Roles);
                    if (!roleResult.Succeeded)
                    {
                        _logger.LogWarning("Failed to add roles to user {UserId}. Errors: {Errors}",
                            user.Id, string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                    }
                }

                var userDto = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    UserName = user.UserName,
                    Roles = createUserDto.Roles ?? new List<string>()
                };

                return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, userDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user for email {Email}", createUserDto.Email);
                return StatusCode(500, new
                {
                    Message = "An unexpected error occurred while creating user",
                    Details = ex.Message
                });
            }
        }

        [HttpPost("check-email")]
        public async Task<ActionResult<bool>> CheckEmailExists([FromBody] string email)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                return Ok(user != null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking email existence");
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


        [HttpGet("roles")]
        public async Task<ActionResult<IEnumerable<string>>> GetAvailableRoles()
        {
            try
            {
                var roles = await _roleManager.Roles
                    .Select(r => r.Name)
                    .ToListAsync();

                return Ok(roles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching available roles");
                return StatusCode(500, "Internal server error");
            }
        }


        
        [HttpPut("{id}/change-password")]
        public async Task<IActionResult> ChangePassword(string id, [FromBody] ChangePasswordDto changePasswordDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                // Remove the user's current password
                var removePasswordResult = await _userManager.RemovePasswordAsync(user);
                if (!removePasswordResult.Succeeded)
                {
                    foreach (var error in removePasswordResult.Errors)
                    {
                        ModelState.AddModelError(error.Code, error.Description);
                    }
                    return BadRequest(ModelState);
                }

                // Add the new password
                var addPasswordResult = await _userManager.AddPasswordAsync(user, changePasswordDto.NewPassword);
                if (!addPasswordResult.Succeeded)
                {
                    foreach (var error in addPasswordResult.Errors)
                    {
                        ModelState.AddModelError(error.Code, error.Description);
                    }
                    return BadRequest(ModelState);
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error changing password for user with ID {id}");
                return StatusCode(500, "Internal server error");
            }
        }


        [HttpGet("user-roles")]
        public async Task<ActionResult<List<string>>> GetUserRoles()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var roles = await _userManager.GetRolesAsync(await _userManager.FindByIdAsync(userId));
            return Ok(roles);
        }

    }
}