using System.ComponentModel.DataAnnotations;

namespace SolarEnergy.ViewModels
{
    public class RequestQuoteViewModel
    {
        [Required(ErrorMessage = "O ID da empresa é obrigatório")]
        public string CompanyId { get; set; } = string.Empty;

        public string CompanyName { get; set; } = string.Empty;

        [Required(ErrorMessage = "O consumo mensal é obrigatório")]
        [Display(Name = "Consumo Médio Mensal (kWh)")]
        [Range(1, 99999, ErrorMessage = "O consumo deve ser entre 1 e 99.999 kWh")]
        public int MonthlyConsumptionKwh { get; set; }

        [Required(ErrorMessage = "O tipo de serviço é obrigatório")]
        [Display(Name = "Tipo de Serviço")]
        public string ServiceType { get; set; } = string.Empty;

        [Display(Name = "Mensagem Adicional")]
        [StringLength(1000, ErrorMessage = "A mensagem deve ter no máximo 1000 caracteres")]
        public string? Message { get; set; }

        // Informações adicionais da empresa para exibição
        public string? CompanyDescription { get; set; }
        public string? CompanyLocation { get; set; }
        public string? CompanyWebsite { get; set; }
        public SolarEnergy.Models.SolarServiceType? ServiceTypeEnum { get; set; }
    }

    public class QuoteListViewModel
    {
        public int QuoteId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public int MonthlyConsumptionKwh { get; set; }
        public string ServiceType { get; set; } = string.Empty;
        public DateTime RequestDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool HasProposal { get; set; }
        public int ProposalCount { get; set; }
    }

    public class QuoteDetailsViewModel
    {
        public int QuoteId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string CompanyId { get; set; } = string.Empty;
        public string? CompanyProfileImagePath { get; set; }
        public int MonthlyConsumptionKwh { get; set; }
        public string ServiceType { get; set; } = string.Empty;
        public string? Message { get; set; }
        public DateTime RequestDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<ProposalViewModel> Proposals { get; set; } = new();
        
        // Campos para resposta da empresa
        public string? CompanyResponseMessage { get; set; }
        public DateTime? CompanyResponseDate { get; set; }
    }

    public class ProposalViewModel
    {
        public int ProposalId { get; set; }
        public decimal Value { get; set; }
        public string? Description { get; set; }
        public int? InstallationTimeframe { get; set; }
        public int? Warranty { get; set; }
        public decimal? EstimatedMonthlySavings { get; set; }
        public DateTime ProposalDate { get; set; }
        public DateTime? ValidUntil { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}