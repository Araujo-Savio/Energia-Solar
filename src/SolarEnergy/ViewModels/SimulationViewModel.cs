using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SolarEnergy.ViewModels
{
    public class SimulationViewModel
    {
        public SimulationScenarioInput InstallationInput { get; set; } = SimulationScenarioInput.CreateDefaultInstallation();
        public SimulationScenarioInput RentalInput { get; set; } = SimulationScenarioInput.CreateDefaultRental();

        public SimulationScenarioResult? InstallationResult { get; set; }
        public SimulationScenarioResult? RentalResult { get; set; }
        public SimulationComparisonResult? Comparison { get; set; }
        public IReadOnlyList<EconomyProjectionPoint> Projection { get; set; } = new List<EconomyProjectionPoint>();
        public string ActiveScenario { get; set; } = "installation";

        public bool HasResults => InstallationResult != null && RentalResult != null && Comparison != null;
    }

    public class SimulationScenarioInput
    {
        [Display(Name = "Consumo da Rede (kWh/mês)")]
        [Range(100, 100000, ErrorMessage = "Informe um consumo entre {1} e {2} kWh por mês.")]
        public double MonthlyConsumption { get; set; }

        [Display(Name = "Cobertura (%)")]
        [Range(10, 100, ErrorMessage = "Informe uma cobertura entre {1}% e {2}%.")]
        public int Coverage { get; set; }

        [Display(Name = "Horizonte (anos)")]
        [Range(1, 30, ErrorMessage = "Informe um horizonte entre {1} e {2} anos.")]
        public int HorizonYears { get; set; }

        [Display(Name = "Tarifa média da rede (R$/kWh)")]
        [Range(0.1, 5.0, ErrorMessage = "Informe uma tarifa entre {1} e {2} R$/kWh.")]
        public double EnergyTariff { get; set; }

        [Display(Name = "Reajuste anual da tarifa (%)")]
        [Range(0, 30, ErrorMessage = "Informe um reajuste entre {1}% e {2}%.")]
        public double AnnualTariffIncrease { get; set; }

        public static SimulationScenarioInput CreateDefaultInstallation() => new()
        {
            MonthlyConsumption = 3500,
            Coverage = 80,
            HorizonYears = 10,
            EnergyTariff = 0.92,
            AnnualTariffIncrease = 6
        };

        public static SimulationScenarioInput CreateDefaultRental() => new()
        {
            MonthlyConsumption = 3100,
            Coverage = 70,
            HorizonYears = 8,
            EnergyTariff = 0.92,
            AnnualTariffIncrease = 5
        };
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
        public IReadOnlyList<double> AnnualNetSavingsTimeline { get; set; } = new List<double>();

        public bool HasPayback => !double.IsNaN(PaybackYears);
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

    public class EconomyProjectionPoint
    {
        public int Year { get; set; }
        public double InstallationCumulative { get; set; }
        public double RentalCumulative { get; set; }
    }
}
