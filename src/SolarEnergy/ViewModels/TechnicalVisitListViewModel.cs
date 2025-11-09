using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using SolarEnergy.Models;

namespace SolarEnergy.ViewModels
{
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
