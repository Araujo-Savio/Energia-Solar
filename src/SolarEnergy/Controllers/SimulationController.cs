using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SolarEnergy.Models;
using SolarEnergy.Services;
using SolarEnergy.ViewModels;

namespace SolarEnergy.Controllers
{
    [Authorize]
    public class SimulationController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICompanyCostService _companyCostService;
        private readonly ISimulationService _simulationService;

        public SimulationController(
            UserManager<ApplicationUser> userManager,
            ICompanyCostService companyCostService,
            ISimulationService simulationService)
        {
            _userManager = userManager;
            _companyCostService = companyCostService;
            _simulationService = simulationService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? companyId = null)
        {
            await SetUserTypeInViewData();

            var model = new CompanySimulationInputViewModel
            {
                AvailableCompanies = await LoadCompanyOptionsAsync()
            };

            model.SelectedCompanyId = companyId ?? model.AvailableCompanies.FirstOrDefault()?.CompanyId ?? string.Empty;

            if (!string.IsNullOrEmpty(model.SelectedCompanyId))
            {
                model.CompanyCosts = await _companyCostService.GetSummaryAsync(model.SelectedCompanyId);
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Calculate(CompanySimulationInputViewModel model)
        {
            await SetUserTypeInViewData();

            model.AvailableCompanies = await LoadCompanyOptionsAsync();

            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            model.CompanyCosts = await _companyCostService.GetSummaryAsync(model.SelectedCompanyId);

            var installationResult = _simulationService.CalculateInstallation(model.Simulation.InstallationInput, model.CompanyCosts);
            var rentalResult = _simulationService.CalculateRental(model.Simulation.RentalInput, model.CompanyCosts);
            model.Simulation.InstallationResult = installationResult;
            model.Simulation.RentalResult = rentalResult;
            model.Simulation.Comparison = _simulationService.BuildComparison(installationResult, rentalResult);
            model.Simulation.Projection = _simulationService.BuildProjection(installationResult, rentalResult);

            return View("Index", model);
        }

        private async Task<IList<CompanySimulationOptionViewModel>> LoadCompanyOptionsAsync()
        {
            return await _userManager.Users
                .Where(u => u.UserType == UserType.Company)
                .OrderBy(u => u.CompanyTradeName)
                .Select(u => new CompanySimulationOptionViewModel
                {
                    CompanyId = u.Id,
                    CompanyName = !string.IsNullOrEmpty(u.CompanyTradeName) ? u.CompanyTradeName : u.CompanyLegalName ?? u.FullName,
                    Location = u.Location,
                    ServiceType = u.ServiceType?.ToString()
                })
                .ToListAsync();
        }

        private async Task SetUserTypeInViewData()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    ViewData["UserType"] = user.UserType;
                }
            }
        }
    }
}
