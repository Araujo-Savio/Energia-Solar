using Microsoft.EntityFrameworkCore;
using SolarEnergy.Data;
using SolarEnergy.Models;
using SolarEnergy.ViewModels;
using System.Globalization;

namespace SolarEnergy.Services
{
    public interface IReportService
    {
        Task<MonthlyReportViewModel> GenerateMonthlyReportAsync(string companyId, DateTime month);
        Task<List<MonthOption>> GetAvailableMonthsAsync(string companyId);
        Task<byte[]> ExportReportAsync(string companyId, ReportExportRequest request);
    }

    public class ReportService : IReportService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILeadService _leadService;
        private readonly IExportService _exportService;

        public ReportService(ApplicationDbContext context, ILeadService leadService, IExportService exportService)
        {
            _context = context;
            _leadService = leadService;
            _exportService = exportService;
        }

        public async Task<MonthlyReportViewModel> GenerateMonthlyReportAsync(string companyId, DateTime month)
        {
            var startDate = new DateTime(month.Year, month.Month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);
            
            var previousMonth = startDate.AddMonths(-1);
            var previousStartDate = new DateTime(previousMonth.Year, previousMonth.Month, 1);
            var previousEndDate = previousStartDate.AddMonths(1).AddDays(-1);

            var company = await _context.Users.FindAsync(companyId);
            if (company == null) throw new ArgumentException("Company not found");

            var report = new MonthlyReportViewModel
            {
                ReportMonth = month,
                CompanyName = company.CompanyTradeName ?? company.CompanyLegalName ?? company.FullName,
                CompanyId = companyId,
                LeadMetrics = await GenerateLeadMetricsAsync(companyId, startDate, endDate, previousStartDate, previousEndDate),
                FinancialMetrics = await GenerateFinancialMetricsAsync(companyId, startDate, endDate, previousStartDate, previousEndDate),
                SatisfactionMetrics = await GenerateSatisfactionMetricsAsync(companyId, startDate, endDate, previousStartDate, previousEndDate),
                PerformanceMetrics = await GeneratePerformanceMetricsAsync(companyId, startDate, endDate),
                ChartsData = await GenerateChartsDataAsync(companyId, month),
                AvailableMonths = await GetAvailableMonthsAsync(companyId)
            };

            return report;
        }

        private async Task<LeadMetrics> GenerateLeadMetricsAsync(string companyId, DateTime startDate, DateTime endDate, DateTime prevStartDate, DateTime prevEndDate)
        {
            // Current month quotes
            var currentMonthQuotes = await _context.Quotes
                .Where(q => q.CompanyId == companyId && q.RequestDate >= startDate && q.RequestDate <= endDate)
                .ToListAsync();

            // Previous month quotes
            var previousMonthQuotes = await _context.Quotes
                .Where(q => q.CompanyId == companyId && q.RequestDate >= prevStartDate && q.RequestDate <= prevEndDate)
                .ToListAsync();

            // Lead purchases in current month
            var leadPurchases = await _context.LeadPurchases
                .Where(p => p.CompanyId == companyId && p.PurchaseDate >= startDate && p.PurchaseDate <= endDate)
                .ToListAsync();

            var respondedLeads = currentMonthQuotes.Count(q => !string.IsNullOrEmpty(q.CompanyResponseMessage));
            var convertedLeads = await _context.Proposals
                .CountAsync(p => currentMonthQuotes.Select(q => q.QuoteId).Contains(p.QuoteId) && p.Status == "Accepted");

            var totalLeads = currentMonthQuotes.Count;
            var previousLeads = previousMonthQuotes.Count;
            var paidLeads = leadPurchases.Sum(p => p.LeadQuantity);
            var organicLeads = Math.Max(0, totalLeads - paidLeads);

            return new LeadMetrics
            {
                TotalLeadsReceived = totalLeads,
                LeadsResponded = respondedLeads,
                LeadsConverted = convertedLeads,
                ConversionRate = totalLeads > 0 ? Math.Round((decimal)convertedLeads / totalLeads * 100, 1) : 0,
                PreviousMonthLeads = previousLeads,
                LeadsGrowthPercentage = previousLeads > 0 ? Math.Round((decimal)(totalLeads - previousLeads) / previousLeads * 100, 1) : 0,
                OrganicLeads = organicLeads,
                PaidLeads = paidLeads,
                LeadPurchaseInvestment = leadPurchases.Sum(p => p.TotalAmount),
                LeadPurchaseQuantity = paidLeads
            };
        }

        private async Task<FinancialMetrics> GenerateFinancialMetricsAsync(string companyId, DateTime startDate, DateTime endDate, DateTime prevStartDate, DateTime prevEndDate)
        {
            // Current month proposals
            var currentMonthProposals = await _context.Proposals
                .Include(p => p.Quote)
                .Where(p => p.Quote.CompanyId == companyId && 
                           p.ProposalDate >= startDate && 
                           p.ProposalDate <= endDate &&
                           p.Status == "Accepted")
                .ToListAsync();

            // Previous month proposals
            var previousMonthProposals = await _context.Proposals
                .Include(p => p.Quote)
                .Where(p => p.Quote.CompanyId == companyId && 
                           p.ProposalDate >= prevStartDate && 
                           p.ProposalDate <= prevEndDate &&
                           p.Status == "Accepted")
                .ToListAsync();

            var currentRevenue = currentMonthProposals.Sum(p => p.Value);
            var previousRevenue = previousMonthProposals.Sum(p => p.Value);
            var averageValue = currentMonthProposals.Any() ? currentMonthProposals.Average(p => p.Value) : 0;

            // Lead acquisition cost
            var leadInvestment = await _context.LeadPurchases
                .Where(p => p.CompanyId == companyId && p.PurchaseDate >= startDate && p.PurchaseDate <= endDate)
                .SumAsync(p => p.TotalAmount);

            var totalLeads = await _context.Quotes
                .CountAsync(q => q.CompanyId == companyId && q.RequestDate >= startDate && q.RequestDate <= endDate);

            return new FinancialMetrics
            {
                TotalRevenue = currentRevenue,
                AverageContractValue = averageValue,
                LeadAcquisitionCost = totalLeads > 0 ? leadInvestment / totalLeads : 0,
                CustomerAcquisitionCost = currentMonthProposals.Count > 0 ? leadInvestment / currentMonthProposals.Count : 0,
                ReturnOnInvestment = leadInvestment > 0 ? Math.Round((currentRevenue - leadInvestment) / leadInvestment * 100, 1) : 
                                   (currentRevenue > 0 ? 100 : 0), // 100% ROI when no investment but has revenue
                PreviousMonthRevenue = previousRevenue,
                RevenueGrowthPercentage = previousRevenue > 0 ? Math.Round((currentRevenue - previousRevenue) / previousRevenue * 100, 1) : 0
            };
        }

        private async Task<SatisfactionMetrics> GenerateSatisfactionMetricsAsync(string companyId, DateTime startDate, DateTime endDate, DateTime prevStartDate, DateTime prevEndDate)
        {
            var currentReviews = await _context.CompanyReviews
                .Include(r => r.Reviewer)
                .Where(r => r.CompanyId == companyId && r.CreatedAt >= startDate && r.CreatedAt <= endDate)
                .ToListAsync();

            var previousReviews = await _context.CompanyReviews
                .Where(r => r.CompanyId == companyId && r.CreatedAt >= prevStartDate && r.CreatedAt <= prevEndDate)
                .ToListAsync();

            var avgRating = currentReviews.Where(r => r.Rating.HasValue && r.Rating > 0).Any() 
                ? (decimal)Math.Round(currentReviews.Where(r => r.Rating.HasValue && r.Rating > 0).Average(r => r.Rating.Value), 1) 
                : 0;
                
            var prevAvgRating = previousReviews.Where(r => r.Rating.HasValue && r.Rating > 0).Any() 
                ? (decimal)Math.Round(previousReviews.Where(r => r.Rating.HasValue && r.Rating > 0).Average(r => r.Rating.Value), 1) 
                : 0;

            var ratingCounts = currentReviews
                .Where(r => r.Rating.HasValue && r.Rating > 0 && r.Rating <= 5)
                .GroupBy(r => r.Rating.Value)
                .ToDictionary(g => g.Key, g => g.Count());

            return new SatisfactionMetrics
            {
                AverageRating = avgRating,
                TotalReviews = currentReviews.Count,
                FiveStarReviews = ratingCounts.GetValueOrDefault(5, 0),
                FourStarReviews = ratingCounts.GetValueOrDefault(4, 0),
                ThreeStarReviews = ratingCounts.GetValueOrDefault(3, 0),
                TwoStarReviews = ratingCounts.GetValueOrDefault(2, 0),
                OneStarReviews = ratingCounts.GetValueOrDefault(1, 0),
                SatisfactionScore = avgRating * 20,
                PreviousMonthRating = prevAvgRating,
                RatingImprovement = avgRating - prevAvgRating,
                RecentReviews = currentReviews
                    .Where(r => r.Rating.HasValue && r.Rating > 0)
                    .OrderByDescending(r => r.CreatedAt)
                    .Take(5)
                    .Select(r => new ReviewSummary
                    {
                        ClientName = r.Reviewer?.FullName ?? "Cliente Anônimo",
                        Rating = r.Rating ?? 0,
                        Comment = !string.IsNullOrWhiteSpace(r.Comment) ? r.Comment : "Sem comentário",
                        Date = r.CreatedAt,
                        ServiceType = "Energia Solar"
                    }).ToList()
            };
        }

        private async Task<PerformanceMetrics> GeneratePerformanceMetricsAsync(string companyId, DateTime startDate, DateTime endDate)
        {
            var quotes = await _context.Quotes
                .Where(q => q.CompanyId == companyId && q.RequestDate >= startDate && q.RequestDate <= endDate)
                .ToListAsync();

            var proposals = await _context.Proposals
                .Include(p => p.Quote)
                .Where(p => p.Quote.CompanyId == companyId && p.ProposalDate >= startDate && p.ProposalDate <= endDate)
                .ToListAsync();

            var respondedQuotes = quotes.Where(q => q.CompanyResponseDate.HasValue).ToList();
            var avgResponseTime = respondedQuotes.Any() 
                ? TimeSpan.FromMinutes(respondedQuotes.Average(q => (q.CompanyResponseDate!.Value - q.RequestDate).TotalMinutes))
                : TimeSpan.Zero;

            return new PerformanceMetrics
            {
                AverageResponseTime = avgResponseTime,
                ResponseRate = quotes.Count > 0 ? Math.Round((decimal)respondedQuotes.Count / quotes.Count * 100, 1) : 0,
                TotalProposalsSent = proposals.Count,
                AcceptedProposals = proposals.Count(p => p.Status == "Accepted"),
                ProposalAcceptanceRate = proposals.Count > 0 ? Math.Round((decimal)proposals.Count(p => p.Status == "Accepted") / proposals.Count * 100, 1) : 0,
                ActiveQuotes = quotes.Count(q => q.Status == "Em Análise"),
                CompletedProjects = proposals.Count(p => p.Status == "Completed"),
                CustomerRetentionRate = 85 // This would need more complex logic based on repeat customers
            };
        }

        private async Task<ChartsData> GenerateChartsDataAsync(string companyId, DateTime month)
        {
            var chartData = new ChartsData();
            
            // Generate last 6 months of data
            var months = new List<DateTime>();
            for (int i = 5; i >= 0; i--)
            {
                months.Add(month.AddMonths(-i));
            }

            foreach (var m in months)
            {
                var startDate = new DateTime(m.Year, m.Month, 1);
                var endDate = startDate.AddMonths(1).AddDays(-1);

                var leadsCount = await _context.Quotes
                    .CountAsync(q => q.CompanyId == companyId && q.RequestDate >= startDate && q.RequestDate <= endDate);

                var revenue = await _context.Proposals
                    .Include(p => p.Quote)
                    .Where(p => p.Quote.CompanyId == companyId && 
                               p.ProposalDate >= startDate && 
                               p.ProposalDate <= endDate &&
                               p.Status == "Accepted")
                    .SumAsync(p => p.Value);

                chartData.LeadsChart.Add(new MonthlyDataPoint
                {
                    Month = m.ToString("MMM/yyyy", new CultureInfo("pt-BR")),
                    Value = leadsCount,
                    Label = $"{leadsCount} leads",
                    Color = GetChartColor(months.IndexOf(m))
                });

                chartData.RevenueChart.Add(new MonthlyDataPoint
                {
                    Month = m.ToString("MMM/yyyy", new CultureInfo("pt-BR")),
                    Value = revenue,
                    Label = revenue.ToString("C", new CultureInfo("pt-BR")),
                    Color = GetChartColor(months.IndexOf(m))
                });
            }

            // Rating distribution
            var reviews = await _context.CompanyReviews
                .Where(r => r.CompanyId == companyId)
                .Where(r => r.Rating.HasValue)
                .ToListAsync();

            var totalReviews = reviews.Count;
            for (int rating = 1; rating <= 5; rating++)
            {
                var count = reviews.Count(r => r.Rating == rating);
                chartData.RatingDistribution.Add(new RatingDistribution
                {
                    Stars = rating,
                    Count = count,
                    Percentage = totalReviews > 0 ? Math.Round((decimal)count / totalReviews * 100, 1) : 0,
                    Color = GetRatingColor(rating)
                });
            }

            return chartData;
        }

        public async Task<List<MonthOption>> GetAvailableMonthsAsync(string companyId)
        {
            var firstQuote = await _context.Quotes
                .Where(q => q.CompanyId == companyId)
                .OrderBy(q => q.RequestDate)
                .FirstOrDefaultAsync();

            if (firstQuote == null)
            {
                return new List<MonthOption> 
                { 
                    new MonthOption 
                    { 
                        Date = DateTime.Now, 
                        DisplayName = DateTime.Now.ToString("MMMM yyyy", new CultureInfo("pt-BR")), 
                        HasData = false 
                    } 
                };
            }

            var months = new List<MonthOption>();
            var currentMonth = DateTime.Now;
            var startMonth = new DateTime(firstQuote.RequestDate.Year, firstQuote.RequestDate.Month, 1);

            while (startMonth <= currentMonth)
            {
                var hasData = await _context.Quotes
                    .AnyAsync(q => q.CompanyId == companyId && 
                                  q.RequestDate >= startMonth && 
                                  q.RequestDate < startMonth.AddMonths(1));

                months.Add(new MonthOption
                {
                    Date = startMonth,
                    DisplayName = startMonth.ToString("MMMM yyyy", new CultureInfo("pt-BR")),
                    HasData = hasData
                });

                startMonth = startMonth.AddMonths(1);
            }

            return months.OrderByDescending(m => m.Date).ToList();
        }

        public async Task<byte[]> ExportReportAsync(string companyId, ReportExportRequest request)
        {
            var report = await GenerateMonthlyReportAsync(companyId, request.Month);

            return request.Format.ToLower() switch
            {
                "pdf" => await _exportService.ExportToPdfAsync(report),
                "csv" => await _exportService.ExportToCsvAsync(report),
                _ => throw new ArgumentException($"Unsupported format: {request.Format}")
            };
        }

        private static string GetChartColor(int index)
        {
            var colors = new[] { "#FF6B35", "#F7931E", "#FFD23F", "#06D6A0", "#118AB2", "#073B4C" };
            return colors[index % colors.Length];
        }

        private static string GetRatingColor(int rating)
        {
            return rating switch
            {
                5 => "#4CAF50",
                4 => "#8BC34A",
                3 => "#FFC107",
                2 => "#FF9800",
                1 => "#F44336",
                _ => "#9E9E9E"
            };
        }
    }
}