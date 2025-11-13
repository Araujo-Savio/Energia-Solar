using System;
using System.Collections.Generic;
using System.Globalization;

namespace SolarEnergy.ViewModels
{
    public class MonthlyReportViewModel
    {
        public string CompanyName { get; set; } = string.Empty;
        public int CurrentMonthLeads { get; set; }
        public double LeadsGrowthPercentage { get; set; }
        public double ConversionRate { get; set; }
        public double ConversionRateDelta { get; set; }
        public decimal CurrentMonthRevenue { get; set; }
        public double RevenueGrowthPercentage { get; set; }
        public double? AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public IReadOnlyCollection<MonthlyLeadData> LeadsByMonth { get; set; } = Array.Empty<MonthlyLeadData>();
        public string CurrentMonthName { get; set; } = string.Empty;

        public string FormatGrowthText(double value)
        {
            var formatted = value.ToString("+#0.0;-#0.0;0.0", CultureInfo.GetCultureInfo("pt-BR"));
            return $"{formatted}% vs mês anterior";
        }
    }

    public class MonthlyLeadData
    {
        public string MonthName { get; set; } = string.Empty;
        public int LeadCount { get; set; }
    }
}
