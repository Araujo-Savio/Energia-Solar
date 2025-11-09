using System.ComponentModel.DataAnnotations;
using SolarEnergy.Models;

namespace SolarEnergy.ViewModels
{
    public class UserSettingsViewModel
    {
        public string UserId { get; set; } = string.Empty;

        [Required(ErrorMessage = "O nome completo é obrigatório")]
        [Display(Name = "Nome Completo")]
        [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "O email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        [Display(Name = "Email")]
        [StringLength(100, ErrorMessage = "O email deve ter no máximo 100 caracteres")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Telefone")]
        [Phone(ErrorMessage = "Telefone inválido")]
        [StringLength(20, ErrorMessage = "O telefone deve ter no máximo 20 caracteres")]
        public string? Phone { get; set; }

        [Display(Name = "Localização")]
        [StringLength(120, ErrorMessage = "A localização deve ter no máximo 120 caracteres")]
        public string? Location { get; set; }

        [Display(Name = "Foto de Perfil")]
        public string? ProfileImagePath { get; set; }

        public UserType UserType { get; set; }

        // Campos específicos para empresas
        [Display(Name = "Razão Social")]
        [StringLength(120, ErrorMessage = "A razão social deve ter no máximo 120 caracteres")]
        public string? CompanyLegalName { get; set; }

        [Display(Name = "Nome Fantasia")]
        [StringLength(120, ErrorMessage = "O nome fantasia deve ter no máximo 120 caracteres")]
        public string? CompanyTradeName { get; set; }

        [Display(Name = "Telefone Comercial")]
        [Phone(ErrorMessage = "Telefone comercial inválido")]
        [StringLength(20, ErrorMessage = "O telefone comercial deve ter no máximo 20 caracteres")]
        public string? CompanyPhone { get; set; }

        [Display(Name = "Website")]
        [Url(ErrorMessage = "URL inválida")]
        [StringLength(200, ErrorMessage = "O website deve ter no máximo 200 caracteres")]
        public string? CompanyWebsite { get; set; }

        [Display(Name = "Descrição da Empresa")]
        [StringLength(500, ErrorMessage = "A descrição deve ter no máximo 500 caracteres")]
        public string? CompanyDescription { get; set; }

        [Display(Name = "Tipo de Serviço Solar")]
        public SolarServiceType? ServiceType { get; set; }

        // Configurações de notificação
        [Display(Name = "Receber notificações por email")]
        public bool EmailNotifications { get; set; } = true;

        [Display(Name = "Receber notificações por SMS")]
        public bool SmsNotifications { get; set; } = false;

        [Display(Name = "Notificações de propostas")]
        public bool ProposalNotifications { get; set; } = true;

        [Display(Name = "Notificações de mensagens")]
        public bool MessageNotifications { get; set; } = true;

        [Display(Name = "Receber emails de marketing")]
        public bool MarketingEmails { get; set; } = false;

        [Display(Name = "Alertas de segurança")]
        public bool SecurityAlerts { get; set; } = true;
    }

    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "A senha atual é obrigatória")]
        [DataType(DataType.Password)]
        [Display(Name = "Senha Atual")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "A nova senha é obrigatória")]
        [StringLength(100, ErrorMessage = "A senha deve ter pelo menos {2} e no máximo {1} caracteres.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Nova Senha")]
        public string NewPassword { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar Nova Senha")]
        [Compare("NewPassword", ErrorMessage = "A nova senha e a confirmação não coincidem.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}