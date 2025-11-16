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
                return BadRequest(new { message = "Parâmetros inválidos." });
            }

            var companyUser = await _userManager.GetUserAsync(User);
            if (companyUser is null)
            {
                _logger.LogWarning("Company user not found while saving parameters.");
                return Unauthorized();
            }

            var parameters = await _context.CompanyParameters
                .SingleOrDefaultAsync(p => p.CompanyId == companyUser.Id);

            if (parameters is null)
            {
                parameters = new CompanyParameters
                {
                    CompanyId = companyUser.Id
                };

                _context.CompanyParameters.Add(parameters);
            }

            parameters.PricePerKwp = ClampNonNegative(input.PricePerKwp);
            parameters.MaintenancePercent = ClampNonNegative(input.MaintenancePercent);
            parameters.InstallDiscountPercent = ClampNonNegative(input.InstallDiscountPercent);
            parameters.RentalFactorPercent = ClampNonNegative(input.RentalFactorPercent);
            parameters.RentalMinMonthly = ClampNonNegative(input.RentalMinMonthly);
            parameters.RentalSetupPerKwp = ClampNonNegative(input.RentalSetupPerKwp);
            parameters.RentalAnnualIncreasePercent = ClampNonNegative(input.RentalAnnualIncreasePercent);
            parameters.RentalDiscountPercent = ClampNonNegative(input.RentalDiscountPercent);
            parameters.ConsumptionPerKwp = Math.Max(1m, input.ConsumptionPerKwp);
            parameters.MinSystemSizeKwp = ClampNonNegative(input.MinSystemSizeKwp);
            parameters.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao salvar parâmetros da empresa {CompanyId}", companyUser.Id);
                return StatusCode(500, "Erro ao salvar parâmetros da empresa.");
            }

            var response = MapToDto(parameters);
            return Ok(response);
        }

        private static CompanyParametersInputModel MapToDto(CompanyParameters parameters)
        {
            return new CompanyParametersInputModel
            {
                PricePerKwp = parameters.PricePerKwp,
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
