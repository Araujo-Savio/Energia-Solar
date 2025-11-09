using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using SolarEnergy.Models;
using SolarEnergy.Services;
using SolarEnergy.ViewModels;
using System.Globalization;

namespace SolarEnergy.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IReportService _reportService;

        public ReportsController(UserManager<ApplicationUser> userManager, IReportService reportService)
        {
            _userManager = userManager;
            _reportService = reportService;
        }

        public async Task<IActionResult> MonthlyReport(DateTime? month)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null || user.UserType != UserType.Company)
            {
                TempData["ErrorMessage"] = "Relatórios disponíveis apenas para empresas.";
                return RedirectToAction("SearchCompanies", "Home");
            }

            // Add UserType to ViewData for navbar display
            ViewData["UserType"] = user.UserType;

            var reportMonth = month ?? DateTime.Now;
            var report = await _reportService.GenerateMonthlyReportAsync(user.Id, reportMonth);

            return View(report);
        }

        [HttpPost]
        public async Task<IActionResult> ExportReport([FromBody] ReportExportRequest request)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null || user.UserType != UserType.Company)
                {
                    return BadRequest("Usuário não autorizado para exportar relatórios");
                }

                if (request == null)
                {
                    return BadRequest("Dados de requisição inválidos");
                }

                byte[] reportData = null;
                
                try
                {
                    reportData = await _reportService.ExportReportAsync(user.Id, request);
                }
                catch (Exception ex)
                {
                    // Try to generate a simple fallback report
                    try
                    {
                        var report = await _reportService.GenerateMonthlyReportAsync(user.Id, request.Month);
                        var simpleExportService = new SimpleExportService();
                        
                        if (request.Format.ToLower() == "pdf")
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
                        return BadRequest($"Falha na geração do relatório: {fallbackEx.Message}");
                    }
                }
                
                if (reportData == null || reportData.Length == 0)
                {
                    return BadRequest("Falha na geração do relatório - dados vazios");
                }
                
                var fileName = $"relatorio-{request.Month:yyyy-MM}-{user.CompanyTradeName?.Replace(" ", "-") ?? "empresa"}.{request.Format.ToLower()}";
                var contentType = request.Format.ToLower() switch
                {
                    "pdf" => "application/pdf",
                    "csv" => "text/csv; charset=utf-8",
                    _ => "application/octet-stream"
                };

                // For PDF fallback (HTML), set proper content type
                if (request.Format.ToLower() == "pdf" && reportData.Length < 10000) // Likely HTML fallback
                {
                    contentType = "text/html; charset=utf-8";
                    fileName = fileName.Replace(".pdf", ".html");
                }

                Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{fileName}\"");
                return File(reportData, contentType, fileName);
            }
            catch (Exception ex)
            {
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
                
                var fileName = $"relatorio-{month:yyyy-MM}-{user.CompanyTradeName?.Replace(" ", "-") ?? "empresa"}.{format}";
                var contentType = format.ToLower() switch
                {
                    "pdf" => "application/pdf",
                    "csv" => "text/csv",
                    _ => "application/octet-stream"
                };

                return File(reportData, contentType, fileName);
            }
            catch (Exception ex)
            {
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
                return Json(new { success = false, message = $"Erro: {ex.Message}" });
            }
        }
    }
}