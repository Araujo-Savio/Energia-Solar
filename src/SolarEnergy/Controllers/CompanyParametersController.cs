using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SolarEnergy.Models;
using SolarEnergy.Services;
using SolarEnergy.ViewModels;

namespace SolarEnergy.Controllers
{
    [Authorize(Roles = "Company")]
    public class CompanyParametersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICompanyParametersService _companyParametersService;

        public CompanyParametersController(
            UserManager<ApplicationUser> userManager,
            ICompanyParametersService companyParametersService)
        {
            _userManager = userManager;
            _companyParametersService = companyParametersService;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save([FromBody] CompanyParametersInputModel model)
        {
            if (model == null)
            {
                return BadRequest();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var parameters = new CompanyParameters
            {
                CompanyId = user.Id,
                PricePerKwP = model.PricePerKwP,
                AnnualMaintenance = model.AnnualMaintenance,
                InstallationDiscount = model.InstallationDiscount,
                RentalPercent = model.RentalPercent,
                MinRentalValue = model.MinRentalValue,
                RentalSetupFee = model.RentalSetupFee,
                AnnualRentIncrease = model.AnnualRentIncrease,
                RentDiscount = model.RentDiscount,
                KwhPerKwp = model.KwhPerKwp,
                MinSystemPower = model.MinSystemPower
            };

            await _companyParametersService.SaveOrUpdate(parameters);

            return Json(new { success = true });
        }
    }
}
