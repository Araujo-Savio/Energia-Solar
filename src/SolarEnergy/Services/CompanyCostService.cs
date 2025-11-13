using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SolarEnergy.Data;
using SolarEnergy.Models;
using SolarEnergy.ViewModels;

namespace SolarEnergy.Services
{
    public class CompanyCostService : ICompanyCostService
    {
        private readonly ApplicationDbContext _context;

        public CompanyCostService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<CompanyCostProfile?> GetProfileAsync(string companyId, CancellationToken cancellationToken = default)
        {
            return await _context.CompanyCostProfiles
                .Include(p => p.CostItems)
                .Include(p => p.SystemSizeCosts)
                .FirstOrDefaultAsync(p => p.CompanyId == companyId, cancellationToken);
        }

        public async Task<IReadOnlyList<CompanyCostProfile>> ListProfilesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.CompanyCostProfiles
                .AsNoTracking()
                .Include(p => p.CostItems)
                .Include(p => p.SystemSizeCosts)
                .OrderBy(p => p.CompanyId)
                .ToListAsync(cancellationToken);
        }

        public async Task<CompanyCostProfile> UpsertProfileAsync(CompanyCostProfile profile, CancellationToken cancellationToken = default)
        {
            var existing = await _context.CompanyCostProfiles
                .Include(p => p.CostItems)
                .Include(p => p.SystemSizeCosts)
                .FirstOrDefaultAsync(p => p.Id == profile.Id || p.CompanyId == profile.CompanyId, cancellationToken);

            if (existing == null)
            {
                profile.CreatedAt = DateTime.UtcNow;
                profile.UpdatedAt = DateTime.UtcNow;
                foreach (var item in profile.CostItems)
                {
                    item.CompanyCostProfileId = profile.Id;
                    item.Profile = profile;
                }

                foreach (var systemCost in profile.SystemSizeCosts)
                {
                    systemCost.CompanyCostProfileId = profile.Id;
                    systemCost.Profile = profile;
                }

                _context.CompanyCostProfiles.Add(profile);
            }
            else
            {
                existing.ProductionPerKilowattPeak = profile.ProductionPerKilowattPeak;
                existing.MaintenanceRate = profile.MaintenanceRate;
                existing.RentalRatePerKwh = profile.RentalRatePerKwh;
                existing.RentalAnnualIncrease = profile.RentalAnnualIncrease;
                existing.UpdatedAt = DateTime.UtcNow;

                _context.CompanyCostItems.RemoveRange(existing.CostItems);
                _context.CompanySystemSizeCosts.RemoveRange(existing.SystemSizeCosts);

                foreach (var item in profile.CostItems)
                {
                    item.CompanyCostProfileId = existing.Id;
                    item.Profile = existing;
                }

                foreach (var systemCost in profile.SystemSizeCosts)
                {
                    systemCost.CompanyCostProfileId = existing.Id;
                    systemCost.Profile = existing;
                }

                existing.CostItems = profile.CostItems;
                existing.SystemSizeCosts = profile.SystemSizeCosts;
            }

            await _context.SaveChangesAsync(cancellationToken);

            return existing ?? profile;
        }

        public async Task<CompanyCostSummaryViewModel?> GetSummaryAsync(string companyId, CancellationToken cancellationToken = default)
        {
            var profile = await GetProfileAsync(companyId, cancellationToken);
            if (profile == null)
            {
                return null;
            }

            var equipmentCosts = profile.CostItems
                .Where(i => i.ItemType == CompanyCostItemType.Equipment && i.IsActive)
                .OrderBy(i => i.Name)
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
                .ToList();

            var serviceCosts = profile.CostItems
                .Where(i => i.ItemType != CompanyCostItemType.Equipment && i.IsActive)
                .OrderBy(i => i.Name)
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
                .ToList();

            var systemCosts = profile.SystemSizeCosts
                .OrderBy(c => c.SystemSizeKwp)
                .Select(c => new CompanySystemSizeCostViewModel
                {
                    Id = c.Id,
                    Label = c.Label,
                    SystemSizeKwp = c.SystemSizeKwp,
                    AverageCost = c.AverageCost,
                    Notes = c.Notes
                })
                .ToList();

            return new CompanyCostSummaryViewModel
            {
                CompanyId = profile.CompanyId,
                ProductionPerKilowattPeak = profile.ProductionPerKilowattPeak,
                MaintenanceRate = profile.MaintenanceRate,
                RentalRatePerKwh = profile.RentalRatePerKwh,
                RentalAnnualIncrease = profile.RentalAnnualIncrease,
                EquipmentCosts = equipmentCosts,
                ServiceCosts = serviceCosts,
                SystemSizeCosts = systemCosts
            };
        }
    }
}
