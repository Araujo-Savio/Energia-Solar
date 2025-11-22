namespace SolarEnergy.ViewModels
{
    public class SimulationPdfViewModel
    {
        public bool IsCompanyUser { get; set; }
        public string? SelectedCompanyName { get; set; }

        public decimal AverageMonthlyConsumptionKwh { get; set; }
        public decimal TariffPerKwh { get; set; }
        public decimal CoveragePercent { get; set; }
        public decimal DegradationPercent { get; set; }
        public int HorizonYears { get; set; }
        public decimal TariffInflationPercent { get; set; }

        public CompanyParametersInputModel? CompanyParameters { get; set; }

        public UserSimulationResult? UserResult { get; set; }
        public CompanySimulationResult? CompanyResult { get; set; }
    }
}
