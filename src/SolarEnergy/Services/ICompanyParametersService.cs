using SolarEnergy.Models;

namespace SolarEnergy.Services
{
    public interface ICompanyParametersService
    {
        Task<CompanyParameters?> GetByCompanyId(string companyId);
        Task SaveOrUpdate(CompanyParameters model);
    }
}
