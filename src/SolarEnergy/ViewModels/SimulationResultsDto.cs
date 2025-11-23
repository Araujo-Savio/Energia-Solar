namespace SolarEnergy.ViewModels
{
    public class SimulationResultsDto
    {
        public bool IsCompanyUser { get; set; }
        public string? SelectedCompanyName { get; set; }

        public decimal CalculatorAverageMonthlyConsumptionKwh { get; set; }
        public decimal CalculatorTariffPerKwh { get; set; }
        public decimal CalculatorCoveragePercent { get; set; }
        public decimal CalculatorDegradationPercent { get; set; }
        public int CalculatorHorizonYears { get; set; }
        public decimal CalculatorTariffInflationPercent { get; set; }

        public CompanyParametersViewModel? CompanyParameters { get; set; }

        public UserSimulationResult? UserResult { get; set; }
        public CompanySimulationResult? CompanyResult { get; set; }
    }
}
