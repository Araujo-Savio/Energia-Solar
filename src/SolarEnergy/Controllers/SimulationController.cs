using System;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolarEnergy.Services;
using SolarEnergy.ViewModels;

namespace SolarEnergy.Controllers
{
    [Authorize]
    public class SimulationController : Controller
    {
        private readonly IUserSimulationService _simulationService;
        private readonly ISimulationExportService _simulationExportService;

        public SimulationController(IUserSimulationService simulationService, ISimulationExportService simulationExportService)
        {
            _simulationService = simulationService;
            _simulationExportService = simulationExportService;
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

            var result = _simulationService.CalculateUserSimulation(model);
            var csv = _simulationExportService.GenerateCsv(result);

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

            var result = _simulationService.CalculateUserSimulation(model);
            var pdfBytes = _simulationExportService.GeneratePdf(result);
            var fileName = $"simulacao-usuario-{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";

            return File(pdfBytes, "application/pdf", fileName);
        }
    }
}
