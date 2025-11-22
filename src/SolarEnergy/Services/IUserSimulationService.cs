using SolarEnergy.ViewModels;

namespace SolarEnergy.Services
{
    public interface IUserSimulationService
    {
        UserSimulationResultViewModel CalculateUserSimulation(SimulationViewModel model);
    }
}
