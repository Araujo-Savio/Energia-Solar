using System;
using SolarEnergy.ViewModels;

namespace SolarEnergy.Services
{
    public class UserSimulationService : IUserSimulationService
    {
        public UserSimulationResultViewModel CalculateUserSimulation(SimulationViewModel model)
        {
            var input = model.UserInput ?? new SimulationInputModel();
            var companyParameters = model.CompanyParameters;

            var monthlyConsumption = Math.Max(input.MonthlyConsumption, 0);
            var energyTariff = Math.Max(input.EnergyTariff, 0);
            var coveragePercent = input.Coverage;
            var degradationPercent = input.Degradation;
            var horizonYears = Math.Max(input.HorizonYears, 1);
            var inflationPercent = input.Inflation;

            var scenarioDefaults = DeriveScenarioDefaults(monthlyConsumption, energyTariff, companyParameters);

            var installationCost = scenarioDefaults.InstallationCost;
            var installationTimeMonths = scenarioDefaults.InstallationTimeMonths;
            var maintenanceAnnual = scenarioDefaults.MaintenanceAnnual;
            var installationIncentive = scenarioDefaults.InstallationIncentive;

            var rentalMonthly = scenarioDefaults.RentalMonthly;
            var rentalSetup = scenarioDefaults.RentalSetup;
            var rentalIncreaseRate = scenarioDefaults.RentalIncreaseRate;
            var rentalDiscountRate = scenarioDefaults.RentalDiscountRate;

            var coverage = Math.Min(Math.Max(coveragePercent / 100, 0), 1.2);
            var degradation = Math.Min(Math.Max(degradationPercent / 100, 0), 0.3);
            var inflation = Math.Max(inflationPercent / 100, 0);
            var rentalIncrease = Math.Max(rentalIncreaseRate, 0);
            var rentalDiscount = Math.Min(Math.Max(rentalDiscountRate, 0), 1);

            var monthlyTariffWithoutInflation = energyTariff > 0 ? energyTariff : 0;
            var annualConsumption = monthlyConsumption * 12;

            double cumulativeBase = 0;
            double cumulativeInstall = installationCost - installationIncentive;
            double cumulativeRental = rentalSetup;
            double annualSavingsInstallSum = 0;
            double annualSavingsRentalSum = 0;
            double fiveYearSavings = 0;

            double? paybackYears = null;
            double cumulativeInstallSavings = -cumulativeInstall;
            double cumulativeRentalSavings = -cumulativeRental;
            double firstYearInstallSavings = 0;
            double firstYearRentalSavings = 0;

            for (var year = 1; year <= horizonYears; year++)
            {
                var tariffWithInflation = monthlyTariffWithoutInflation * Math.Pow(1 + inflation, year - 1);

                var coverageForYear = Math.Min(coverage, 1) * Math.Pow(1 - degradation, Math.Max(year - 1, 0));
                var coveredConsumptionMonthly = monthlyConsumption * coverageForYear;
                var remainingConsumptionMonthly = Math.Max(monthlyConsumption - coveredConsumptionMonthly, 0);
                var remainingConsumptionAnnual = remainingConsumptionMonthly * 12;

                var baseCostYear = annualConsumption * tariffWithInflation;
                cumulativeBase += baseCostYear;

                var maintenanceYear = maintenanceAnnual * Math.Pow(1 + inflation, year - 1);
                var installCostYear = remainingConsumptionAnnual * tariffWithInflation + maintenanceYear;
                cumulativeInstall += installCostYear;

                var rentalMonthlyYear = rentalMonthly * Math.Pow(1 + rentalIncrease, year - 1);
                var rentalDiscountYear = remainingConsumptionAnnual * tariffWithInflation * (1 - rentalDiscount);
                var rentalCostYear = rentalMonthlyYear * 12 + rentalDiscountYear;
                cumulativeRental += rentalCostYear;

                var annualSavingsInstall = baseCostYear - installCostYear;
                var annualSavingsRental = baseCostYear - rentalCostYear;

                if (year == 1)
                {
                    firstYearInstallSavings = annualSavingsInstall;
                    firstYearRentalSavings = annualSavingsRental;
                }

                annualSavingsInstallSum += annualSavingsInstall;
                annualSavingsRentalSum += annualSavingsRental;

                if (year <= 5)
                {
                    fiveYearSavings += annualSavingsInstall;
                }

                cumulativeInstallSavings += annualSavingsInstall;
                cumulativeRentalSavings += annualSavingsRental;

                if (paybackYears == null && cumulativeInstallSavings >= 0)
                {
                    var previousSavings = cumulativeInstallSavings - annualSavingsInstall;
                    var fractionOfYear = annualSavingsInstall > 0
                        ? (installationCost - installationIncentive - previousSavings) / annualSavingsInstall
                        : 0;

                    paybackYears = year - 1 + Math.Max(0, Math.Min(1, fractionOfYear));
                }
            }

            var totalBaseCost = cumulativeBase;
            var totalInstallCost = cumulativeInstall;
            var totalRentalCost = cumulativeRental;

            var totalInstallSavings = totalBaseCost - totalInstallCost;
            var totalRentalSavings = totalBaseCost - totalRentalCost;

            var annualGeneration = annualConsumption * Math.Min(coverage, 1);

            var monthlyInstallSavings = firstYearInstallSavings / 12;
            var monthlyRentalSavings = firstYearRentalSavings / 12;
            var averageAnnualSavings = horizonYears > 0 ? annualSavingsInstallSum / horizonYears : 0;

            return new UserSimulationResultViewModel
            {
                Input = input,
                CompanyParameters = companyParameters,
                SelectedCompanyId = model.SelectedCompanyId,
                SelectedCompanyName = model.SelectedCompanyName,
                CostWithoutSolar = totalBaseCost,
                InstallationInvestment = installationCost - installationIncentive,
                RentCost = totalRentalCost,
                InstallationSavings = totalInstallSavings,
                RentSavings = totalRentalSavings,
                AnnualGeneratedEnergyKwh = annualGeneration,
                MonthlyInstallSavings = monthlyInstallSavings,
                MonthlyRentalSavings = monthlyRentalSavings,
                AverageAnnualSavings = averageAnnualSavings,
                PaybackYears = paybackYears,
                InstallationTimeMonths = installationTimeMonths,
                RentalMonthlyCost = rentalMonthly,
                RentalDiscountRate = rentalDiscount,
                FiveYearSavings = fiveYearSavings,
                TotalInstallCost = totalInstallCost,
                TotalRentalCost = totalRentalCost,
                CoveragePercent = Math.Min(coverage, 1) * 100
            };
        }

        private static (double InstallationCost, double InstallationTimeMonths, double MaintenanceAnnual, double InstallationIncentive,
            double RentalMonthly, double RentalSetup, double RentalIncreaseRate, double RentalDiscountRate)
            DeriveScenarioDefaults(double monthlyConsumption, double energyTariff, CompanyParametersInputModel? companyParameters)
        {
            var safeConsumption = Math.Max(monthlyConsumption, 0);
            var safeTariff = Math.Max(energyTariff, 0);

            var monthlyBill = safeConsumption * safeTariff;
            var systemSizeKw = Math.Max(safeConsumption / 120d, 2.5d);

            var installationCost = systemSizeKw * 4200d;
            var maintenanceAnnual = installationCost * 0.012d;
            var installationIncentive = installationCost * 0.04d;
            var installationTimeMonths = Math.Min(Math.Max(systemSizeKw * 0.8d, 2d), 8d);

            var rentalMonthly = Math.Max(monthlyBill * 0.68d, 250d);
            var rentalSetup = Math.Max(systemSizeKw * 150d, 0);
            var rentalIncreaseRate = 0.045d;
            var rentalDiscountRate = Math.Min(0.18d, 0.1d + systemSizeKw / 100d);

            if (companyParameters is not null)
            {
                var pricePerKwp = Math.Max(companyParameters.SystemPricePerKwp, 0);
                var maintenancePercent = Math.Max(companyParameters.MaintenancePercent, 0);
                var installDiscountPercent = Math.Max(companyParameters.InstallDiscountPercent, 0);
                var rentalFactorPercent = Math.Max(companyParameters.RentalFactorPercent, 0);
                var rentalMinMonthly = Math.Max(companyParameters.RentalMinMonthly, 0);
                var rentalSetupPerKwp = Math.Max(companyParameters.RentalSetupPerKwp, 0);
                var rentalAnnualIncreasePercent = Math.Max(companyParameters.RentalAnnualIncreasePercent, 0);
                var rentalDiscountPercent = Math.Max(companyParameters.RentalDiscountPercent, 0);
                var consumptionPerKwp = Math.Max(companyParameters.ConsumptionPerKwp, 0);
                var minSystemSizeKwp = Math.Max(companyParameters.MinSystemSizeKwp, 0);

                var baseSystemSize = consumptionPerKwp > 0 ? safeConsumption / consumptionPerKwp : safeConsumption;
                systemSizeKw = Math.Max(baseSystemSize, minSystemSizeKwp);

                installationCost = systemSizeKw * pricePerKwp;
                maintenanceAnnual = installationCost * maintenancePercent / 100d;
                installationIncentive = installationCost * installDiscountPercent / 100d;
                installationTimeMonths = Math.Min(Math.Max(systemSizeKw * 0.8d, 2d), 8d);

                var rentalFactor = rentalFactorPercent / 100d;
                rentalMonthly = Math.Max(monthlyBill * rentalFactor, rentalMinMonthly);
                rentalSetup = Math.Max(systemSizeKw * rentalSetupPerKwp, 0);
                rentalIncreaseRate = rentalAnnualIncreasePercent / 100d;
                rentalDiscountRate = Math.Min(rentalDiscountPercent / 100d, 1);
            }

            return (installationCost, installationTimeMonths, maintenanceAnnual, installationIncentive, rentalMonthly, rentalSetup,
                rentalIncreaseRate, rentalDiscountRate);
        }
    }
}
