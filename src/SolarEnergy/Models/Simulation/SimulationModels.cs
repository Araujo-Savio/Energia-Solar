using System.Collections.Generic;

namespace SolarEnergy.Models.Simulation
{
    public class SimulationScenarioInput
    {
        public double MonthlyConsumption { get; set; }
        public double Coverage { get; set; }
        public int HorizonYears { get; set; }
        public double EnergyTariff { get; set; }
        public double AnnualTariffIncrease { get; set; }
    }

    public class EconomyProjectionPoint
    {
        public int Year { get; set; }
        public double InstallationCumulative { get; set; }
        public double RentalCumulative { get; set; }
    }

    public class EconomyProjectionResult
    {
        public List<EconomyProjectionPoint> Points { get; set; } = new();
        public double TotalInstallEconomy { get; set; }
        public double TotalRentalEconomy { get; set; }
    }

    public class SimulationScenarioResult
    {
        public string Scenario { get; set; } = string.Empty;
        public double MonthlyConsumption { get; set; }
        public double Coverage { get; set; }
        public int HorizonYears { get; set; }
        public double MonthlyGeneration { get; set; }
        public double SystemSizeKw { get; set; }
        public double Investment { get; set; }
        public double MonthlyCost { get; set; }
        public double AnnualCost { get; set; }
        public double TotalScenarioCosts { get; set; }
        public double MonthlyGrossSavings { get; set; }
        public double MonthlyNetSavings { get; set; }
        public double AnnualGrossSavings { get; set; }
        public double AnnualNetSavings { get; set; }
        public double TotalGrossSavings { get; set; }
        public double TotalNetSavings { get; set; }
        public double NetGain { get; set; }
        public double LastYearMonthlyNetSavings { get; set; }
        public double EmissionOffsetTons { get; set; }
        public double PaybackYears { get; set; }
        public List<double> AnnualNetSavingsTimeline { get; set; } = new();
    }

    public class SimulationComparisonResult
    {
        public double InstallationNetGain { get; set; }
        public double RentalNetGain { get; set; }
        public double Difference { get; set; }
        public string SuggestedOption { get; set; } = string.Empty;
        public double? InstallationPaybackYears { get; set; }
        public double InstallationInvestment { get; set; }
        public double RentalTotalCosts { get; set; }
    }
}
