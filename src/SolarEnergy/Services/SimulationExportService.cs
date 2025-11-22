using System.IO;
using iText.Html2pdf;
using iText.Kernel.Pdf;
using SolarEnergy.ViewModels;

namespace SolarEnergy.Services
{
    public class SimulationExportService : ISimulationExportService
    {
        private readonly IRazorViewRenderer _viewRenderer;

        public SimulationExportService(IRazorViewRenderer viewRenderer)
        {
            _viewRenderer = viewRenderer;
        }

        public byte[] GenerateSimulationPdf(SimulationPdfViewModel model)
        {
            var html = _viewRenderer.RenderViewToStringAsync("/Views/Simulation/SimulationPdf.cshtml", model)
                .GetAwaiter()
                .GetResult();

            using var memoryStream = new MemoryStream();
            using var pdfWriter = new PdfWriter(memoryStream);
            using var pdfDocument = new PdfDocument(pdfWriter);
            var converterProperties = new ConverterProperties();

            HtmlConverter.ConvertToPdf(html, pdfDocument, converterProperties);
            return memoryStream.ToArray();
        }
    }
}
