using SolarEnergy.ViewModels;

namespace SolarEnergy.Services
{
    public interface ISimulationExportService
    {
        byte[] GenerateSimulationPdf(SimulationPdfViewModel model);
    }
}
