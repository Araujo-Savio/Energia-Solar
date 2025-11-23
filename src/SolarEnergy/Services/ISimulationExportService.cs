using SolarEnergy.ViewModels;

namespace SolarEnergy.Services
{
    public interface ISimulationExportService
    {
        byte[] ExportarSimulacaoParaPdf(SimulationResultsDto resultados);
    }
}
