using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToolTrackingSystem.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Department = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Position = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tools",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ToolType = table.Column<int>(type: "int", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Unit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    StockQuantity = table.Column<int>(type: "int", nullable: false),
                    MinimumStock = table.Column<int>(type: "int", nullable: false),
                    CalibrationRequired = table.Column<bool>(type: "bit", nullable: false),
                    CalibrationFrequencyDays = table.Column<int>(type: "int", nullable: true),
                    LastCalibrationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NextCalibrationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tools", x => x.Id);
                    table.CheckConstraint("CK_Tools_CalibrationDates", "NOT (CalibrationRequired = 1 AND (LastCalibrationDate IS NULL OR NextCalibrationDate IS NULL))");
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Salt = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false),
                    EmployeeId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    LastLogin = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ToolCalibrations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ToolId = table.Column<int>(type: "int", nullable: false),
                    CalibrationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NextCalibrationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PerformedById = table.Column<int>(type: "int", nullable: false),
                    CertificateNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ToolCalibrations", x => x.Id);
                    table.CheckConstraint("CK_ToolCalibration_Dates", "NextCalibrationDate > CalibrationDate");
                    table.ForeignKey(
                        name: "FK_ToolCalibrations_Tools_ToolId",
                        column: x => x.ToolId,
                        principalTable: "Tools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ToolCalibrations_Users_PerformedById",
                        column: x => x.PerformedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ToolCalibrations_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ToolIssuances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ToolId = table.Column<int>(type: "int", nullable: false),
                    IssuedToId = table.Column<int>(type: "int", nullable: false),
                    IssuedById = table.Column<int>(type: "int", nullable: false),
                    IssuedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ExpectedReturnDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Purpose = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ToolReturnId = table.Column<int>(type: "int", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ToolIssuances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ToolIssuances_Employees_IssuedToId",
                        column: x => x.IssuedToId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ToolIssuances_Tools_ToolId",
                        column: x => x.ToolId,
                        principalTable: "Tools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ToolIssuances_Users_IssuedById",
                        column: x => x.IssuedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ToolIssuances_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ToolReturns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IssuanceId = table.Column<int>(type: "int", nullable: false),
                    ReturnedById = table.Column<int>(type: "int", nullable: false),
                    ReturnedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    Condition = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ToolReturns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ToolReturns_ToolIssuances_IssuanceId",
                        column: x => x.IssuanceId,
                        principalTable: "ToolIssuances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ToolReturns_Users_ReturnedById",
                        column: x => x.ReturnedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ToolReturns_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Employees_EmployeeId",
                table: "Employees",
                column: "EmployeeId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ToolCalibrations_PerformedById",
                table: "ToolCalibrations",
                column: "PerformedById");

            migrationBuilder.CreateIndex(
                name: "IX_ToolCalibrations_ToolId",
                table: "ToolCalibrations",
                column: "ToolId");

            migrationBuilder.CreateIndex(
                name: "IX_ToolCalibrations_UserId",
                table: "ToolCalibrations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ToolIssuances_IssuedById",
                table: "ToolIssuances",
                column: "IssuedById");

            migrationBuilder.CreateIndex(
                name: "IX_ToolIssuances_IssuedToId",
                table: "ToolIssuances",
                column: "IssuedToId");

            migrationBuilder.CreateIndex(
                name: "IX_ToolIssuances_ToolId",
                table: "ToolIssuances",
                column: "ToolId");

            migrationBuilder.CreateIndex(
                name: "IX_ToolIssuances_ToolReturnId",
                table: "ToolIssuances",
                column: "ToolReturnId");

            migrationBuilder.CreateIndex(
                name: "IX_ToolIssuances_UserId",
                table: "ToolIssuances",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ToolReturns_IssuanceId",
                table: "ToolReturns",
                column: "IssuanceId");

            migrationBuilder.CreateIndex(
                name: "IX_ToolReturns_ReturnedById",
                table: "ToolReturns",
                column: "ReturnedById");

            migrationBuilder.CreateIndex(
                name: "IX_ToolReturns_UserId",
                table: "ToolReturns",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Tools_Code",
                table: "Tools",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_EmployeeId",
                table: "Users",
                column: "EmployeeId",
                unique: true,
                filter: "[EmployeeId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ToolIssuances_ToolReturns_ToolReturnId",
                table: "ToolIssuances",
                column: "ToolReturnId",
                principalTable: "ToolReturns",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ToolIssuances_Tools_ToolId",
                table: "ToolIssuances");

            migrationBuilder.DropForeignKey(
                name: "FK_ToolIssuances_Users_IssuedById",
                table: "ToolIssuances");

            migrationBuilder.DropForeignKey(
                name: "FK_ToolIssuances_Users_UserId",
                table: "ToolIssuances");

            migrationBuilder.DropForeignKey(
                name: "FK_ToolReturns_Users_ReturnedById",
                table: "ToolReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_ToolReturns_Users_UserId",
                table: "ToolReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_ToolIssuances_Employees_IssuedToId",
                table: "ToolIssuances");

            migrationBuilder.DropForeignKey(
                name: "FK_ToolIssuances_ToolReturns_ToolReturnId",
                table: "ToolIssuances");

            migrationBuilder.DropTable(
                name: "ToolCalibrations");

            migrationBuilder.DropTable(
                name: "Tools");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Employees");

            migrationBuilder.DropTable(
                name: "ToolReturns");

            migrationBuilder.DropTable(
                name: "ToolIssuances");
        }
    }
}
