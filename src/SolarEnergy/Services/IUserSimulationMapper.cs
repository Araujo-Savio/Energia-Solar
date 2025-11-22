using SolarEnergy.ViewModels;

namespace SolarEnergy.Services
{
    public interface IUserSimulationMapper
    {
        UserSimulationInput ToInput(SimulationViewModel model);
    }
}
