namespace SolarEnergy.ViewModels
{
    public class SimulationPdfViewModel
    {
        public bool IsCompanyUser { get; set; }
        public string? SelectedCompanyName { get; set; }

        public UserSimulationInput? UserInput { get; set; }
        public CompanySimulationInput? CompanyInput { get; set; }

        public CompanyParametersInputModel? CompanyParameters { get; set; }

        public UserSimulationResult? UserResult { get; set; }
        public CompanySimulationResult? CompanyResult { get; set; }
    }
}
