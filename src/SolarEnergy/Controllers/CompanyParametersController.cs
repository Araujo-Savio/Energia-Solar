using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SolarEnergy.Data;
using SolarEnergy.Models;
using System;
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

            return View(parameters);
        }


        // ===============================
        // POST: /CompanyParameters/Save
        // ===============================
        [HttpPost]
        public async Task<IActionResult> Save(CompanyParameters updated)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Unauthorized();

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

            return RedirectToAction("Index");
        }
    }
}
