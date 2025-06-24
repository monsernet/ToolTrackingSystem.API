namespace ToolTrackingSystem.API.Core.Constants
{
    public static class RoleHelper
    {
        public const string Admin = "admin";
        public const string Agent = "agent";
        public const string Manager = "manager";

        public static List<string> GetAllRoles()
        {
            return new List<string> { Admin, Agent, Manager };
        }

        public static bool IsValidRole(string roleName)
        {
            return GetAllRoles().Contains(roleName.ToLower());
        }
    }
}
