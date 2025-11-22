using SolarEnergy.ViewModels;

namespace SolarEnergy.Services
{
    public interface ICompanySimulationService
    {
        CompanySimulationResult Calculate(CompanySimulationInput input);
    }
}
