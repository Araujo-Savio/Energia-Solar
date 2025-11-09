using System.Collections.Generic;
using SolarEnergy.Models;

namespace SolarEnergy.ViewModels
{
    public class CompanySearchViewModel
    {
        public string? SearchTerm { get; set; }
        public List<CompanySummaryViewModel> Companies { get; set; } = new();
    }

    public class CompanySummaryViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? LegalName { get; set; }
        public string? TradeName { get; set; }
        public string? Location { get; set; }
        public string? Phone { get; set; }
        public string? Website { get; set; }
        public string? Description { get; set; }
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public SolarServiceType? ServiceType { get; set; }
    }
}
