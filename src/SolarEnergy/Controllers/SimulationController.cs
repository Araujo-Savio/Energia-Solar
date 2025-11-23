using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolarEnergy.Services;
using SolarEnergy.ViewModels;
using System.Text.Json;

namespace SolarEnergy.Controllers
{
    [Authorize]
    public class SimulationController : Controller
    {
        private readonly IUserSimulationService _simulationService;
        private readonly ICompanySimulationService _companySimulationService;
        private readonly ISimulationExportService _simulationExportService;
        private readonly IUserSimulationMapper _userSimulationMapper;
        private readonly ICompanySimulationMapper _companySimulationMapper;

        public SimulationController(
            IUserSimulationService simulationService,
            ICompanySimulationService companySimulationService,
            ISimulationExportService simulationExportService,
            IUserSimulationMapper userSimulationMapper,
            ICompanySimulationMapper companySimulationMapper)
        {
            _simulationService = simulationService;
            _companySimulationService = companySimulationService;
            _simulationExportService = simulationExportService;
            _userSimulationMapper = userSimulationMapper;
            _companySimulationMapper = companySimulationMapper;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ExportSimulationPdf(SimulationViewModel model)
        {
            HydrateCompanyParameters(model);

            var resultados = MapResultadosSimulacao(model);
            if (resultados is null)
            {
                return BadRequest("Resultados da simulação não foram enviados.");
            }

            var pdfBytes = _simulationExportService.ExportarSimulacaoParaPdf(resultados);
            var fileName = $"simulacao-{(resultados.IsCompanyUser ? "empresa" : "usuario")}-{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";

            return File(pdfBytes, "application/pdf", fileName);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CalculateUserSimulation([FromBody] SimulationViewModel model)
        {
            HydrateCompanyParameters(model);

            var input = _userSimulationMapper.ToInput(model);

            // Na chamada AJAX normalmente os valores já vêm corretos (0,92, 0,7, 7,5),
            // mas a normalização é idempotente, então é seguro aplicar aqui também.
            NormalizeSimulationInput(input);

            var result = _simulationService.Calculate(input);

            return Ok(result);
        }

        /// <summary>
        /// Normaliza campos de input (tanto de usuário quanto de empresa) quando
        /// eles vierem em escala inteira (ex.: 0,92 → 92; 0,7 → 7; 7,5 → 75).
        /// Usa dynamic para não depender do tipo concreto do input.
        /// Se o tipo não tiver as propriedades esperadas, simplesmente ignora.
        /// </summary>
        private static void NormalizeSimulationInput(dynamic input)
        {
            if (input is null) return;

            try
            {
                // Tarifa tipicamente é algo como 0,xx ou 1,xx; se vier > 10, assumimos centavos.
                if (input.TariffPerKwh > 10m)
                {
                    input.TariffPerKwh = input.TariffPerKwh / 100m;  // 92 -> 0,92
                }

                // Degradação anual costuma ser baixa (0–5%); se vier > 1, assumimos décimos.
                if (input.DegradationPercent > 1m)
                {
                    input.DegradationPercent = input.DegradationPercent / 10m; // 7 -> 0,7
                }

                // Inflação anual costuma ser algo como 7,5; se vier muito alta (> 20),
                // assumimos décimos (75 -> 7,5).
                if (input.TariffInflationPercent > 20m)
                {
                    input.TariffInflationPercent = input.TariffInflationPercent / 10m; // 75 -> 7,5
                }
            }
            catch
            {
                // Se por algum motivo o tipo não tiver essas propriedades, só ignora.
            }
        }

        private static SimulationResultsDto? MapResultadosSimulacao(SimulationViewModel model)
        {
            var simulationJson = model.SimulationResultJson?.Trim();
            if (string.IsNullOrWhiteSpace(simulationJson) || simulationJson == "{}" || simulationJson.Equals("null", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            if (model.IsCompanyUser)
            {
                var companyResult = JsonSerializer.Deserialize<CompanySimulationResult>(simulationJson, jsonOptions);
                if (companyResult is null)
                {
                    return null;
                }

                return new SimulationResultsDto
                {
                    IsCompanyUser = true,
                    SelectedCompanyName = model.SelectedCompanyName,
                    CalculatorAverageMonthlyConsumptionKwh = model.AverageMonthlyConsumptionKwh,
                    CalculatorTariffPerKwh = model.TariffPerKwh,
                    CalculatorCoveragePercent = model.CoveragePercent,
                    CalculatorDegradationPercent = model.DegradationPercent,
                    CalculatorHorizonYears = model.HorizonYears,
                    CalculatorTariffInflationPercent = model.TariffInflationPercent,
                    CompanyParameters = MapCompanyParameters(model.CompanyParameters),
                    CompanyResult = companyResult
                };
            }

            var userResult = JsonSerializer.Deserialize<UserSimulationResult>(simulationJson, jsonOptions);
            if (userResult is null)
            {
                return null;
            }

            return new SimulationResultsDto
            {
                IsCompanyUser = false,
                SelectedCompanyName = model.SelectedCompanyName,
                CalculatorAverageMonthlyConsumptionKwh = model.AverageMonthlyConsumptionKwh,
                CalculatorTariffPerKwh = model.TariffPerKwh,
                CalculatorCoveragePercent = model.CoveragePercent,
                CalculatorDegradationPercent = model.DegradationPercent,
                CalculatorHorizonYears = model.HorizonYears,
                CalculatorTariffInflationPercent = model.TariffInflationPercent,
                CompanyParameters = MapCompanyParameters(model.CompanyParameters),
                UserResult = userResult
            };
        }

        private static void HydrateCompanyParameters(SimulationViewModel model)
        {
            if (model.CompanyParameters is not null)
            {
                return;
            }

            var json = model.CompanyParametersJson;
            if (string.IsNullOrWhiteSpace(json) || json.Trim().Equals("null", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            try
            {
                model.CompanyParameters = JsonSerializer.Deserialize<CompanyParametersInputModel>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch
            {
                model.CompanyParameters = null;
            }
        }

        private static CompanyParametersViewModel? MapCompanyParameters(CompanyParametersInputModel? input)
        {
            if (input is null)
            {
                return null;
            }

            return new CompanyParametersViewModel
            {
                SystemPricePerKwp = input.SystemPricePerKwp,
                MaintenancePercent = input.MaintenancePercent,
                InstallDiscountPercent = input.InstallDiscountPercent,
                RentalFactorPercent = input.RentalFactorPercent,
                RentalSetupPerKwp = input.RentalSetupPerKwp,
                RentalMinMonthly = input.RentalMinMonthly,
                RentalAnnualIncreasePercent = input.RentalAnnualIncreasePercent,
                RentalDiscountPercent = input.RentalDiscountPercent,
                ConsumptionPerKwp = input.ConsumptionPerKwp,
                MinSystemSizeKwp = input.MinSystemSizeKwp
            };
        }
    }
}

