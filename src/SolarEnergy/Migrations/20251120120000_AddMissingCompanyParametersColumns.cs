using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SolarEnergy.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingCompanyParametersColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH('CompanyParameters', 'MaintenancePercent') IS NULL
    ALTER TABLE CompanyParameters ADD MaintenancePercent decimal(5,2) NOT NULL CONSTRAINT DF_CompanyParameters_MaintenancePercent DEFAULT 1.2;
IF COL_LENGTH('CompanyParameters', 'InstallDiscountPercent') IS NULL
    ALTER TABLE CompanyParameters ADD InstallDiscountPercent decimal(5,2) NOT NULL CONSTRAINT DF_CompanyParameters_InstallDiscountPercent DEFAULT 4;
IF COL_LENGTH('CompanyParameters', 'RentalFactorPercent') IS NULL
    ALTER TABLE CompanyParameters ADD RentalFactorPercent decimal(5,2) NOT NULL CONSTRAINT DF_CompanyParameters_RentalFactorPercent DEFAULT 68;
IF COL_LENGTH('CompanyParameters', 'RentalMinMonthly') IS NULL
    ALTER TABLE CompanyParameters ADD RentalMinMonthly decimal(18,2) NOT NULL CONSTRAINT DF_CompanyParameters_RentalMinMonthly DEFAULT 250;
IF COL_LENGTH('CompanyParameters', 'RentalSetupPerKwp') IS NULL
    ALTER TABLE CompanyParameters ADD RentalSetupPerKwp decimal(18,2) NOT NULL CONSTRAINT DF_CompanyParameters_RentalSetupPerKwp DEFAULT 150;
IF COL_LENGTH('CompanyParameters', 'RentalAnnualIncreasePercent') IS NULL
    ALTER TABLE CompanyParameters ADD RentalAnnualIncreasePercent decimal(5,2) NOT NULL CONSTRAINT DF_CompanyParameters_RentalAnnualIncreasePercent DEFAULT 4.5;
IF COL_LENGTH('CompanyParameters', 'RentalDiscountPercent') IS NULL
    ALTER TABLE CompanyParameters ADD RentalDiscountPercent decimal(5,2) NOT NULL CONSTRAINT DF_CompanyParameters_RentalDiscountPercent DEFAULT 15;
IF COL_LENGTH('CompanyParameters', 'ConsumptionPerKwp') IS NULL
    ALTER TABLE CompanyParameters ADD ConsumptionPerKwp decimal(18,2) NOT NULL CONSTRAINT DF_CompanyParameters_ConsumptionPerKwp DEFAULT 120;
IF COL_LENGTH('CompanyParameters', 'MinSystemSizeKwp') IS NULL
    ALTER TABLE CompanyParameters ADD MinSystemSizeKwp decimal(18,2) NOT NULL CONSTRAINT DF_CompanyParameters_MinSystemSizeKwp DEFAULT 2.5;
IF COL_LENGTH('CompanyParameters', 'PricePerKwp') IS NULL
    ALTER TABLE CompanyParameters ADD PricePerKwp decimal(18,2) NOT NULL CONSTRAINT DF_CompanyParameters_PricePerKwp DEFAULT 4200;
IF COL_LENGTH('CompanyParameters', 'UpdatedAt') IS NULL
    ALTER TABLE CompanyParameters ADD UpdatedAt datetime2 NOT NULL CONSTRAINT DF_CompanyParameters_UpdatedAt DEFAULT SYSUTCDATETIME();
IF COL_LENGTH('CompanyParameters', 'CompanyId') IS NULL
    ALTER TABLE CompanyParameters ADD CompanyId nvarchar(450) NOT NULL DEFAULT '';
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No-op: the additional columns are part of the current model and should not be removed.
        }
    }
}
