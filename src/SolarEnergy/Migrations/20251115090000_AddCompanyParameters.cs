using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SolarEnergy.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanyParameters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CompanyParameters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    PricePerKwp = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaintenancePercent = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    InstallDiscountPercent = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    RentalFactorPercent = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    RentalMinMonthly = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RentalSetupPerKwp = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RentalAnnualIncreasePercent = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    RentalDiscountPercent = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    ConsumptionPerKwp = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MinSystemSizeKwp = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyParameters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanyParameters_AspNetUsers_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CompanyParameters_CompanyId",
                table: "CompanyParameters",
                column: "CompanyId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompanyParameters");
        }
    }
}
