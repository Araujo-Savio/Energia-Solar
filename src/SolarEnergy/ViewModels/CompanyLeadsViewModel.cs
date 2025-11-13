namespace SolarEnergy.ViewModels
{
    public class CompanyLeadsViewModel
    {
        public LeadsStatsViewModel Stats { get; set; } = new();
        public List<QuoteLeadViewModel> Quotes { get; set; } = new();
        public Models.SolarServiceType? CompanyServiceType { get; set; }
    }

    public class LeadsStatsViewModel
    {
        public int Total { get; set; }
        public int Novos { get; set; }
        public int EmAnalise { get; set; }
        public int PropostasEnviadas { get; set; }
        public int ConversaoPercentual { get; set; }
    }

    public class QuoteLeadViewModel
    {
        public int QuoteId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string ClientEmail { get; set; } = string.Empty;
        public string ClientPhone { get; set; } = string.Empty;
        public string ClientLocation { get; set; } = string.Empty;
        public int MonthlyConsumptionKwh { get; set; }
        public string ServiceType { get; set; } = string.Empty;
        public string? Message { get; set; }
        public DateTime RequestDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool HasProposal { get; set; }
        public int ProposalCount { get; set; }
    }

    public class QuoteDetailForCompanyViewModel
    {
        public int QuoteId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string ClientEmail { get; set; } = string.Empty;
        public string ClientPhone { get; set; } = string.Empty;
        public string ClientLocation { get; set; } = string.Empty;
        public int MonthlyConsumptionKwh { get; set; }
        public string ServiceType { get; set; } = string.Empty;
        public string? Message { get; set; }
        public DateTime RequestDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public Models.SolarServiceType? CompanyServiceType { get; set; }
        public bool HasProposal { get; set; }
        public List<ProposalViewModel> Proposals { get; set; } = new();
    }

    public class CompanyResponseViewModel
    {
        public int QuoteId { get; set; }
        public string ResponseType { get; set; } = string.Empty; // "Message" ou "Proposal"
        public string? Message { get; set; }
        
        // Para propostas (venda de painéis)
        public decimal? Value { get; set; }
        public string? Description { get; set; }
        public int? InstallationTimeframe { get; set; }
        public int? Warranty { get; set; }
        public decimal? EstimatedMonthlySavings { get; set; }
        public DateTime? ValidUntil { get; set; }
    }
}