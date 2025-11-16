using Microsoft.EntityFrameworkCore;
using SolarEnergy.Data;
using SolarEnergy.Models;

namespace SolarEnergy.Services
{
    public class CompanyParametersService : ICompanyParametersService
    {
        private readonly ApplicationDbContext _context;

        public CompanyParametersService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<CompanyParameters?> GetByCompanyId(string companyId)
        {
            return await _context.CompanyParameters
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.CompanyId == companyId);
        }

        public async Task SaveOrUpdate(CompanyParameters model)
        {
            var existing = await _context.CompanyParameters.FirstOrDefaultAsync(p => p.CompanyId == model.CompanyId);
            if (existing == null)
            {
                model.UpdatedAt = DateTime.UtcNow;
                _context.CompanyParameters.Add(model);
            }
            else
            {
                existing.PricePerKwP = model.PricePerKwP;
                existing.AnnualMaintenance = model.AnnualMaintenance;
                existing.InstallationDiscount = model.InstallationDiscount;
                existing.RentalPercent = model.RentalPercent;
                existing.MinRentalValue = model.MinRentalValue;
                existing.RentalSetupFee = model.RentalSetupFee;
                existing.AnnualRentIncrease = model.AnnualRentIncrease;
                existing.RentDiscount = model.RentDiscount;
                existing.KwhPerKwp = model.KwhPerKwp;
                existing.MinSystemPower = model.MinSystemPower;
                existing.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }
    }
}
