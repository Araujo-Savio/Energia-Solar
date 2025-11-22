using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SolarEnergy.Data;
using SolarEnergy.Models;
using SolarEnergy.ViewModels;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace SolarEnergy.Controllers
{
    [Authorize(Roles = "Company")]
    public class CompanyParametersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CompanyParametersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ===============================
        // GET: /CompanyParameters
        // ===============================
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Unauthorized();

            var parameters = await _context.CompanyParameters
                .FirstOrDefaultAsync(x => x.CompanyId == user.Id);

            if (parameters == null)
            {
                parameters = new CompanyParameters
                {
                    CompanyId = user.Id,
                    SystemPricePerKwp = 0,
                    MaintenancePercent = 0,
                    InstallDiscountPercent = 0,
                    RentalFactorPercent = 0,
                    RentalMinMonthly = 0,
                    RentalSetupPerKwp = 0,
                    RentalAnnualIncreasePercent = 0,
                    RentalDiscountPercent = 0,
                    ConsumptionPerKwp = 0,
                    MinSystemSizeKwp = 0
                };

                _context.CompanyParameters.Add(parameters);
                await _context.SaveChangesAsync();
            }

            var viewModel = MapToViewModel(parameters);

            return View(viewModel);
        }


        // ===============================
        // POST: /CompanyParameters/Save
        // ===============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(CompanyParametersViewModel updated)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Unauthorized();

            if (!ModelState.IsValid)
            {
                if (IsAjaxRequest())
                {
                    return BadRequest(new { success = false, errors = ModelState });
                }

                return View("Index", updated);
            }

            var existing = await _context.CompanyParameters
                .FirstOrDefaultAsync(x => x.CompanyId == user.Id);

            if (existing == null)
                return NotFound();

            // Atualização dos campos
            existing.SystemPricePerKwp = updated.SystemPricePerKwp;
            existing.MaintenancePercent = updated.MaintenancePercent;
            existing.InstallDiscountPercent = updated.InstallDiscountPercent;
            existing.RentalFactorPercent = updated.RentalFactorPercent;
            existing.RentalMinMonthly = updated.RentalMinMonthly;
            existing.RentalSetupPerKwp = updated.RentalSetupPerKwp;
            existing.RentalAnnualIncreasePercent = updated.RentalAnnualIncreasePercent;
            existing.RentalDiscountPercent = updated.RentalDiscountPercent;
            existing.ConsumptionPerKwp = updated.ConsumptionPerKwp;
            existing.MinSystemSizeKwp = updated.MinSystemSizeKwp;

            existing.UpdatedAt = DateTime.UtcNow;

            _context.CompanyParameters.Update(existing);
            await _context.SaveChangesAsync();

            if (IsAjaxRequest())
            {
                return Json(new { success = true });
            }

            return RedirectToAction("Index");
        }

        // ===============================
        // POST: /CompanyParameters/SaveForCurrentCompany
        // ===============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveForCurrentCompany([FromBody] CompanyParametersInputModel model)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Unauthorized();

            if (model == null)
                return BadRequest("Parâmetros inválidos.");

            var existing = await _context.CompanyParameters
                .FirstOrDefaultAsync(x => x.CompanyId == user.Id);

            if (existing == null)
            {
                existing = new CompanyParameters
                {
                    CompanyId = user.Id
                };

                UpdateEntityFromInputModel(existing, model);
                _context.CompanyParameters.Add(existing);
            }
            else
            {
                UpdateEntityFromInputModel(existing, model);
                _context.CompanyParameters.Update(existing);
            }

            await _context.SaveChangesAsync();

            var responseModel = MapToInputModel(existing);

            return Json(model);
        }


        private static CompanyParametersViewModel MapToViewModel(CompanyParameters parameters)
        {
            return new CompanyParametersViewModel
            {
                Id = parameters.Id,
                CompanyId = parameters.CompanyId,
                SystemPricePerKwp = parameters.SystemPricePerKwp,
                MaintenancePercent = parameters.MaintenancePercent,
                InstallDiscountPercent = parameters.InstallDiscountPercent,
                RentalFactorPercent = parameters.RentalFactorPercent,
                RentalSetupPerKwp = parameters.RentalSetupPerKwp,
                RentalMinMonthly = parameters.RentalMinMonthly,
                RentalAnnualIncreasePercent = parameters.RentalAnnualIncreasePercent,
                RentalDiscountPercent = parameters.RentalDiscountPercent,
                ConsumptionPerKwp = parameters.ConsumptionPerKwp,
                MinSystemSizeKwp = parameters.MinSystemSizeKwp
            };
        }


        private static void UpdateEntityFromInputModel(CompanyParameters entity, CompanyParametersInputModel model)
        {
            entity.SystemPricePerKwp = model.SystemPricePerKwp;
            entity.MaintenancePercent = model.MaintenancePercent;
            entity.InstallDiscountPercent = model.InstallDiscountPercent;
            entity.RentalFactorPercent = model.RentalFactorPercent;
            entity.RentalMinMonthly = model.RentalMinMonthly;
            entity.RentalSetupPerKwp = model.RentalSetupPerKwp;
            entity.RentalAnnualIncreasePercent = model.RentalAnnualIncreasePercent;
            entity.RentalDiscountPercent = model.RentalDiscountPercent;
            entity.ConsumptionPerKwp = model.ConsumptionPerKwp;
            entity.MinSystemSizeKwp = model.MinSystemSizeKwp;
            entity.UpdatedAt = DateTime.UtcNow;
        }

        private static CompanyParametersInputModel MapToInputModel(CompanyParameters parameters)
        {
            return new CompanyParametersInputModel
            {
                SystemPricePerKwp = parameters.SystemPricePerKwp,
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

        private bool IsAjaxRequest()
        {
            var requestedWith = Request?.Headers?["X-Requested-With"].ToString();

            return string.Equals(requestedWith, "XMLHttpRequest", StringComparison.OrdinalIgnoreCase)
                || (Request?.Headers?["Accept"].ToString().Contains("application/json") ?? false);
        }
    }
}
