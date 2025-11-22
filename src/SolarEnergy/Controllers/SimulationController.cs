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

        public SimulationController(IUserSimulationService simulationService, ICompanySimulationService companySimulationService,
            ISimulationExportService simulationExportService, IUserSimulationMapper userSimulationMapper,
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
        public IActionResult UserSimulator(SimulationViewModel model)
        {
            // Mantém compatibilidade com o formulário do simulador do usuário.
            return RedirectToAction("Simulation", "Home", new { companyId = model.SelectedCompanyId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ExportSimulationPdf(SimulationViewModel model)
        {
            HydrateCompanyParameters(model);

            SimulationPdfViewModel pdfModel;

            if (!model.IsCompanyUser)
            {
                var userInput = _userSimulationMapper.ToInput(model);
                var userResult = _simulationService.Calculate(userInput);

                pdfModel = new SimulationPdfViewModel
                {
                    IsCompanyUser = false,
                    SelectedCompanyName = model.SelectedCompanyName,
                    UserInput = userInput,
                    CompanyParameters = model.CompanyParameters,
                    UserResult = userResult
                };
            }
            else
            {
                var companyInput = _companySimulationMapper.ToInput(model);
                var companyResult = _companySimulationService.Calculate(companyInput);

                pdfModel = new SimulationPdfViewModel
                {
                    IsCompanyUser = true,
                    SelectedCompanyName = model.SelectedCompanyName,
                    CompanyInput = companyInput,
                    CompanyParameters = model.CompanyParameters,
                    CompanyResult = companyResult
                };
            }

            var pdfBytes = _simulationExportService.GenerateSimulationPdf(pdfModel);
            var fileName = $"simulacao-{(pdfModel.IsCompanyUser ? "empresa" : "usuario")}-{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";

            return File(pdfBytes, "application/pdf", fileName);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CalculateUserSimulation([FromBody] SimulationViewModel model)
        {
            HydrateCompanyParameters(model);
            var input = _userSimulationMapper.ToInput(model);
            var result = _simulationService.Calculate(input);

            return Ok(result);
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
    }
}
