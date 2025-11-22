using System.Collections.Generic;

namespace SolarEnergy.ViewModels
{
    public class SimulationViewModel
    {
        public bool IsCompanyUser { get; set; }
        public string? SelectedCompanyId { get; set; }
        public string? SelectedCompanyName { get; set; }
        public IEnumerable<CompanyOptionViewModel> Companies { get; set; } = new List<CompanyOptionViewModel>();
        public CompanyParametersInputModel? CompanyParameters { get; set; }
        public string CompanyParametersJson { get; set; } = "null";
        public SimulationInputModel UserInput { get; set; } = new SimulationInputModel
        {
            MonthlyConsumption = 0,
            EnergyTariff = 0.92,
            Coverage = 0,
            Degradation = 0.7,
            HorizonYears = 10,
            Inflation = 7.5
        };
    }

    public class CompanyOptionViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    public class SimulationInputModel
    {
        public double MonthlyConsumption { get; set; }
        public double EnergyTariff { get; set; }
        public double Coverage { get; set; }
        public double Degradation { get; set; }
        public int HorizonYears { get; set; }
        public double Inflation { get; set; }
    }

    public class UserSimulationInput
    {
        public double AverageMonthlyConsumptionKwh { get; set; }
        public double TariffPerKwh { get; set; }
        public double CoveragePercent { get; set; }
        public double DegradationPercent { get; set; }
        public int HorizonYears { get; set; }
        public double InflationPercent { get; set; }
        public CompanyParametersInputModel? CompanyParameters { get; set; }
        public string? SelectedCompanyId { get; set; }
        public string? SelectedCompanyName { get; set; }
    }

    public class UserSimulationResult
    {
        public UserSimulationInput Input { get; set; } = new();
        public CompanyParametersInputModel? CompanyParameters { get; set; }
        public string? SelectedCompanyId { get; set; }
        public string? SelectedCompanyName { get; set; }
        public double CostWithoutSolar { get; set; }
        public double InstallationInvestment { get; set; }
        public double RentCost { get; set; }
        public double InstallationSavings { get; set; }
        public double RentSavings { get; set; }
        public double AnnualGeneratedEnergyKwh { get; set; }
        public double MonthlySavingInstallation { get; set; }
        public double MonthlySavingRent { get; set; }
        public double AverageAnnualSaving { get; set; }
        public double? PaybackYears { get; set; }
        public double InstallationTimeMonths { get; set; }
        public double InitialRentAmount { get; set; }
        public double DiscountAppliedPercent { get; set; }
        public double FiveYearAccumulatedSaving { get; set; }
        public double TotalInstallCost { get; set; }
        public double TotalRentalCost { get; set; }
        public double CoveragePercent { get; set; }
        public int AnalyzedHorizonYears { get; set; }
    }
}
