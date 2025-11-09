using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using SolarEnergy.Models;

namespace SolarEnergy.ViewModels
{
    public class ScheduleVisitViewModel : IValidatableObject
    {
        [Required(ErrorMessage = "Selecione a empresa responsável pela visita.")]
        [Display(Name = "Empresa"])
        public string? CompanyId { get; set; }

        [Required(ErrorMessage = "Selecione o cliente que receberá a visita.")]
        [Display(Name = "Cliente"])
        public string? ClientId { get; set; }

        [Required(ErrorMessage = "Informe o tipo de serviço.")]
        [StringLength(60, ErrorMessage = "O tipo de serviço pode ter no máximo 60 caracteres.")]
        [Display(Name = "Tipo de Serviço"])
        public string? ServiceType { get; set; }

        [Required(ErrorMessage = "Informe a data da visita.")]
        [DataType(DataType.Date)]
        [Display(Name = "Data da Visita"])
        public DateTime? VisitDate { get; set; }

        [Required(ErrorMessage = "Informe o horário da visita.")]
        [DataType(DataType.Time)]
        [Display(Name = "Horário"])
        public TimeSpan? VisitTime { get; set; }

        [Required(ErrorMessage = "Informe o endereço da visita.")]
        [StringLength(200, ErrorMessage = "O endereço pode ter no máximo 200 caracteres.")]
        [Display(Name = "Endereço"])
        public string? Address { get; set; }

        [Display(Name = "Observações"])
        [DataType(DataType.MultilineText)]
        public string? Notes { get; set; }

        public IEnumerable<SelectListItem> Companies { get; set; } = Enumerable.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> Clients { get; set; } = Enumerable.Empty<SelectListItem>();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (VisitDate.HasValue && VisitDate.Value.Date < DateTime.Today)
            {
                yield return new ValidationResult(
                    "A data da visita não pode ser no passado.",
                    new[] { nameof(VisitDate) });
            }

            if (VisitTime.HasValue)
            {
                var min = new TimeSpan(8, 0, 0);
                var max = new TimeSpan(18, 0, 0);
                if (VisitTime.Value < min || VisitTime.Value > max)
                {
                    yield return new ValidationResult(
                        "O horário deve estar entre 08:00 e 18:00.",
                        new[] { nameof(VisitTime) });
                }
            }
        }
    }

    public class TechnicalVisitListItemViewModel
    {
        public long Id { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public string ServiceType { get; set; } = string.Empty;
        public DateTime VisitDate { get; set; }
        public TimeSpan VisitTime { get; set; }
        public TechnicalVisitStatus Status { get; set; }
    }

    public class TechnicalVisitListViewModel
    {
        [Display(Name = "Data")]
        [DataType(DataType.Date)]
        public DateTime? VisitDate { get; set; }

        [Display(Name = "Empresa")]
        public string? CompanyId { get; set; }

        [Display(Name = "Cliente")]
        public string? ClientId { get; set; }

        [Display(Name = "Status")]
        public TechnicalVisitStatus? Status { get; set; }

        public IEnumerable<SelectListItem> CompanyOptions { get; set; } = Enumerable.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> ClientOptions { get; set; } = Enumerable.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> StatusOptions { get; set; } = Enumerable.Empty<SelectListItem>();

        public IEnumerable<TechnicalVisitListItemViewModel> Visits { get; set; } = Enumerable.Empty<TechnicalVisitListItemViewModel>();

        public int PageNumber { get; set; }
        public int TotalPages { get; set; }
        public int TotalItems { get; set; }
    }
}
