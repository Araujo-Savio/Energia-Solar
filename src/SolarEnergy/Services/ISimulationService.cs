using System.Collections.Generic;
using SolarEnergy.ViewModels;

namespace SolarEnergy.Services
{
    public interface ISimulationService
    {
        SimulationScenarioResult CalculateInstallation(SimulationScenarioInput input, CompanyCostSummaryViewModel? companyCosts = null);
        SimulationScenarioResult CalculateRental(SimulationScenarioInput input, CompanyCostSummaryViewModel? companyCosts = null);
        IReadOnlyList<EconomyProjectionPoint> BuildProjection(SimulationScenarioResult installation, SimulationScenarioResult rental);
        SimulationComparisonResult BuildComparison(SimulationScenarioResult installation, SimulationScenarioResult rental);
    }
}
