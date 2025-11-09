using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using SolarEnergy.Models;

namespace SolarEnergy.ViewModels
{
    public class MonthlyReportViewModel
    {
        public DateTime ReportMonth { get; set; } = DateTime.Now;
        public string CompanyName { get; set; } = string.Empty;
        public string CompanyId { get; set; } = string.Empty;

        // Legacy properties for backward compatibility
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

        // New detailed metrics
        public LeadMetrics LeadMetrics { get; set; } = new();
        public FinancialMetrics FinancialMetrics { get; set; } = new();
        public SatisfactionMetrics SatisfactionMetrics { get; set; } = new();
        public PerformanceMetrics PerformanceMetrics { get; set; } = new();
        public ChartsData ChartsData { get; set; } = new();
        public List<MonthOption> AvailableMonths { get; set; } = new();

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

    // New detailed metric classes
    public class LeadMetrics
    {
        public int TotalLeadsReceived { get; set; }
        public int LeadsResponded { get; set; }
        public int LeadsConverted { get; set; }
        public decimal ConversionRate { get; set; }
        
        public int PreviousMonthLeads { get; set; }
        public decimal LeadsGrowthPercentage { get; set; }
        
        public int OrganicLeads { get; set; }
        public int PaidLeads { get; set; }
        public decimal LeadPurchaseInvestment { get; set; }
        public int LeadPurchaseQuantity { get; set; }
    }

    public class FinancialMetrics
    {
        public decimal TotalRevenue { get; set; }
        public decimal AverageContractValue { get; set; }
        public decimal LeadAcquisitionCost { get; set; }
        public decimal CustomerAcquisitionCost { get; set; }
        public decimal ReturnOnInvestment { get; set; }
        
        public decimal PreviousMonthRevenue { get; set; }
        public decimal RevenueGrowthPercentage { get; set; }
    }

    public class SatisfactionMetrics
    {
        public decimal AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public int FiveStarReviews { get; set; }
        public int FourStarReviews { get; set; }
        public int ThreeStarReviews { get; set; }
        public int TwoStarReviews { get; set; }
        public int OneStarReviews { get; set; }
        
        public decimal SatisfactionScore { get; set; }
        public decimal PreviousMonthRating { get; set; }
        public decimal RatingImprovement { get; set; }
        
        public List<ReviewSummary> RecentReviews { get; set; } = new();
    }

    public class PerformanceMetrics
    {
        public TimeSpan AverageResponseTime { get; set; }
        public decimal ResponseRate { get; set; }
        public int TotalProposalsSent { get; set; }
        public int AcceptedProposals { get; set; }
        public decimal ProposalAcceptanceRate { get; set; }
        
        public int ActiveQuotes { get; set; }
        public int CompletedProjects { get; set; }
        public decimal CustomerRetentionRate { get; set; }
    }

    public class ChartsData
    {
        public List<MonthlyDataPoint> LeadsChart { get; set; } = new();
        public List<MonthlyDataPoint> RevenueChart { get; set; } = new();
        public List<RatingDistribution> RatingDistribution { get; set; } = new();
    }

    public class MonthlyDataPoint
    {
        public string Month { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public string Label { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
    }

    public class RatingDistribution
    {
        public int Stars { get; set; }
        public int Count { get; set; }
        public decimal Percentage { get; set; }
        public string Color { get; set; } = string.Empty;
    }

    public class ReviewSummary
    {
        public string ClientName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string ServiceType { get; set; } = string.Empty;
    }

    public class MonthOption
    {
        public DateTime Date { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public bool HasData { get; set; }
    }

    public class ReportExportRequest
    {
        public DateTime Month { get; set; }
        public string Format { get; set; } = "pdf";
        public bool IncludeCharts { get; set; } = true;
        public bool IncludeReviews { get; set; } = true;
        public List<string> Sections { get; set; } = new();
    }
}
