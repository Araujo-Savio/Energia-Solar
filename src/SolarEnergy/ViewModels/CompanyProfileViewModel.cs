using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using SolarEnergy.Models;

namespace SolarEnergy.ViewModels
{
    public class CompanyProfileViewModel
    {
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "O nome completo é obrigatório")]
        [Display(Name = "Nome Completo")]
        [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres")]
        public string FullName { get; set; } = string.Empty;

        [Display(Name = "Razão Social")]
        [StringLength(120, ErrorMessage = "A razão social deve ter no máximo 120 caracteres")]
        public string? CompanyLegalName { get; set; }

        [Display(Name = "Nome Fantasia")]
        [StringLength(120, ErrorMessage = "O nome fantasia deve ter no máximo 120 caracteres")]
        public string? CompanyTradeName { get; set; }

        [Required(ErrorMessage = "O e-mail é obrigatório")]
        [EmailAddress(ErrorMessage = "E-mail inválido")]
        [Display(Name = "E-mail")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Telefone")]
        [Phone(ErrorMessage = "Telefone inválido")]
        public string? Phone { get; set; }

        [Display(Name = "Telefone Comercial")]
        [Phone(ErrorMessage = "Telefone inválido")]
        public string? CompanyPhone { get; set; }

        [Display(Name = "Site da Empresa")]
        [Url(ErrorMessage = "Informe um endereço de site válido")]
        public string? CompanyWebsite { get; set; }

        [Display(Name = "Descrição da Empresa")]
        [StringLength(500, ErrorMessage = "A descrição deve ter no máximo 500 caracteres")]
        public string? CompanyDescription { get; set; }

        [Display(Name = "Tipo de Serviço Solar")]
        public SolarServiceType? ServiceType { get; set; }

        [Display(Name = "Localização")]
        [StringLength(120, ErrorMessage = "A localização deve ter no máximo 120 caracteres")]
        public string? Location { get; set; }

        [Display(Name = "Status da Empresa")]
        public bool IsActive { get; set; }

        [Display(Name = "Foto de Perfil")]
        public string? ProfileImagePath { get; set; }

        [Display(Name = "Nova Foto de Perfil")]
        public IFormFile? ProfileImageFile { get; set; }

        // Campos somente leitura
        [Display(Name = "CPF")]
        public string? CPF { get; set; }

        [Display(Name = "CNPJ")]
        public string? CNPJ { get; set; }

        [Display(Name = "Inscrição Estadual")]
        public string? StateRegistration { get; set; }

        [Display(Name = "Nome do Responsável")]
        public string? ResponsibleName { get; set; }

        [Display(Name = "CPF do Responsável")]
        public string? ResponsibleCPF { get; set; }

        [Display(Name = "Tipo de Usuário")]
        public UserType UserType { get; set; }

        [Display(Name = "Data de Cadastro")]
        public DateTime CreatedAt { get; set; }
    }
}