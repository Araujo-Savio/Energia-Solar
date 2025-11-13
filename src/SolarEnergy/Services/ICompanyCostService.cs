using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SolarEnergy.Models;
using SolarEnergy.ViewModels;

namespace SolarEnergy.Services
{
    public interface ICompanyCostService
    {
        Task<CompanyCostProfile?> GetProfileAsync(string companyId, CancellationToken cancellationToken = default);
        Task<CompanyCostProfile> UpsertProfileAsync(CompanyCostProfile profile, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<CompanyCostProfile>> ListProfilesAsync(CancellationToken cancellationToken = default);
        Task<CompanyCostSummaryViewModel?> GetSummaryAsync(string companyId, CancellationToken cancellationToken = default);
    }
}
