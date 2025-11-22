using SolarEnergy.ViewModels;

namespace SolarEnergy.Services
{
    public class UserSimulationMapper : IUserSimulationMapper
    {
        public UserSimulationInput ToInput(SimulationViewModel model)
        {
            var userInput = model.UserInput ?? new SimulationInputModel();

            return new UserSimulationInput
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
