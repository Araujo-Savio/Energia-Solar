using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SolarEnergy.Models;
using SolarEnergy.Services;
using SolarEnergy.ViewModels;

// using SolarEnergy.ViewModels; // remova se não for usado
using System;

namespace SolarEnergy.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IReportService _reportService;
        private readonly ILogger<ReportsController> _logger;

        public ReportsController(
            UserManager<ApplicationUser> userManager,
            IReportService reportService,
            ILogger<ReportsController> logger)
        {
            _userManager = userManager;
            _reportService = reportService;
            _logger = logger;
        }

        public async Task<IActionResult> MonthlyReport(DateTime? month)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null || user.UserType != UserType.Company)
            {
                TempData["ErrorMessage"] = "Relatórios disponíveis apenas para empresas.";
                return RedirectToAction("SearchCompanies", "Home");
            }

            ViewData["UserType"] = user.UserType;

            var reportMonth = month ?? DateTime.Now;
            var report = await _reportService.GenerateMonthlyReportAsync(user.Id, reportMonth);

            return View(report);
        }

        [HttpPost]
        public async Task<IActionResult> ExportReport([FromBody] ReportExportRequest? request)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null || user.UserType != UserType.Company)
                {
                    return BadRequest("Usuário não autorizado para exportar relatórios");
                }

                if (request is null)
                {
                    return BadRequest("Dados de requisição inválidos");
                }

                byte[]? reportData = null;

                try
                {
                    reportData = await _reportService.ExportReportAsync(user.Id, request);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "ExportReportAsync falhou para userId {UserId}, mês {Month}, formato {Format}. Tentando fallback.",
                        user.Id, request.Month, request.Format);

                    try
                    {
                        var report = await _reportService.GenerateMonthlyReportAsync(user.Id, request.Month);
                        var simpleExportService = new SimpleExportService();

                        if (string.Equals(request.Format, "pdf", StringComparison.OrdinalIgnoreCase))
                        {
                            reportData = await simpleExportService.ExportToPdfAsync(report);
                        }
                        else
                        {
                            reportData = await simpleExportService.ExportToCsvAsync(report);
                        }
                    }
                    catch (Exception fallbackEx)
                    {
                        _logger.LogError(fallbackEx,
                            "Fallback simples também falhou para userId {UserId}.", user.Id);
                        return BadRequest($"Falha na geração do relatório: {fallbackEx.Message}");
                    }
                }

                if (reportData is null || reportData.Length == 0)
                {
                    return BadRequest("Falha na geração do relatório - dados vazios");
                }

                var ext = (request.Format ?? "pdf").ToLowerInvariant();
                var fileName =
                    $"relatorio-{request.Month:yyyy-MM}-{(user.CompanyTradeName ?? "empresa").Replace(" ", "-")}.{ext}";

                var contentType = ext switch
                {
                    "pdf" => "application/pdf",
                    "csv" => "text/csv; charset=utf-8",
                    _ => "application/octet-stream"
                };

                // Se o "PDF" for um HTML de fallback curto, ajusta o content-type e extensão
                if (string.Equals(ext, "pdf", StringComparison.OrdinalIgnoreCase) && reportData.Length < 10000)
                {
                    contentType = "text/html; charset=utf-8";
                    fileName = fileName.Replace(".pdf", ".html");
                }

                // NÃO adiciona header manualmente (evita ASP0019 e duplicidade)
                return File(reportData!, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado em ExportReport");
                return BadRequest($"Erro na exportação: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ExportReportGet(DateTime month, string format = "pdf")
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null || user.UserType != UserType.Company)
                {
                    return Forbid();
                }

                var request = new ReportExportRequest
                {
                    Month = month,
                    Format = format,
                    IncludeCharts = true,
                    IncludeReviews = true
                };

                var reportData = await _reportService.ExportReportAsync(user.Id, request);

                var fileName =
                    $"relatorio-{month:yyyy-MM}-{(user.CompanyTradeName ?? "empresa").Replace(" ", "-")}.{format.ToLowerInvariant()}";

                var contentType = format.Equals("pdf", StringComparison.OrdinalIgnoreCase)
                    ? "application/pdf"
                    : format.Equals("csv", StringComparison.OrdinalIgnoreCase)
                        ? "text/csv; charset=utf-8"
                        : "application/octet-stream";

                return File(reportData, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro em ExportReportGet para {Month}", month);
                TempData["ErrorMessage"] = $"Erro ao exportar relatório: {ex.Message}";
                return RedirectToAction("MonthlyReport", new { month });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetMonthlyData(DateTime month)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.UserType != UserType.Company)
            {
                return Json(new { success = false, message = "Acesso negado" });
            }

            try
            {
                var report = await _reportService.GenerateMonthlyReportAsync(user.Id, month);
                return Json(new { success = true, data = report });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar dados mensais para {Month}", month);
                return Json(new { success = false, message = $"Erro ao carregar dados: {ex.Message}" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAvailableMonths()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.UserType != UserType.Company)
            {
                return Json(new { success = false, message = "Acesso negado" });
            }

            try
            {
                var months = await _reportService.GetAvailableMonthsAsync(user.Id);
                return Json(new { success = true, data = months });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter meses disponíveis");
                return Json(new { success = false, message = $"Erro: {ex.Message}" });
            }
        }
    }
}
