using System.Collections.Generic;

namespace SolarEnergy.ViewModels
{
    public class SimulationViewModel
    {
        public bool IsCompanyUser { get; set; }
        public string? SelectedCompanyId { get; set; }
        public IEnumerable<CompanyOptionViewModel> Companies { get; set; } = new List<CompanyOptionViewModel>();
        public CompanyParametersInputModel? CompanyParameters { get; set; }
        public string CompanyParametersJson { get; set; } = "null";
    }

    public class CompanyOptionViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}
