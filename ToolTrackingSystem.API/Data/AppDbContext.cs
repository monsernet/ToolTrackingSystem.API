using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ToolTrackingSystem.API.Models.Entities;

namespace ToolTrackingSystem.API.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<Tool> Tools { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<ToolIssuance> ToolIssuances { get; set; }
        public DbSet<ToolReturn> ToolReturns { get; set; }
        public DbSet<ToolCalibration> ToolCalibrations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Tool entity
            modelBuilder.Entity<Tool>(entity =>
            {
                entity.HasIndex(t => t.Code).IsUnique();
                entity.Property(t => t.Status).HasDefaultValue(ToolStatus.Active);

                entity.ToTable(tableBuilder =>
                {
                    tableBuilder.HasCheckConstraint(
                        "CK_Tools_CalibrationDates",
                        "NOT (CalibrationRequired = 1 AND (LastCalibrationDate IS NULL OR NextCalibrationDate IS NULL))");
                });
            });

            // Configure Employee entity
            modelBuilder.Entity<Employee>(entity =>
            {
                entity.HasIndex(e => e.EmployeeId).IsUnique();
                entity.Property(e => e.IsActive).HasDefaultValue(true);
            });

            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(u => u.Username).IsUnique();
                entity.HasIndex(u => u.Email).IsUnique();
                entity.Property(u => u.IsActive).HasDefaultValue(true);

                entity.HasOne(u => u.Employee)
                    .WithOne(e => e.User)
                    .HasForeignKey<User>(u => u.EmployeeId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure ToolIssuance entity
            modelBuilder.Entity<ToolIssuance>(entity =>
            {
                entity.Property(i => i.IssuedDate).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(i => i.StatusValue).HasDefaultValue((int)IssuanceStatus.Issued);

                entity.HasOne(i => i.Tool)
                    .WithMany(t => t.Issuances)
                    .HasForeignKey(i => i.ToolId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(i => i.IssuedTo)
                    .WithMany(e => e.ToolIssuances)
                    .HasForeignKey(i => i.IssuedToId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(i => i.IssuedBy)
                    .WithMany()
                    .HasForeignKey(i => i.IssuedById)
                    .OnDelete(DeleteBehavior.Restrict);
            });


            // Configure ToolReturn entity
            modelBuilder.Entity<ToolReturn>(entity =>
            {
                entity.Property(r => r.ReturnedDate).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(r => r.Issuance)
                    .WithMany()
                    .HasForeignKey(r => r.IssuanceId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.ReturnedBy)
                    .WithMany()
                    .HasForeignKey(r => r.ReturnedById)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure ToolCalibration entity
            modelBuilder.Entity<ToolCalibration>(entity =>
            {
                entity.ToTable(tableBuilder =>
                {
                    tableBuilder.HasCheckConstraint(
                        "CK_ToolCalibration_Dates",
                        "NextCalibrationDate > CalibrationDate");
                });

                entity.HasOne(c => c.Tool)
                    .WithMany(t => t.Calibrations)
                    .HasForeignKey(c => c.ToolId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(c => c.PerformedBy)
                    .WithMany()
                    .HasForeignKey(c => c.PerformedById)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
