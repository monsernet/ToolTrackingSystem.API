using Microsoft.AspNetCore.Identity;
using ToolTrackingSystem.API.Models.Entities;

namespace ToolTrackingSystem.API.Repositories
{
    public interface IUserRepository : IGenericRepository<ApplicationUser>
    {
        Task<ApplicationUser?> GetByEmailAsync(string email);
        Task<List<string>> GetRolesAsync(Guid userId);
        Task<bool> RoleExistsAsync(string roleName);
    }
}
