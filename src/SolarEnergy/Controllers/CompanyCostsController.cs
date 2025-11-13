using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SolarEnergy.Models;
using SolarEnergy.Services;
using SolarEnergy.ViewModels;

namespace SolarEnergy.Controllers
{
    [Authorize]
    public class CompanyCostsController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICompanyCostService _companyCostService;
        private readonly ILogger<CompanyCostsController> _logger;

        public CompanyCostsController(
            UserManager<ApplicationUser> userManager,
            ICompanyCostService companyCostService,
            ILogger<CompanyCostsController> logger)
        {
            _userManager = userManager;
            _companyCostService = companyCostService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null || user.UserType != UserType.Company)
            {
                return Forbid();
            }

            var profile = await _companyCostService.GetProfileAsync(user.Id);
            var viewModel = profile is null
                ? CreateDefaultViewModel(user)
                : MapToViewModel(profile);

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CompanyCostProfileEditViewModel viewModel)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null || user.UserType != UserType.Company)
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            try
            {
                var profile = MapToEntity(viewModel, user.Id);
                await _companyCostService.UpsertProfileAsync(profile);
                TempData["SuccessMessage"] = "Custos atualizados com sucesso.";
                return RedirectToAction(nameof(Edit));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar custos da empresa {CompanyId}", user.Id);
                TempData["ErrorMessage"] = "Não foi possível salvar os custos da empresa.";
                return View(viewModel);
            }
        }

        private static CompanyCostProfileEditViewModel CreateDefaultViewModel(ApplicationUser user)
        {
            return new CompanyCostProfileEditViewModel
            {
                CompanyId = user.Id,
                EquipmentCosts =
                {
                    new CompanyCostItemViewModel { Name = "Módulos fotovoltaicos", ItemType = CompanyCostItemType.Equipment },
                    new CompanyCostItemViewModel { Name = "Inversores", ItemType = CompanyCostItemType.Equipment }
                },
                ServiceCosts =
                {
                    new CompanyCostItemViewModel { Name = "Projeto executivo", ItemType = CompanyCostItemType.Service },
                    new CompanyCostItemViewModel { Name = "Instalação", ItemType = CompanyCostItemType.Service }
                },
                SystemSizeCosts =
                {
                    new CompanySystemSizeCostViewModel { Label = "3 kWp", SystemSizeKwp = 3 },
                    new CompanySystemSizeCostViewModel { Label = "5 kWp", SystemSizeKwp = 5 },
                    new CompanySystemSizeCostViewModel { Label = "8 kWp", SystemSizeKwp = 8 },
                    new CompanySystemSizeCostViewModel { Label = "10 kWp", SystemSizeKwp = 10 }
                }
            };
        }

        private static CompanyCostProfileEditViewModel MapToViewModel(CompanyCostProfile profile)
        {
            return new CompanyCostProfileEditViewModel
            {
                CompanyId = profile.CompanyId,
                ProductionPerKilowattPeak = profile.ProductionPerKilowattPeak,
                MaintenanceRatePercent = Math.Round(profile.MaintenanceRate * 100, 2),
                RentalRatePerKwh = profile.RentalRatePerKwh,
                RentalAnnualIncreasePercent = Math.Round(profile.RentalAnnualIncrease * 100, 2),
                EquipmentCosts = profile.CostItems
                    .Where(i => i.ItemType == CompanyCostItemType.Equipment)
                    .Select(i => new CompanyCostItemViewModel
                    {
                        Id = i.Id,
                        Name = i.Name,
                        ItemType = i.ItemType,
                        Cost = i.Cost,
                        Unit = i.Unit,
                        Notes = i.Notes,
                        IsActive = i.IsActive
                    })
                    .ToList(),
                ServiceCosts = profile.CostItems
                    .Where(i => i.ItemType != CompanyCostItemType.Equipment)
                    .Select(i => new CompanyCostItemViewModel
                    {
                        Id = i.Id,
                        Name = i.Name,
                        ItemType = i.ItemType,
                        Cost = i.Cost,
                        Unit = i.Unit,
                        Notes = i.Notes,
                        IsActive = i.IsActive
                    })
                    .ToList(),
                SystemSizeCosts = profile.SystemSizeCosts
                    .OrderBy(c => c.SystemSizeKwp)
                    .Select(c => new CompanySystemSizeCostViewModel
                    {
                        Id = c.Id,
                        Label = c.Label,
                        SystemSizeKwp = c.SystemSizeKwp,
                        AverageCost = c.AverageCost,
                        Notes = c.Notes
                    })
                    .ToList()
            };
        }

        private static CompanyCostProfile MapToEntity(CompanyCostProfileEditViewModel viewModel, string companyId)
        {
            var profile = new CompanyCostProfile
            {
                CompanyId = companyId,
                ProductionPerKilowattPeak = viewModel.ProductionPerKilowattPeak,
                MaintenanceRate = viewModel.MaintenanceRatePercent / 100,
                RentalRatePerKwh = viewModel.RentalRatePerKwh,
                RentalAnnualIncrease = viewModel.RentalAnnualIncreasePercent / 100
            };

            foreach (var item in viewModel.EquipmentCosts.Concat(viewModel.ServiceCosts))
            {
                profile.CostItems.Add(new CompanyCostItem
                {
                    Id = item.Id,
                    ItemType = item.ItemType,
                    Name = item.Name,
                    Cost = item.Cost,
                    Unit = item.Unit,
                    Notes = item.Notes,
                    IsActive = item.IsActive
                });
            }

            foreach (var systemCost in viewModel.SystemSizeCosts)
            {
                profile.SystemSizeCosts.Add(new CompanySystemSizeCost
                {
                    Id = systemCost.Id,
                    Label = systemCost.Label,
                    SystemSizeKwp = systemCost.SystemSizeKwp,
                    AverageCost = systemCost.AverageCost,
                    Notes = systemCost.Notes
                });
            }

            return profile;
        }
    }
}
