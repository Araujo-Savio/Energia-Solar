using SolarEnergy.ViewModels;

namespace SolarEnergy.Services
{
    public interface ICompanySimulationMapper
    {
        CompanySimulationInput ToInput(SimulationViewModel model);
    }
}
