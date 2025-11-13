using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SolarEnergy.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanyCostProfiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CompanyCostProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ProductionPerKilowattPeak = table.Column<double>(type: "float", nullable: false, defaultValue: 140.0),
                    MaintenanceRate = table.Column<decimal>(type: "decimal(18,6)", nullable: false, defaultValue: 0.015m),
                    RentalRatePerKwh = table.Column<decimal>(type: "decimal(18,6)", nullable: false, defaultValue: 0.65m),
                    RentalAnnualIncrease = table.Column<decimal>(type: "decimal(18,6)", nullable: false, defaultValue: 0.03m),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyCostProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanyCostProfiles_AspNetUsers_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CompanyCostItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyCostProfileId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    ItemType = table.Column<int>(type: "int", nullable: false),
                    Cost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyCostItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanyCostItems_CompanyCostProfiles_CompanyCostProfileId",
                        column: x => x.CompanyCostProfileId,
                        principalTable: "CompanyCostProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CompanySystemSizeCosts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyCostProfileId = table.Column<int>(type: "int", nullable: false),
                    Label = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    SystemSizeKwp = table.Column<decimal>(type: "decimal(9,2)", nullable: false),
                    AverageCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanySystemSizeCosts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanySystemSizeCosts_CompanyCostProfiles_CompanyCostProfileId",
                        column: x => x.CompanyCostProfileId,
                        principalTable: "CompanyCostProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CompanyCostItems_CompanyCostProfileId",
                table: "CompanyCostItems",
                column: "CompanyCostProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyCostProfiles_CompanyId",
                table: "CompanyCostProfiles",
                column: "CompanyId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompanySystemSizeCosts_CompanyCostProfileId_SystemSizeKwp",
                table: "CompanySystemSizeCosts",
                columns: new[] { "CompanyCostProfileId", "SystemSizeKwp" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompanyCostItems");

            migrationBuilder.DropTable(
                name: "CompanySystemSizeCosts");

            migrationBuilder.DropTable(
                name: "CompanyCostProfiles");
        }
    }
}
