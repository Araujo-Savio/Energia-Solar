using System;
using SolarEnergy.Models;

namespace SolarEnergy.ViewModels
{
    public class TechnicalVisitListItemViewModel
    {
        public long Id { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public string ServiceType { get; set; } = string.Empty;
        public DateTime VisitDate { get; set; }
        public TimeSpan VisitTime { get; set; }
        public TechnicalVisitStatus Status { get; set; }
    }
}
