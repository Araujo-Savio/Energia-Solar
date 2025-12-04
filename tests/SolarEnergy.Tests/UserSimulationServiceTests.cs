using SolarEnergy.Services;
using SolarEnergy.ViewModels;
using Xunit;

namespace SolarEnergy.Tests;

public class UserSimulationServiceTests
{
    [Fact]
    public void InstallationInvestment_ShouldMatchInstallSummaryValue()
    {
        var service = new UserSimulationService();
        var input = new UserSimulationInput
        {
            AverageMonthlyConsumptionKwh = 450,
            TariffPerKwh = 1.1,
            CoveragePercent = 85,
            DegradationPercent = 1.2,
            HorizonYears = 8,
            InflationPercent = 6,
            CompanyParameters = new CompanyParametersInputModel
            {
                SystemPricePerKwp = 5200m,
                MaintenancePercent = 1.5m,
                InstallDiscountPercent = 3m,
                RentalFactorPercent = 60m,
                RentalMinMonthly = 200m,
                RentalSetupPerKwp = 120m,
                RentalAnnualIncreasePercent = 4m,
                RentalDiscountPercent = 12m,
                ConsumptionPerKwp = 120m,
                MinSystemSizeKwp = 3m
            }
        };

        var result = service.Calculate(input);

        Assert.Equal(result.TotalInstallCost, result.InstallationInvestment, 6);
        Assert.Equal(result.TotalInstallCost, result.InitialInvestment, 6);
    }
}
