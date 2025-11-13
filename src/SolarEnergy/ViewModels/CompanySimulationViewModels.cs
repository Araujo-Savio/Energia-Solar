using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SolarEnergy.ViewModels
{
    public class CompanySimulationInputViewModel
    {
        [Display(Name = "Empresa")]
        [Required]
        public string SelectedCompanyId { get; set; } = string.Empty;

        public SimulationViewModel Simulation { get; set; } = new();

        public IList<CompanySimulationOptionViewModel> AvailableCompanies { get; set; } = new List<CompanySimulationOptionViewModel>();

        public CompanyCostSummaryViewModel? CompanyCosts { get; set; }
    }

    public class CompanySimulationOptionViewModel
    {
        public string CompanyId { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string? Location { get; set; }
        public string? ServiceType { get; set; }
    }
}
