using SolarEnergy.ViewModels;

namespace SolarEnergy.Services
{
    public interface ISimulationExportService
    {
        string GenerateUserCsv(UserSimulationInput input, UserSimulationResult result);
        byte[] GenerateUserPdf(UserSimulationInput input, UserSimulationResult result);
    }
}
