using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace SolarEnergy.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [Display(Name = "Nome Completo")]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Display(Name = "CPF")]
        [StringLength(14)]
        public string? CPF { get; set; }

        [Display(Name = "CNPJ")]
        [StringLength(18)]
        public string? CNPJ { get; set; }

        [Display(Name = "Telefone")]
        [StringLength(20)]
        public string? Phone { get; set; }

        [Display(Name = "Razão Social")]
        [StringLength(120)]
        public string? CompanyLegalName { get; set; }

        [Display(Name = "Nome Fantasia")]
        [StringLength(120)]
        public string? CompanyTradeName { get; set; }

        [Display(Name = "Inscrição Estadual")]
        [StringLength(30)]
        public string? StateRegistration { get; set; }

        [Display(Name = "Telefone Comercial")]
        [StringLength(20)]
        public string? CompanyPhone { get; set; }

        [Display(Name = "Site da Empresa")]
        [StringLength(200)]
        public string? CompanyWebsite { get; set; }

        [Display(Name = "Descrição da Empresa")]
        [StringLength(500)]
        public string? CompanyDescription { get; set; }

        [Display(Name = "Nome do Responsável")]
        [StringLength(100)]
        public string? ResponsibleName { get; set; }

        [Display(Name = "CPF do Responsável")]
        [StringLength(14)]
        public string? ResponsibleCPF { get; set; }

        [Display(Name = "Área de Atendimento")]
        [StringLength(120, ErrorMessage = "Informe as cidades/estados onde sua empresa atende")]
        public string? Location { get; set; }

        [Display(Name = "Foto de Perfil")]
        [StringLength(260)]
        public string? ProfileImagePath { get; set; } = "/images/default-profile.svg";

        [Display(Name = "Data de Nascimento")]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        [Display(Name = "Tipo de Usuário")]
        public UserType UserType { get; set; } = UserType.Client;

        [Display(Name = "Tipo de Serviço Solar")]
        public SolarServiceType? ServiceType { get; set; }

        [Display(Name = "Data de Cadastro")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool IsActive { get; set; } = true;
    }

    public enum UserType
    {
        Client = 1,
        Company = 2,
        Administrator = 3
    }

    public enum SolarServiceType
    {
        [Display(Name = "Venda de Painéis Solares")]
        PanelSales = 1,

        [Display(Name = "Aluguel de Energia Solar")]
        EnergyRental = 2,

        [Display(Name = "Venda e Aluguel")]
        Both = 3
    }
}
