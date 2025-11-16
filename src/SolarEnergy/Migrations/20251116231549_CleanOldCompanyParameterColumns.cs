using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SolarEnergy.Migrations
{
    public partial class CleanOldCompanyParameterColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "PricePerKwp", table: "CompanyParameters");
            migrationBuilder.DropColumn(name: "RentalPercent", table: "CompanyParameters");
            migrationBuilder.DropColumn(name: "MinRentalValue", table: "CompanyParameters");
            migrationBuilder.DropColumn(name: "RentalSetupFee", table: "CompanyParameters");
            migrationBuilder.DropColumn(name: "AnnualRentIncrease", table: "CompanyParameters");
            migrationBuilder.DropColumn(name: "RentDiscount", table: "CompanyParameters");
            migrationBuilder.DropColumn(name: "KwhPerKwp", table: "CompanyParameters");
            migrationBuilder.DropColumn(name: "MinSystemPower", table: "CompanyParameters");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(name: "PricePerKwp", table: "CompanyParameters", type: "decimal(18,2)", nullable: false, defaultValue: 0m);
            migrationBuilder.AddColumn<decimal>(name: "RentalPercent", table: "CompanyParameters", type: "decimal(18,2)", nullable: false, defaultValue: 0m);
            migrationBuilder.AddColumn<decimal>(name: "MinRentalValue", table: "CompanyParameters", type: "decimal(18,2)", nullable: false, defaultValue: 0m);
            migrationBuilder.AddColumn<decimal>(name: "RentalSetupFee", table: "CompanyParameters", type: "decimal(18,2)", nullable: false, defaultValue: 0m);
            migrationBuilder.AddColumn<decimal>(name: "AnnualRentIncrease", table: "CompanyParameters", type: "decimal(18,2)", nullable: false, defaultValue: 0m);
            migrationBuilder.AddColumn<decimal>(name: "RentDiscount", table: "CompanyParameters", type: "decimal(18,2)", nullable: false, defaultValue: 0m);
            migrationBuilder.AddColumn<decimal>(name: "KwhPerKwp", table: "CompanyParameters", type: "decimal(18,2)", nullable: false, defaultValue: 0m);
            migrationBuilder.AddColumn<decimal>(name: "MinSystemPower", table: "CompanyParameters", type: "decimal(18,2)", nullable: false, defaultValue: 0m);
        }
    }
}
