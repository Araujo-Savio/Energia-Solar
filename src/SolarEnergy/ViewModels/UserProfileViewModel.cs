using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using SolarEnergy.Models;

namespace SolarEnergy.ViewModels
{
    public class UserProfileViewModel
    {
        public string Id { get; set; } = string.Empty;

        [Display(Name = "Nome Completo")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "E-mail")]
        public string Email { get; set; } = string.Empty;

        [Phone]
        [Display(Name = "Telefone")]
        public string? Phone { get; set; }

        [Display(Name = "Localização")]
        public string? Location { get; set; }

        [Display(Name = "Cargo")]
        public UserType UserType { get; set; }

        [Display(Name = "Status")]
        public bool IsActive { get; set; }

        [Display(Name = "Data de Cadastro")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Data de Nascimento")]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        [Display(Name = "CPF")]
        public string? CPF { get; set; }

        [Display(Name = "CNPJ")]
        public string? CNPJ { get; set; }

        [Display(Name = "Foto de Perfil")]
        public string? ProfileImagePath { get; set; }

        [Display(Name = "Nova Foto de Perfil")]
        public IFormFile? ProfileImageFile { get; set; }
    }
}
