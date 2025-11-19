using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SolarEnergy.Data;
using SolarEnergy.ViewModels;
using System.Threading.Tasks;

namespace SolarEnergy.Controllers
{
    [Authorize]
    public class SimulationController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SimulationController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetCompanyParameters(string companyId)
        {
            if (string.IsNullOrWhiteSpace(companyId))
            {
                return NotFound();
            }

            var parameters = await _context.CompanyParameters
                .AsNoTracking()
                .SingleOrDefaultAsync(p => p.CompanyId == companyId);

            if (parameters == null)
            {
                return NotFound();
            }

            var viewModel = new CompanyParametersViewModel
            {
                PricePerKwp = parameters.SystemPricePerKwp,
                AnnualMaintenancePercent = parameters.MaintenancePercent,
                InstallationDiscountPercent = parameters.InstallDiscountPercent,
                MonthlyFeeFactorPercent = parameters.RentalFactorPercent,
                MinimumMonthlyFee = parameters.RentalMinMonthly,
                SetupFeePerKwp = parameters.RentalSetupPerKwp,
                AnnualRentAdjustmentPercent = parameters.RentalAnnualIncreasePercent,
                BillDiscountPercent = parameters.RentalDiscountPercent,
                MonthlyConsumptionPerKwp = parameters.ConsumptionPerKwp,
                MinimumSystemPowerKwp = parameters.MinSystemSizeKwp
            };

            return Json(viewModel);
        }
    }
}
