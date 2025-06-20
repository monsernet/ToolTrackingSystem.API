using Microsoft.EntityFrameworkCore;
using ToolTrackingSystem.API.Models.Entities;

namespace ToolTrackingSystem.API.Data
{
    public static class DbInitializer
    {
        public static void Initialize(AppDbContext context)
        {
            context.Database.EnsureCreated();

            if (context.Employees.Any())
            {
                return; // DB has been seeded
            }

            var employees = new Employee[]
            {
                new Employee { EmployeeId = "EMP001", FirstName = "John", LastName = "Doe", Email = "john.doe@company.com", Department = "Maintenance", Position = "Senior Technician" },
                new Employee { EmployeeId = "EMP002", FirstName = "Jane", LastName = "Smith", Email = "jane.smith@company.com", Department = "Service", Position = "Technician" },
                new Employee { EmployeeId = "EMP003", FirstName = "Robert", LastName = "Johnson", Email = "robert.johnson@company.com", Department = "Maintenance", Position = "Manager" }
            };

            context.Employees.AddRange(employees);
            context.SaveChanges();

            var users = new User[]
            {
                new User { Username = "admin", PasswordHash = "AEF024A5B5B3A6B3E6C9B8A5D3F2E5D1C8B3A6B3E6C9B8A5D3F2E5D1C8B3A6B3", Salt = "SALT123", Email = "admin@tooltracking.com", Role = UserRole.Admin, EmployeeId = employees[2].Id },
                new User { Username = "supervisor", PasswordHash = "BEF124B5B3A6B3E6C9B8A5D3F2E5D1C8B3A6B3E6C9B8A5D3F2E5D1C8B3A6B4", Salt = "SALT456", Email = "supervisor@tooltracking.com", Role = UserRole.Supervisor, EmployeeId = employees[0].Id },
                new User { Username = "staff", PasswordHash = "CEF224C5B3A6B3E6C9B8A5D3F2E5D1C8B3A6B3E6C9B8A5D3F2E5D1C8B3A6B5", Salt = "SALT789", Email = "staff@tooltracking.com", Role = UserRole.Staff, EmployeeId = employees[1].Id }
            };

            context.Users.AddRange(users);
            context.SaveChanges();

            var tools = new Tool[]
            {
                new Tool { Code = "T001", Name = "Torque Wrench", Description = "Digital torque wrench 50-250 Nm", ToolType = ToolType.Special, Category = "Mechanical", Unit = "pcs", StockQuantity = 5, MinimumStock = 2, CalibrationRequired = true, CalibrationFrequencyDays = 180, LastCalibrationDate = DateTime.UtcNow.AddDays(-30), NextCalibrationDate = DateTime.UtcNow.AddDays(150) },
                new Tool { Code = "T002", Name = "Multimeter", Description = "Digital multimeter with auto-ranging", ToolType = ToolType.Special, Category = "Electrical", Unit = "pcs", StockQuantity = 8, MinimumStock = 3, CalibrationRequired = true, CalibrationFrequencyDays = 365, LastCalibrationDate = DateTime.UtcNow.AddDays(-100), NextCalibrationDate = DateTime.UtcNow.AddDays(265) },
                new Tool { Code = "T003", Name = "Screwdriver Set", Description = "10-piece insulated screwdriver set", ToolType = ToolType.DailyUse, Category = "General", Unit = "set", StockQuantity = 15, MinimumStock = 5, CalibrationRequired = false },
                new Tool { Code = "T004", Name = "Oil Filter Wrench", Description = "Adjustable oil filter wrench", ToolType = ToolType.DailyUse, Category = "Mechanical", Unit = "pcs", StockQuantity = 3, MinimumStock = 1, CalibrationRequired = false }
            };

            context.Tools.AddRange(tools);
            context.SaveChanges();
        }
    }
}