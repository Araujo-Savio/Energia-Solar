using System;
using System.Collections.Generic;
using System.Linq;
using SolarEnergy.ViewModels;

namespace SolarEnergy.Services
{
    public class EnergySimulationService : ISimulationService
    {
        private const double ProductionPerKilowattPeak = 140.0; // kWh gerados por kWp ao mês (média nacional)
        private const double InstallationCostPerKilowatt = 5200.0; // Custo médio R$/kWp instalado
        private const double MaintenanceRate = 0.015; // Percentual anual de manutenção sobre o investimento
        private const double RentalRatePerKwh = 0.65; // Valor médio do aluguel por kWh gerado
        private const double RentalAnnualIncrease = 0.03; // Reajuste médio anual do aluguel
        private const double EmissionFactorKgPerKwh = 0.084; // kg de CO2 evitado por kWh de energia solar

        public SimulationScenarioResult CalculateInstallation(SimulationScenarioInput input, CompanyCostSummaryViewModel? companyCosts = null)
        {
            var coverageFactor = input.Coverage / 100.0;
            var productionFactor = companyCosts?.ProductionPerKilowattPeak ?? ProductionPerKilowattPeak;
            var monthlyGeneration = input.MonthlyConsumption * coverageFactor;
            var systemSize = productionFactor > 0 ? monthlyGeneration / productionFactor : 0;

            var installationCost = CalculateInstallationCost(systemSize, companyCosts);
            var maintenanceRate = (double)(companyCosts?.MaintenanceRate ?? (decimal)MaintenanceRate);
            var annualMaintenance = installationCost * maintenanceRate;

            var monthlyGrossSavings = monthlyGeneration * input.EnergyTariff;
            var monthlyNetSavings = monthlyGrossSavings - annualMaintenance / 12.0;

            var annualGrossSavings = monthlyGrossSavings * 12.0;
            var annualNetSavings = annualGrossSavings - annualMaintenance;

            var annualNetTimeline = new List<double>();
            double totalNetSavings = 0;
            double totalGrossSavings = 0;
            double totalMaintenance = 0;
            double cumulativeNet = -installationCost;
            double? payback = null;
            double currentTariff = input.EnergyTariff;

            for (int year = 1; year <= input.HorizonYears; year++)
            {
                double grossSavingsYear = monthlyGeneration * 12.0 * currentTariff;
                double maintenanceYear = annualMaintenance;
                double netSavingsYear = grossSavingsYear - maintenanceYear;

                annualNetTimeline.Add(netSavingsYear);
                totalGrossSavings += grossSavingsYear;
                totalMaintenance += maintenanceYear;
                totalNetSavings += netSavingsYear;

                double previousCumulative = cumulativeNet;
                cumulativeNet += netSavingsYear;
                if (!payback.HasValue && cumulativeNet >= 0)
                {
                    if (netSavingsYear > 0)
                    {
                        double fraction = (installationCost - previousCumulative) / netSavingsYear;
                        payback = (year - 1) + fraction;
                    }
                    else
                    {
                        payback = year;
                    }
                }

                currentTariff *= 1 + (input.AnnualTariffIncrease / 100.0);
            }

            double lastYearMonthlyNetSavings = annualNetTimeline.Count > 0 ? annualNetTimeline[^1] / 12.0 : 0;
            double totalEnergyGenerated = monthlyGeneration * 12.0 * input.HorizonYears;

            return new SimulationScenarioResult
            {
                Scenario = "installation",
                MonthlyConsumption = input.MonthlyConsumption,
                Coverage = input.Coverage,
                HorizonYears = input.HorizonYears,
                MonthlyGeneration = monthlyGeneration,
                SystemSizeKw = systemSize,
                Investment = installationCost,
                MonthlyCost = annualMaintenance / 12.0,
                AnnualCost = annualMaintenance,
                TotalScenarioCosts = installationCost + totalMaintenance,
                MonthlyGrossSavings = monthlyGrossSavings,
                MonthlyNetSavings = monthlyNetSavings,
                AnnualGrossSavings = annualGrossSavings,
                AnnualNetSavings = annualNetSavings,
                TotalGrossSavings = totalGrossSavings,
                TotalNetSavings = totalNetSavings,
                NetGain = totalNetSavings - installationCost,
                LastYearMonthlyNetSavings = lastYearMonthlyNetSavings,
                EmissionOffsetTons = (totalEnergyGenerated * EmissionFactorKgPerKwh) / 1000.0,
                PaybackYears = payback ?? double.NaN,
                AnnualNetSavingsTimeline = annualNetTimeline
            };
        }

        public SimulationScenarioResult CalculateRental(SimulationScenarioInput input, CompanyCostSummaryViewModel? companyCosts = null)
        {
            var coverageFactor = input.Coverage / 100.0;
            var productionFactor = companyCosts?.ProductionPerKilowattPeak ?? ProductionPerKilowattPeak;
            var monthlyGeneration = input.MonthlyConsumption * coverageFactor;
            var systemSize = productionFactor > 0 ? monthlyGeneration / productionFactor : 0;

            var rentalRate = (double)(companyCosts?.RentalRatePerKwh ?? (decimal)RentalRatePerKwh);
            var monthlyRentalCost = monthlyGeneration * rentalRate;
            var annualRentalCost = monthlyRentalCost * 12.0;

            var monthlyGrossSavings = monthlyGeneration * input.EnergyTariff;
            var monthlyNetSavings = monthlyGrossSavings - monthlyRentalCost;

            var annualGrossSavings = monthlyGrossSavings * 12.0;
            var annualNetSavings = annualGrossSavings - annualRentalCost;

            var annualNetTimeline = new List<double>();
            double totalGrossSavings = 0;
            double totalRentalCost = 0;
            double totalNetSavings = 0;
            double currentTariff = input.EnergyTariff;
            double currentRentalRate = rentalRate;
            double rentalAnnualIncrease = (double)(companyCosts?.RentalAnnualIncrease ?? (decimal)RentalAnnualIncrease);

            for (int year = 1; year <= input.HorizonYears; year++)
            {
                double grossSavingsYear = monthlyGeneration * 12.0 * currentTariff;
                double rentalCostYear = monthlyGeneration * 12.0 * currentRentalRate;
                double netSavingsYear = grossSavingsYear - rentalCostYear;

                annualNetTimeline.Add(netSavingsYear);
                totalGrossSavings += grossSavingsYear;
                totalRentalCost += rentalCostYear;
                totalNetSavings += netSavingsYear;

                currentTariff *= 1 + (input.AnnualTariffIncrease / 100.0);
                currentRentalRate *= 1 + rentalAnnualIncrease;
            }

            double lastYearMonthlyNetSavings = annualNetTimeline.Count > 0 ? annualNetTimeline[^1] / 12.0 : 0;
            double totalEnergyGenerated = monthlyGeneration * 12.0 * input.HorizonYears;

            return new SimulationScenarioResult
            {
                Scenario = "rental",
                MonthlyConsumption = input.MonthlyConsumption,
                Coverage = input.Coverage,
                HorizonYears = input.HorizonYears,
                MonthlyGeneration = monthlyGeneration,
                SystemSizeKw = systemSize,
                Investment = 0,
                MonthlyCost = monthlyRentalCost,
                AnnualCost = annualRentalCost,
                TotalScenarioCosts = totalRentalCost,
                MonthlyGrossSavings = monthlyGrossSavings,
                MonthlyNetSavings = monthlyNetSavings,
                AnnualGrossSavings = annualGrossSavings,
                AnnualNetSavings = annualNetSavings,
                TotalGrossSavings = totalGrossSavings,
                TotalNetSavings = totalNetSavings,
                NetGain = totalNetSavings,
                LastYearMonthlyNetSavings = lastYearMonthlyNetSavings,
                EmissionOffsetTons = (totalEnergyGenerated * EmissionFactorKgPerKwh) / 1000.0,
                PaybackYears = double.NaN,
                AnnualNetSavingsTimeline = annualNetTimeline
            };
        }

        private static double CalculateInstallationCost(double systemSize, CompanyCostSummaryViewModel? companyCosts)
        {
            if (systemSize <= 0)
            {
                return 0;
            }

            double baseCost = systemSize * InstallationCostPerKilowatt;

            if (companyCosts != null)
            {
                if (companyCosts.SystemSizeCosts?.Count > 0)
                {
                    var ordered = companyCosts.SystemSizeCosts
                        .OrderBy(c => c.SystemSizeKwp)
                        .ToList();

                    var exact = ordered.FirstOrDefault(c => Math.Abs((double)c.SystemSizeKwp - systemSize) < 0.001);
                    if (exact != null)
                    {
                        baseCost = (double)exact.AverageCost;
                    }
                    else
                    {
                        var lower = ordered.LastOrDefault(c => (double)c.SystemSizeKwp < systemSize);
                        var higher = ordered.FirstOrDefault(c => (double)c.SystemSizeKwp > systemSize);

                        if (lower != null && higher != null && Math.Abs((double)higher.SystemSizeKwp - (double)lower.SystemSizeKwp) > 0.001)
                        {
                            var ratio = (systemSize - (double)lower.SystemSizeKwp) / ((double)higher.SystemSizeKwp - (double)lower.SystemSizeKwp);
                            baseCost = (double)(lower.AverageCost + (decimal)ratio * (higher.AverageCost - lower.AverageCost));
                        }
                        else
                        {
                            var reference = higher ?? lower;
                            if (reference != null && reference.SystemSizeKwp > 0)
                            {
                                var costPerKwp = reference.AverageCost / reference.SystemSizeKwp;
                                baseCost = (double)(costPerKwp * (decimal)systemSize);
                            }
                        }
                    }
                }

                var additionalCosts = 0m;

                if (companyCosts.EquipmentCosts != null)
                {
                    additionalCosts += companyCosts.EquipmentCosts.Where(c => c.IsActive).Sum(c => c.Cost);
                }

                if (companyCosts.ServiceCosts != null)
                {
                    additionalCosts += companyCosts.ServiceCosts.Where(c => c.IsActive).Sum(c => c.Cost);
                }

                baseCost += (double)additionalCosts;
            }

            return baseCost;
        }

        public IReadOnlyList<EconomyProjectionPoint> BuildProjection(SimulationScenarioResult installation, SimulationScenarioResult rental)
        {
            var points = new List<EconomyProjectionPoint>();
            double installationCumulative = -installation.Investment;
            double rentalCumulative = 0;
            points.Add(new EconomyProjectionPoint
            {
                Year = 0,
                InstallationCumulative = installationCumulative,
                RentalCumulative = rentalCumulative
            });

            int maxYears = Math.Max(installation.AnnualNetSavingsTimeline.Count, rental.AnnualNetSavingsTimeline.Count);
            for (int year = 1; year <= maxYears; year++)
            {
                if (year <= installation.AnnualNetSavingsTimeline.Count)
                {
                    installationCumulative += installation.AnnualNetSavingsTimeline[year - 1];
                }

                if (year <= rental.AnnualNetSavingsTimeline.Count)
                {
                    rentalCumulative += rental.AnnualNetSavingsTimeline[year - 1];
                }

                points.Add(new EconomyProjectionPoint
                {
                    Year = year,
                    InstallationCumulative = installationCumulative,
                    RentalCumulative = rentalCumulative
                });
            }

            return points;
        }

        public SimulationComparisonResult BuildComparison(SimulationScenarioResult installation, SimulationScenarioResult rental)
        {
            double difference = installation.NetGain - rental.NetGain;
            string suggestedOption = difference >= 0 ? "Instalação própria" : "Aluguel do sistema";

            return new SimulationComparisonResult
            {
                InstallationNetGain = installation.NetGain,
                RentalNetGain = rental.NetGain,
                Difference = difference,
                SuggestedOption = suggestedOption,
                InstallationPaybackYears = double.IsNaN(installation.PaybackYears) ? null : installation.PaybackYears,
                InstallationInvestment = installation.Investment,
                RentalTotalCosts = rental.TotalScenarioCosts
            };
        }
    }
}
