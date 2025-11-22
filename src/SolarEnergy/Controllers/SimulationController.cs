using System;
using System.Text;
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
        public IActionResult ExportUserSimulationCsv(SimulationViewModel model)
        {
            if (User.IsInRole("Company"))
            {
                return Forbid();
            }

            HydrateCompanyParameters(model);
            var input = _userSimulationMapper.ToInput(model);
            var result = _simulationService.Calculate(input);
            var csv = _simulationExportService.GenerateUserCsv(input, result);

            var bytes = Encoding.UTF8.GetBytes(csv);
            var fileName = $"simulacao-usuario-{DateTime.UtcNow:yyyyMMddHHmmss}.csv";

            return File(bytes, "text/csv", fileName);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ExportUserSimulationPdf(SimulationViewModel model)
        {
            if (User.IsInRole("Company"))
            {
                return Forbid();
            }

            HydrateCompanyParameters(model);
            var input = _userSimulationMapper.ToInput(model);
            var result = _simulationService.Calculate(input);
            var pdfBytes = _simulationExportService.GenerateUserPdf(input, result);
            var fileName = $"simulacao-usuario-{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";

            return File(pdfBytes, "application/pdf", fileName);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CalculateUserSimulation([FromBody] SimulationViewModel model)
        {
            if (User.IsInRole("Company"))
            {
                return Forbid();
            }

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
