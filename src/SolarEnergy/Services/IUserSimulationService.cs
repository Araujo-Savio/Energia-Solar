using SolarEnergy.ViewModels;

namespace SolarEnergy.Services
{
    public interface IUserSimulationService
    {
        UserSimulationResult Calculate(UserSimulationInput input);
    }
}
