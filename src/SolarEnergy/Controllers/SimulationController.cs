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
        private readonly ISimulationExportService _simulationExportService;
        private readonly IUserSimulationMapper _userSimulationMapper;

        public SimulationController(IUserSimulationService simulationService, ISimulationExportService simulationExportService,
            IUserSimulationMapper userSimulationMapper)
        {
            _simulationService = simulationService;
            _simulationExportService = simulationExportService;
            _userSimulationMapper = userSimulationMapper;
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
                    UserResult = userResult
                };
            }
            else
            {
                var companyInput = _userSimulationMapper.ToInput(model);
                var companyResult = _simulationService.Calculate(companyInput);

                pdfModel = new SimulationPdfViewModel
                {
                    IsCompanyUser = true,
                    SelectedCompanyName = model.SelectedCompanyName,
                    CompanyInput = companyInput,
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
