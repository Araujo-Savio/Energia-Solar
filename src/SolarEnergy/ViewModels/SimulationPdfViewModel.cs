namespace SolarEnergy.ViewModels
{
    public class SimulationPdfViewModel
    {
        public bool IsCompanyUser { get; set; }
        public string? SelectedCompanyName { get; set; }
        public UserSimulationInput? UserInput { get; set; }
        public UserSimulationResult? UserResult { get; set; }
        public UserSimulationInput? CompanyInput { get; set; }
        public UserSimulationResult? CompanyResult { get; set; }
    }
}
