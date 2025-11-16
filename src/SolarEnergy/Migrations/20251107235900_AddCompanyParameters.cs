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
                    PricePerKwP = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AnnualMaintenance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InstallationDiscount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RentalPercent = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MinRentalValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RentalSetupFee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AnnualRentIncrease = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RentDiscount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    KwhPerKwp = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MinSystemPower = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyParameters", x => x.Id);
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
