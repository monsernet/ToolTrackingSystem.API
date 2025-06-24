using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ToolTrackingSystem.API.Core.Constants;
using ToolTrackingSystem.API.Data;
using ToolTrackingSystem.API.Models.Entities;

namespace ToolTrackingSystem.API.Repositories
{
    public class UserRepository : GenericRepository<ApplicationUser>, IUserRepository
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserRepository(AppDbContext dbcontext, 
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager) : base(dbcontext)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<ApplicationUser?> GetByEmailAsync(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<List<string>> GetRolesAsync(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return new List<string>();
            }
            var roles = await _userManager.GetRolesAsync(user);
            return roles.ToList();
        }

        public async Task<bool> RoleExistsAsync(string roleName)
        {
           
            if (!RoleHelper.IsValidRole(roleName))
                return false;
            
            return await _roleManager.RoleExistsAsync(roleName);
        }
    }
}
