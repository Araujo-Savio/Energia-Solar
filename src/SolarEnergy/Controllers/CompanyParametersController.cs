using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SolarEnergy.Data;
using SolarEnergy.Models;
using SolarEnergy.ViewModels;

namespace SolarEnergy.Controllers
{
    [Authorize(Roles = "Company")]
    [AutoValidateAntiforgeryToken]
    public class CompanyParametersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<CompanyParametersController> _logger;

        public CompanyParametersController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<CompanyParametersController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveForCurrentCompany([FromBody] CompanyParametersInputModel? input)
        {
            if (input is null)
            {
                return BadRequest("Parâmetros inválidos.");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user is null)
            {
                return Unauthorized("Usuário não autenticado.");
            }

            var company = await _context.Users
                .SingleOrDefaultAsync(u => u.Id == user.Id && u.UserType == UserType.Company);

            if (company is null)
            {
                _logger.LogWarning("Nenhuma empresa encontrada para o usuário {UserId}.", user.Id);
                return BadRequest("Empresa não encontrada para o usuário atual.");
            }

            var companyId = company.Id;

            var parameters = await _context.CompanyParameters
                .SingleOrDefaultAsync(p => p.CompanyId == companyId);

            if (parameters is null)
            {
                parameters = new CompanyParameters
                {
                    CompanyId = companyId
                };

                _context.CompanyParameters.Add(parameters);
            }

            parameters.PricePerKwp = ClampNonNegative(input.SystemPricePerKwp);
            parameters.MaintenancePercent = ClampNonNegative(input.MaintenancePercent);
            parameters.InstallDiscountPercent = ClampNonNegative(input.InstallDiscountPercent);
            parameters.RentalFactorPercent = ClampNonNegative(input.RentalFactorPercent);
            parameters.RentalMinMonthly = ClampNonNegative(input.RentalMinMonthly);
            parameters.RentalSetupPerKwp = ClampNonNegative(input.RentalSetupPerKwp);
            parameters.RentalAnnualIncreasePercent = ClampNonNegative(input.RentalAnnualIncreasePercent);
            parameters.RentalDiscountPercent = ClampNonNegative(input.RentalDiscountPercent);
            parameters.ConsumptionPerKwp = ClampNonNegative(input.ConsumptionPerKwp);
            parameters.MinSystemSizeKwp = ClampNonNegative(input.MinSystemSizeKwp);
            parameters.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
                var response = MapToDto(parameters);
                return Ok(response);
            }
            catch (DbUpdateException dbEx)
            {
                var detailedMessage = dbEx.InnerException?.Message ?? dbEx.Message;

                _logger.LogError(dbEx,
                    "Erro de banco ao salvar parâmetros da empresa {CompanyId}. Detalhe: {Message}",
                    companyId, detailedMessage);

                // TEMPORÁRIO: só pra debug, pra gente ver a mensagem real do SQL
                return StatusCode(500, detailedMessage);
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao salvar parâmetros da empresa {CompanyId}.", companyId);
                return StatusCode(500, "Erro inesperado ao salvar parâmetros da empresa.");
            }
        }

        private static CompanyParametersInputModel MapToDto(CompanyParameters parameters)
        {
            return new CompanyParametersInputModel
            {
                SystemPricePerKwp = parameters.PricePerKwp,
                MaintenancePercent = parameters.MaintenancePercent,
                InstallDiscountPercent = parameters.InstallDiscountPercent,
                RentalFactorPercent = parameters.RentalFactorPercent,
                RentalMinMonthly = parameters.RentalMinMonthly,
                RentalSetupPerKwp = parameters.RentalSetupPerKwp,
                RentalAnnualIncreasePercent = parameters.RentalAnnualIncreasePercent,
                RentalDiscountPercent = parameters.RentalDiscountPercent,
                ConsumptionPerKwp = parameters.ConsumptionPerKwp,
                MinSystemSizeKwp = parameters.MinSystemSizeKwp
            };
        }

        private static decimal ClampNonNegative(decimal value) => value < 0 ? 0 : value;
    }
}
