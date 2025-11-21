using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SolarEnergy.Migrations
{
    public partial class DropInstallationDiscountColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InstallationDiscount",
                table: "CompanyParameters");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "InstallationDiscount",
                table: "CompanyParameters",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
