using SolarEnergy.ViewModels;

namespace SolarEnergy.Services
{
    public class CompanySimulationMapper : ICompanySimulationMapper
    {
        public CompanySimulationInput ToInput(SimulationViewModel model)
        {
            var userInput = model.UserInput ?? new SimulationInputModel();

            return new CompanySimulationInput
            {
                AverageMonthlyConsumptionKwh = userInput.MonthlyConsumption,
                TariffPerKwh = userInput.EnergyTariff,
                CoveragePercent = userInput.Coverage,
                DegradationPercent = userInput.Degradation,
                HorizonYears = userInput.HorizonYears,
                InflationPercent = userInput.Inflation,
                CompanyParameters = model.CompanyParameters,
                SelectedCompanyId = model.SelectedCompanyId,
                SelectedCompanyName = model.SelectedCompanyName
            };
        }
    }
}
