using SolarEnergy.ViewModels;

namespace SolarEnergy.Services
{
    public class CompanySimulationService : ICompanySimulationService
    {
        private readonly IUserSimulationService _userSimulationService;

        public CompanySimulationService(IUserSimulationService userSimulationService)
        {
            _userSimulationService = userSimulationService;
        }

        public CompanySimulationResult Calculate(CompanySimulationInput input)
        {
            var userResult = _userSimulationService.Calculate(input);

            return new CompanySimulationResult
            {
                Input = userResult.Input,
                CompanyParameters = userResult.CompanyParameters,
                SelectedCompanyId = userResult.SelectedCompanyId,
                SelectedCompanyName = userResult.SelectedCompanyName,
                InstallationSavingsTimeline = userResult.InstallationSavingsTimeline,
                RentalSavingsTimeline = userResult.RentalSavingsTimeline,
                TimelineLabels = userResult.TimelineLabels,
                CostWithoutSolar = userResult.CostWithoutSolar,
                InstallationInvestment = userResult.InstallationInvestment,
                RentCost = userResult.RentCost,
                InstallationSavings = userResult.InstallationSavings,
                RentSavings = userResult.RentSavings,
                AnnualGeneratedEnergyKwh = userResult.AnnualGeneratedEnergyKwh,
                MonthlySavingInstallation = userResult.MonthlySavingInstallation,
                MonthlySavingRent = userResult.MonthlySavingRent,
                AverageAnnualSaving = userResult.AverageAnnualSaving,
                PaybackYears = userResult.PaybackYears,
                InstallationTimeMonths = userResult.InstallationTimeMonths,
                InitialRentAmount = userResult.InitialRentAmount,
                DiscountAppliedPercent = userResult.DiscountAppliedPercent,
                FiveYearAccumulatedSaving = userResult.FiveYearAccumulatedSaving,
                TotalInstallCost = userResult.TotalInstallCost,
                TotalRentalCost = userResult.TotalRentalCost,
                CoveragePercent = userResult.CoveragePercent,
                AnalyzedHorizonYears = userResult.AnalyzedHorizonYears,
                InitialInvestment = userResult.InitialInvestment,
                InstallationTotalSavingHorizon = userResult.InstallationTotalSavingHorizon,
                RentTotalSavingHorizon = userResult.RentTotalSavingHorizon,
                InvestmentRecoveryYears = userResult.InvestmentRecoveryYears
            };
        }
    }
}
