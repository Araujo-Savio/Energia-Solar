using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolarEnergy.Services;
using SolarEnergy.ViewModels;
using System.Text.Json;
using System.Globalization;
using System.Linq;

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
            // Garante que os parâmetros da empresa estejam preenchidos
            HydrateCompanyParameters(model);

            // Lê os valores originais do formulário. O <input type="number"> envia "0.92",
            // então precisamos usar CultureInfo.InvariantCulture para obter 0.92, 0.7 e 7.5.
            double? tariffFromForm = null;
            double? degradationFromForm = null;
            double? inflationFromForm = null;

            if (Request.HasFormContentType)
            {
                var form = Request.Form;

                if (form.TryGetValue(nameof(model.TariffPerKwh), out var tariffStr) &&
                    double.TryParse(tariffStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var tariffParsed))
                {
                    tariffFromForm = tariffParsed;              // ex.: 0.92
                }

                if (form.TryGetValue(nameof(model.DegradationPercent), out var degrStr) &&
                    double.TryParse(degrStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var degrParsed))
                {
                    degradationFromForm = degrParsed;          // ex.: 0.7
                }

                if (form.TryGetValue(nameof(model.TariffInflationPercent), out var inflStr) &&
                    double.TryParse(inflStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var inflParsed))
                {
                    inflationFromForm = inflParsed;            // ex.: 7.5
                }
            }

            SimulationPdfViewModel pdfModel;

            if (!model.IsCompanyUser)
            {
                // Input de simulação para USUÁRIO
                var userInput = _userSimulationMapper.ToInput(model);

                // Corrige escala no input (tarifa, degradação, inflação)
                FixInputScale(userInput, tariffFromForm, degradationFromForm, inflationFromForm);

                var userResult = _simulationService.Calculate(userInput);

                pdfModel = new SimulationPdfViewModel
                {
                    IsCompanyUser = false,
                    SelectedCompanyName = model.SelectedCompanyName,

                    // Valores da calculadora que você já usa no PDF
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
            else
            {
                // Input de simulação para EMPRESA
                var companyInput = _companySimulationMapper.ToInput(model);

                // Mesma correção no input da empresa
                FixInputScale(companyInput, tariffFromForm, degradationFromForm, inflationFromForm);

                var companyResult = _companySimulationService.Calculate(companyInput);


                pdfModel = new SimulationPdfViewModel
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

            var pdfBytes = _simulationExportService.GenerateSimulationPdf(pdfModel);
            var fileName = $"simulacao-{(pdfModel.IsCompanyUser ? "empresa" : "usuario")}-{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";

            return File(pdfBytes, "application/pdf", fileName);
        }


        /// <summary>
        /// Corrige a escala de tarifa, degradação e inflação no input de simulação.
        /// Funciona tanto para UserSimulationInput quanto para CompanySimulationInput.
        /// </summary>
        private static void FixInputScale(object input, double? tariff, double? degradation, double? inflation)
        {
            if (input == null) return;

            var type = input.GetType();

            // Tarifa (TariffPerKwh)
            if (tariff.HasValue)
            {
                var prop = type.GetProperty("TariffPerKwh");
                if (prop != null && prop.CanWrite)
                {
                    var value = Convert.ChangeType(tariff.Value, prop.PropertyType, CultureInfo.InvariantCulture);
                    prop.SetValue(input, value);
                }
            }

            // Degradação (DegradationPercent)
            if (degradation.HasValue)
            {
                var prop = type.GetProperty("DegradationPercent");
                if (prop != null && prop.CanWrite)
                {
                    var value = Convert.ChangeType(degradation.Value, prop.PropertyType, CultureInfo.InvariantCulture);
                    prop.SetValue(input, value);
                }
            }

            // Inflação: procura qualquer propriedade que contenha "Inflation" no nome
            if (inflation.HasValue)
            {
                var prop = type
                    .GetProperties()
                    .FirstOrDefault(p =>
                        p.Name.Contains("Inflation", StringComparison.OrdinalIgnoreCase) &&
                        (p.PropertyType == typeof(double) || p.PropertyType == typeof(decimal)));

                if (prop != null && prop.CanWrite)
                {
                    var value = Convert.ChangeType(inflation.Value, prop.PropertyType, CultureInfo.InvariantCulture);
                    prop.SetValue(input, value);
                }
            }
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

