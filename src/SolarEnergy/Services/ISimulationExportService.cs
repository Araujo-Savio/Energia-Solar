using SolarEnergy.ViewModels;

namespace SolarEnergy.Services
{
    public interface ISimulationExportService
    {
        string GenerateCsv(UserSimulationResultViewModel result);
        byte[] GeneratePdf(UserSimulationResultViewModel result);
    }
}
