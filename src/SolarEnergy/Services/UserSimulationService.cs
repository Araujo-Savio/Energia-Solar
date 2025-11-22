using System;
using System.Collections.Generic;
using SolarEnergy.ViewModels;

namespace SolarEnergy.Services
{
    public class UserSimulationService : IUserSimulationService
    {
        public UserSimulationResult Calculate(UserSimulationInput input)
        {
            var companyParameters = input.CompanyParameters;

            var timelineLabels = new List<string>();
            var installationSavingsTimeline = new List<double>();
            var rentalSavingsTimeline = new List<double>();

            var monthlyConsumption = Math.Max((decimal)input.AverageMonthlyConsumptionKwh, 0m);
            var energyTariff = Math.Max((decimal)input.TariffPerKwh, 0m);
            var coveragePercent = (decimal)input.CoveragePercent;
            var degradationPercent = (decimal)input.DegradationPercent;
            var horizonYears = Math.Max(input.HorizonYears, 1);
            var inflationPercent = (decimal)input.InflationPercent;

            var scenarioDefaults = DeriveScenarioDefaults(monthlyConsumption, energyTariff, companyParameters);

            var installationCost = scenarioDefaults.InstallationCost;
            var installationTimeMonths = scenarioDefaults.InstallationTimeMonths;
            var maintenanceAnnual = scenarioDefaults.MaintenanceAnnual;
            var installationIncentive = scenarioDefaults.InstallationIncentive;

            var rentalMonthly = scenarioDefaults.RentalMonthly;
            var rentalSetup = scenarioDefaults.RentalSetup;
            var rentalIncreaseRate = scenarioDefaults.RentalIncreaseRate;
            var rentalDiscountRate = scenarioDefaults.RentalDiscountRate;

            var coverage = Math.Min(Math.Max(coveragePercent / 100m, 0m), 1.2m);
            var degradation = Math.Min(Math.Max(degradationPercent / 100m, 0m), 0.3m);
            var inflation = Math.Max(inflationPercent / 100m, 0m);
            var rentalIncrease = Math.Max(rentalIncreaseRate, 0m);
            var rentalDiscount = Math.Min(Math.Max(rentalDiscountRate, 0m), 1m);

            var monthlyTariffWithoutInflation = energyTariff > 0 ? energyTariff : 0m;
            var annualConsumption = monthlyConsumption * 12m;

            decimal cumulativeBase = 0m;
            decimal cumulativeInstall = installationCost - installationIncentive;
            decimal cumulativeRental = rentalSetup;
            decimal annualSavingsInstallSum = 0m;
            decimal annualSavingsRentalSum = 0m;
            decimal fiveYearSavings = 0m;

            decimal? paybackYears = null;
            decimal cumulativeInstallSavings = -cumulativeInstall;
            decimal cumulativeRentalSavings = -cumulativeRental;
            decimal firstYearInstallSavings = 0m;
            decimal firstYearRentalSavings = 0m;

            for (var year = 1; year <= horizonYears; year++)
            {
                var inflationFactor = (decimal)Math.Pow((double)(1 + inflation), year - 1);
                var tariffWithInflation = monthlyTariffWithoutInflation * inflationFactor;

                var coverageForYear = Math.Min(coverage, 1m) * (decimal)Math.Pow((double)(1 - degradation), Math.Max(year - 1, 0));
                var coveredConsumptionMonthly = monthlyConsumption * coverageForYear;
                var remainingConsumptionMonthly = Math.Max(monthlyConsumption - coveredConsumptionMonthly, 0m);
                var remainingConsumptionAnnual = remainingConsumptionMonthly * 12m;

                var baseCostYear = annualConsumption * tariffWithInflation;
                cumulativeBase += baseCostYear;

                var maintenanceYear = maintenanceAnnual * inflationFactor;
                var installCostYear = remainingConsumptionAnnual * tariffWithInflation + maintenanceYear;
                cumulativeInstall += installCostYear;

                var rentalIncreaseFactor = (decimal)Math.Pow((double)(1 + rentalIncrease), year - 1);
                var rentalMonthlyYear = rentalMonthly * rentalIncreaseFactor;
                var rentalDiscountYear = remainingConsumptionAnnual * tariffWithInflation * (1 - rentalDiscount);
                var rentalCostYear = rentalMonthlyYear * 12m + rentalDiscountYear;
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
                    var fractionOfYear = annualSavingsInstall > 0m
                        ? (installationCost - installationIncentive - previousSavings) / annualSavingsInstall
                        : 0m;

                    paybackYears = year - 1 + Math.Max(0m, Math.Min(1m, fractionOfYear));
                }

                timelineLabels.Add($"Ano {year}");
                installationSavingsTimeline.Add((double)cumulativeInstallSavings);
                rentalSavingsTimeline.Add((double)cumulativeRentalSavings);
            }

            var totalBaseCost = cumulativeBase;
            var totalInstallCost = cumulativeInstall;
            var totalRentalCost = cumulativeRental;

            var totalInstallSavings = totalBaseCost - totalInstallCost;
            var totalRentalSavings = totalBaseCost - totalRentalCost;

            var annualGeneration = annualConsumption * Math.Min(coverage, 1m);

            var monthlyInstallSavings = firstYearInstallSavings / 12m;
            var monthlyRentalSavings = firstYearRentalSavings / 12m;
            var averageAnnualSavings = horizonYears > 0 ? annualSavingsInstallSum / horizonYears : 0m;

            return new UserSimulationResult
            {
                Input = input,
                CompanyParameters = companyParameters,
                SelectedCompanyId = input.SelectedCompanyId,
                SelectedCompanyName = input.SelectedCompanyName,
                InstallationSavingsTimeline = installationSavingsTimeline,
                RentalSavingsTimeline = rentalSavingsTimeline,
                TimelineLabels = timelineLabels,
                CostWithoutSolar = (double)totalBaseCost,
                InstallationInvestment = (double)(installationCost - installationIncentive),
                RentCost = (double)totalRentalCost,
                InstallationSavings = (double)totalInstallSavings,
                RentSavings = (double)totalRentalSavings,
                AnnualGeneratedEnergyKwh = (double)annualGeneration,
                MonthlySavingInstallation = (double)monthlyInstallSavings,
                MonthlySavingRent = (double)monthlyRentalSavings,
                AverageAnnualSaving = (double)averageAnnualSavings,
                PaybackYears = (double?)paybackYears,
                InstallationTimeMonths = (double)installationTimeMonths,
                InitialRentAmount = (double)rentalMonthly,
                DiscountAppliedPercent = (double)(rentalDiscount * 100m),
                FiveYearAccumulatedSaving = (double)fiveYearSavings,
                TotalInstallCost = (double)totalInstallCost,
                TotalRentalCost = (double)totalRentalCost,
                CoveragePercent = (double)Math.Min(Math.Max(coveragePercent, 0m), 120m),
                AnalyzedHorizonYears = horizonYears
            };
        }

        private static (decimal InstallationCost, decimal InstallationTimeMonths, decimal MaintenanceAnnual, decimal InstallationIncentive,
            decimal RentalMonthly, decimal RentalSetup, decimal RentalIncreaseRate, decimal RentalDiscountRate)
            DeriveScenarioDefaults(decimal monthlyConsumption, decimal energyTariff, CompanyParametersInputModel? companyParameters)
        {
            var safeConsumption = Math.Max(monthlyConsumption, 0m);
            var safeTariff = Math.Max(energyTariff, 0m);

            var monthlyBill = safeConsumption * safeTariff;
            var systemSizeKw = Math.Max(safeConsumption / 120m, 2.5m);

            var installationCost = systemSizeKw * 4200m;
            var maintenanceAnnual = installationCost * 0.012m;
            var installationIncentive = installationCost * 0.04m;
            var installationTimeMonths = Math.Min(Math.Max(systemSizeKw * 0.8m, 2m), 8m);

            var rentalMonthly = Math.Max(monthlyBill * 0.68m, 250m);
            var rentalSetup = Math.Max(systemSizeKw * 150m, 0m);
            var rentalIncreaseRate = 0.045m;
            var rentalDiscountRate = Math.Min(0.18m, 0.1m + systemSizeKw / 100m);

            if (companyParameters is not null)
            {
                var pricePerKwp = Math.Max(companyParameters.SystemPricePerKwp, 0m);
                var maintenancePercent = Math.Max(companyParameters.MaintenancePercent, 0m);
                var installDiscountPercent = Math.Max(companyParameters.InstallDiscountPercent, 0m);
                var rentalFactorPercent = Math.Max(companyParameters.RentalFactorPercent, 0m);
                var rentalMinMonthly = Math.Max(companyParameters.RentalMinMonthly, 0m);
                var rentalSetupPerKwp = Math.Max(companyParameters.RentalSetupPerKwp, 0m);
                var rentalAnnualIncreasePercent = Math.Max(companyParameters.RentalAnnualIncreasePercent, 0m);
                var rentalDiscountPercent = Math.Max(companyParameters.RentalDiscountPercent, 0m);
                var consumptionPerKwp = Math.Max(companyParameters.ConsumptionPerKwp, 0m);
                var minSystemSizeKwp = Math.Max(companyParameters.MinSystemSizeKwp, 0m);

                var baseSystemSize = consumptionPerKwp > 0 ? safeConsumption / consumptionPerKwp : safeConsumption;
                systemSizeKw = Math.Max(baseSystemSize, minSystemSizeKwp);

                installationCost = systemSizeKw * pricePerKwp;
                maintenanceAnnual = installationCost * maintenancePercent / 100m;
                installationIncentive = installationCost * installDiscountPercent / 100m;
                installationTimeMonths = Math.Min(Math.Max(systemSizeKw * 0.8m, 2m), 8m);

                var rentalFactor = rentalFactorPercent / 100m;
                rentalMonthly = Math.Max(monthlyBill * rentalFactor, rentalMinMonthly);
                rentalSetup = Math.Max(systemSizeKw * rentalSetupPerKwp, 0m);
                rentalIncreaseRate = rentalAnnualIncreasePercent / 100m;
                rentalDiscountRate = Math.Min(rentalDiscountPercent / 100m, 1m);
            }

            return (installationCost, installationTimeMonths, maintenanceAnnual, installationIncentive, rentalMonthly, rentalSetup,
                rentalIncreaseRate, rentalDiscountRate);
        }
    }
}
