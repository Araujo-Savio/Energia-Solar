using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SolarEnergy.Data;
using SolarEnergy.Models;
using SolarEnergy.ViewModels;

namespace SolarEnergy.Controllers
{
    [Authorize]
    public class SchedulingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public SchedulingController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var viewModel = new ScheduleVisitViewModel
            {
                Companies = await GetCompaniesAsync(),
                Clients = await GetClientsAsync(),
                VisitDate = DateTime.Today
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ScheduleVisitViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                viewModel.Companies = await GetCompaniesAsync();
                viewModel.Clients = await GetClientsAsync();
                return View("Index", viewModel);
            }

            var visit = new TechnicalVisit
            {
                CompanyId = viewModel.CompanyId!,
                ClientId = viewModel.ClientId!,
                ServiceType = viewModel.ServiceType!,
                VisitDate = viewModel.VisitDate!.Value.Date,
                VisitTime = viewModel.VisitTime!.Value,
                Address = viewModel.Address!,
                Notes = string.IsNullOrWhiteSpace(viewModel.Notes) ? null : viewModel.Notes.Trim(),
                Status = TechnicalVisitStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _context.TechnicalVisits.Add(visit);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Visita agendada com sucesso.";
            return RedirectToAction(nameof(List));
        }

        [HttpGet]
        public async Task<IActionResult> List(DateTime? visitDate, string? companyId, string? clientId, TechnicalVisitStatus? status, int page = 1)
        {
            const int pageSize = 10;
            var query = _context.TechnicalVisits
                .Include(tv => tv.Company)
                .Include(tv => tv.Client)
                .AsQueryable();

            if (visitDate.HasValue)
            {
                var date = visitDate.Value.Date;
                query = query.Where(tv => tv.VisitDate == date);
            }

            if (!string.IsNullOrWhiteSpace(companyId))
            {
                query = query.Where(tv => tv.CompanyId == companyId);
            }

            if (!string.IsNullOrWhiteSpace(clientId))
            {
                query = query.Where(tv => tv.ClientId == clientId);
            }

            if (status.HasValue)
            {
                query = query.Where(tv => tv.Status == status.Value);
            }

            var totalItems = await query.CountAsync();
            var totalPages = Math.Max(1, (int)Math.Ceiling(totalItems / (double)pageSize));
            page = Math.Clamp(page, 1, totalPages);

            var visits = await query
                .OrderBy(tv => tv.VisitDate)
                .ThenBy(tv => tv.VisitTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(tv => new TechnicalVisitListItemViewModel
                {
                    Id = tv.Id,
                    CompanyName = !string.IsNullOrWhiteSpace(tv.Company.CompanyTradeName)
                        ? tv.Company.CompanyTradeName!
                        : tv.Company.FullName,
                    ClientName = tv.Client.FullName,
                    ServiceType = tv.ServiceType,
                    VisitDate = tv.VisitDate,
                    VisitTime = tv.VisitTime,
                    Status = tv.Status
                })
                .ToListAsync();

            var viewModel = new TechnicalVisitListViewModel
            {
                VisitDate = visitDate,
                CompanyId = companyId,
                ClientId = clientId,
                Status = status,
                Visits = visits,
                PageNumber = page,
                TotalPages = totalPages,
                TotalItems = totalItems,
                CompanyOptions = await GetCompaniesAsync(true),
                ClientOptions = await GetClientsAsync(true),
                StatusOptions = GetStatusOptions()
            };

            return View(viewModel);
        }

        private async Task<IEnumerable<SelectListItem>> GetCompaniesAsync(bool includeEmptyOption = false)
        {
            var companies = await _userManager.Users
                .Where(u => u.UserType == UserType.Company && u.IsActive)
                .OrderBy(u => u.CompanyTradeName ?? u.FullName)
                .Select(u => new SelectListItem
                {
                    Value = u.Id,
                    Text = !string.IsNullOrWhiteSpace(u.CompanyTradeName) ? u.CompanyTradeName! : u.FullName
                })
                .ToListAsync();

            if (includeEmptyOption)
            {
                companies.Insert(0, new SelectListItem { Value = string.Empty, Text = "Todas" });
            }

            return companies;
        }

        private async Task<IEnumerable<SelectListItem>> GetClientsAsync(bool includeEmptyOption = false)
        {
            var clients = await _userManager.Users
                .Where(u => u.UserType == UserType.Client && u.IsActive)
                .OrderBy(u => u.FullName)
                .Select(u => new SelectListItem
                {
                    Value = u.Id,
                    Text = u.FullName
                })
                .ToListAsync();

            if (includeEmptyOption)
            {
                clients.Insert(0, new SelectListItem { Value = string.Empty, Text = "Todos" });
            }

            return clients;
        }

        private IEnumerable<SelectListItem> GetStatusOptions()
        {
            var options = Enum.GetValues(typeof(TechnicalVisitStatus))
                .Cast<TechnicalVisitStatus>()
                .Select(status => new SelectListItem
                {
                    Value = status.ToString(),
                    Text = status switch
                    {
                        TechnicalVisitStatus.Pending => "Pendente",
                        TechnicalVisitStatus.Confirmed => "Confirmada",
                        TechnicalVisitStatus.Done => "Concluída",
                        _ => status.ToString()
                    }
                })
                .ToList();

            options.Insert(0, new SelectListItem { Value = string.Empty, Text = "Todos" });

            return options;
        }
    }
}
