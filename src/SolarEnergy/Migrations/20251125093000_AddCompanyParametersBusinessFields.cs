using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SolarEnergy.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanyParametersBusinessFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ConsumptionPerKwp",
                table: "CompanyParameters",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "InstallDiscountPercent",
                table: "CompanyParameters",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MaintenancePercent",
                table: "CompanyParameters",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MinSystemSizeKwp",
                table: "CompanyParameters",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "RentalAnnualIncreasePercent",
                table: "CompanyParameters",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "RentalDiscountPercent",
                table: "CompanyParameters",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "RentalFactorPercent",
                table: "CompanyParameters",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "RentalMinMonthly",
                table: "CompanyParameters",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "RentalSetupPerKwp",
                table: "CompanyParameters",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConsumptionPerKwp",
                table: "CompanyParameters");

            migrationBuilder.DropColumn(
                name: "InstallDiscountPercent",
                table: "CompanyParameters");

            migrationBuilder.DropColumn(
                name: "MaintenancePercent",
                table: "CompanyParameters");

            migrationBuilder.DropColumn(
                name: "MinSystemSizeKwp",
                table: "CompanyParameters");

            migrationBuilder.DropColumn(
                name: "RentalAnnualIncreasePercent",
                table: "CompanyParameters");

            migrationBuilder.DropColumn(
                name: "RentalDiscountPercent",
                table: "CompanyParameters");

            migrationBuilder.DropColumn(
                name: "RentalFactorPercent",
                table: "CompanyParameters");

            migrationBuilder.DropColumn(
                name: "RentalMinMonthly",
                table: "CompanyParameters");

            migrationBuilder.DropColumn(
                name: "RentalSetupPerKwp",
                table: "CompanyParameters");
        }
    }
}
