using SolarEnergy.Models;

namespace SolarEnergy.ViewModels
{
    // ViewModel principal do dashboard administrativo
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalClients { get; set; }
        public int TotalCompanies { get; set; }
        public int TotalAdmins { get; set; }
        public int ActiveUsers { get; set; }
        public int InactiveUsers { get; set; }
        public int TotalQuotes { get; set; }
        public int PendingQuotes { get; set; }
        public int CompletedQuotes { get; set; }
        public int TotalProposals { get; set; }
        public int TotalReviews { get; set; }
        public int PendingCompanies { get; set; }
        public int NewUsersLast30Days { get; set; }
        public decimal TotalRevenue { get; set; }
        public List<AdminMonthlyStatViewModel> MonthlyStats { get; set; } = new();
        public List<AdminActivityViewModel> RecentActivities { get; set; } = new();
    }

    // Estatísticas mensais
    public class AdminMonthlyStatViewModel
    {
        public string Month { get; set; } = string.Empty;
        public int NewUsers { get; set; }
        public int NewQuotes { get; set; }
        public decimal Revenue { get; set; }
    }

    // Atividades recentes
    public class AdminActivityViewModel
    {
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
    }

    // Gerenciamento de usuários
    public class AdminUsersPageViewModel
    {
        public List<AdminUserListViewModel> Users { get; set; } = new();
        public string? SearchTerm { get; set; }
        public UserType? SelectedUserType { get; set; }
        public bool? SelectedIsActive { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int TotalUsers { get; set; }
    }

    public class AdminUserListViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public UserType UserType { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CompanyName { get; set; }
        public string? Phone { get; set; }
        public string? Location { get; set; }
    }

    // Detalhes do usuário
    public class AdminUserDetailsViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public UserType UserType { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CPF { get; set; }
        public string? CNPJ { get; set; }
        public string? CompanyLegalName { get; set; }
        public string? CompanyTradeName { get; set; }
        public string? CompanyPhone { get; set; }
        public string? CompanyWebsite { get; set; }
        public string? CompanyDescription { get; set; }
        public string? Location { get; set; }
        public SolarServiceType? ServiceType { get; set; }
        public AdminCompanyStatsViewModel? CompanyStats { get; set; }
        public AdminClientStatsViewModel? ClientStats { get; set; }
    }

    public class AdminCompanyStatsViewModel
    {
        public int TotalQuotes { get; set; }
        public int TotalProposals { get; set; }
        public int TotalReviews { get; set; }
        public double AverageRating { get; set; }
    }

    public class AdminClientStatsViewModel
    {
        public int TotalQuotes { get; set; }
        public int TotalReviews { get; set; }
    }

    // Gerenciamento de empresas
    public class AdminCompaniesPageViewModel
    {
        public List<AdminCompanyListViewModel> Companies { get; set; } = new();
        public string? SearchTerm { get; set; }
        public bool? SelectedIsVerified { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int TotalCompanies { get; set; }
    }

    public class AdminCompanyListViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string? LegalName { get; set; }
        public string? TradeName { get; set; }
        public string? CNPJ { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Location { get; set; }
        public SolarServiceType? ServiceType { get; set; }
        public bool IsVerified { get; set; }
        public DateTime CreatedAt { get; set; }
        public int TotalQuotes { get; set; }
        public int TotalReviews { get; set; }
        public double AverageRating { get; set; }
    }

    // Moderação de avaliações
    public class AdminReviewsPageViewModel
    {
        public List<AdminReviewListViewModel> Reviews { get; set; } = new();
        public string? SearchTerm { get; set; }
        public int? SelectedRating { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int TotalReviews { get; set; }
    }

    public class AdminReviewListViewModel
    {
        public int Id { get; set; }
        public string ReviewerName { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public int? Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CompanyId { get; set; } = string.Empty;
        public string ReviewerId { get; set; } = string.Empty;
    }

    // Relatórios melhorados
    public class AdminReportsViewModel
    {
        public decimal TotalRevenue { get; set; }
        public int NewUsersCount { get; set; }
        public int NewCompaniesCount { get; set; }
        public int TotalQuotes { get; set; }
        public int ClientCount { get; set; }
        public int CompanyCount { get; set; }
        public int ActiveUsers { get; set; }
        public int ActiveSessions { get; set; }
        public int QuotesToday { get; set; }
        public decimal RevenueToday { get; set; }
        
        public List<MonthlyReportData>? MonthlyData { get; set; }
        public List<TopCompanyData>? TopCompanies { get; set; }
        public List<RegionMetricData>? RegionMetrics { get; set; }
    }

    public class MonthlyReportData
    {
        public string Month { get; set; } = string.Empty;
        public int Users { get; set; }
        public decimal Revenue { get; set; }
    }

    // Melhorando dados das top empresas com informações reais
    public class TopCompanyData
    {
        public string CompanyName { get; set; } = string.Empty;
        public int QuoteCount { get; set; }
        public decimal Revenue { get; set; }
        public decimal TotalSpent { get; set; }
        public int TotalLeadsPurchased { get; set; }
        public decimal AverageLeadCost { get; set; }
        public string CompanyId { get; set; } = string.Empty;
        public string? Location { get; set; }
        public DateTime? LastPurchaseDate { get; set; }
        public SolarServiceType? ServiceType { get; set; }
        public double ConversionRate { get; set; }
        
        // Trend indicators
        public decimal RevenueGrowth { get; set; }
        public TrendDirection Trend { get; set; }
        
        // Calculated properties
        public string TrendIcon => Trend switch
        {
            TrendDirection.Up => "fas fa-arrow-up",
            TrendDirection.Down => "fas fa-arrow-down",
            _ => "fas fa-minus"
        };
        
        public string TrendClass => Trend switch
        {
            TrendDirection.Up => "text-success",
            TrendDirection.Down => "text-danger",
            _ => "text-muted"
        };
    }

    public enum TrendDirection
    {
        Up,
        Down,
        Stable
    }

    public class RegionMetricData
    {
        public string State { get; set; } = string.Empty;
        public int UserCount { get; set; }
        public int CompanyCount { get; set; }
        public double ConversionRate { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalQuotes { get; set; }
        public double AverageRating { get; set; }
        public int ActiveCompanies { get; set; }
        public DateTime? LastActivityDate { get; set; }
    }

    // Configurações do sistema
    public class AdminSettingsViewModel
    {
        public string? PlatformName { get; set; }
        public string? ContactEmail { get; set; }
        public string? SupportPhone { get; set; }
        public decimal PlatformCommission { get; set; }
        public string? SmtpServer { get; set; }
        public int SmtpPort { get; set; }
        public string? SenderEmail { get; set; }
        public bool EnableSsl { get; set; }
        public int SessionTimeout { get; set; }
        public int MaxLoginAttempts { get; set; }
        public bool RequireTwoFactor { get; set; }
        public bool EnableAuditLog { get; set; }
        public decimal LeadPrice { get; set; }
        public int FreeLeadsPerCompany { get; set; }
        public int LeadValidityDays { get; set; }
        public bool AutoAssignLeads { get; set; }
        public bool NotifyNewUser { get; set; }
        public bool NotifyNewCompany { get; set; }
        public bool NotifyNewQuote { get; set; }
    }

    public class AdminUsersViewModel
    {
        public List<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
        public int TotalUsers { get; set; }
        public int TotalClients { get; set; }
        public int TotalCompanies { get; set; }
        public int TotalAdmins { get; set; }
        public int PendingUsers { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public string? SearchTerm { get; set; }
        public string? UserTypeFilter { get; set; }
        public string? StatusFilter { get; set; }
    }

    public class AdminCompaniesViewModel
    {
        public List<CompanyViewModel> Companies { get; set; } = new List<CompanyViewModel>();
        public int TotalCompanies { get; set; }
        public int VerifiedCompanies { get; set; }
        public int PendingCompanies { get; set; }
        public int RejectedCompanies { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public string? SearchTerm { get; set; }
        public string? StatusFilter { get; set; }
        public string? StateFilter { get; set; }
    }

    public class CompanyViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string? CompanyName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? CNPJ { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public bool IsVerified { get; set; }
        public bool IsRejected { get; set; }
        public DateTime? VerificationDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public double? AverageRating { get; set; }
        public int ReviewCount { get; set; }
    }
}