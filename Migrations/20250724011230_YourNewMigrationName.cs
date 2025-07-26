using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrafficViolationsAPI.Migrations
{
    public partial class YourNewMigrationName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NationalId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    UserType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Citizen"),
                    PasswordHash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ViolationTypes",
                columns: table => new
                {
                    Violation_Type_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Violation_Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DefaultFineAmount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ViolationTypes", x => x.Violation_Type_ID);
                });

            migrationBuilder.CreateTable(
                name: "Vehicles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Make = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Model = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Year = table.Column<int>(type: "int", nullable: true),
                    Color = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    PlateNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    VehicleType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    OwnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RegistrationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vehicles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Vehicles_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Violations",
                columns: table => new
                {
                    Violation_ID = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Violation_Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Violation_Location = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Plate_Number = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Plate_Type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Dividing = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Violation_Type_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Created_By_User_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Created_At = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    VehicleId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Violations", x => x.Violation_ID);
                    table.ForeignKey(
                        name: "FK_Violations_Users_Created_By_User_ID",
                        column: x => x.Created_By_User_ID,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Violations_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Violations_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Violations_ViolationTypes_Violation_Type_ID",
                        column: x => x.Violation_Type_ID,
                        principalTable: "ViolationTypes",
                        principalColumn: "Violation_Type_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_NationalId",
                table: "Users",
                column: "NationalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_OwnerId",
                table: "Vehicles",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_PlateNumber",
                table: "Vehicles",
                column: "PlateNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Violations_Created_By_User_ID",
                table: "Violations",
                column: "Created_By_User_ID");

            migrationBuilder.CreateIndex(
                name: "IX_Violations_UserId",
                table: "Violations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Violations_VehicleId",
                table: "Violations",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_Violations_Violation_Type_ID",
                table: "Violations",
                column: "Violation_Type_ID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Violations");

            migrationBuilder.DropTable(
                name: "Vehicles");

            migrationBuilder.DropTable(
                name: "ViolationTypes");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
