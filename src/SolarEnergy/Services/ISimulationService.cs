using System.Collections.Generic;
using SolarEnergy.ViewModels;

namespace SolarEnergy.Services
{
    public interface ISimulationService
    {
        SimulationScenarioResult CalculateInstallation(SimulationScenarioInput input);
        SimulationScenarioResult CalculateRental(SimulationScenarioInput input);
        IReadOnlyList<EconomyProjectionPoint> BuildProjection(SimulationScenarioResult installation, SimulationScenarioResult rental);
        SimulationComparisonResult BuildComparison(SimulationScenarioResult installation, SimulationScenarioResult rental);
    }
}
