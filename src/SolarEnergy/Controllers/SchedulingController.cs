using System.Collections.Generic;
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
            var currentUser = await _userManager.GetUserAsync(User);
            SetUserContext(currentUser);

            var viewModel = new ScheduleVisitViewModel
            {
                Companies = await GetCompaniesAsync(),
                VisitDate = DateTime.Today
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ScheduleVisitViewModel viewModel)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            SetUserContext(currentUser);

            if (currentUser == null)
            {
                TempData["ErrorMessage"] = "Não foi possível identificar o usuário autenticado.";
                return RedirectToAction(nameof(Index));
            }

            if (currentUser.UserType != UserType.Client)
            {
                TempData["ErrorMessage"] = "Somente clientes podem solicitar agendamentos.";
                return RedirectToAction(nameof(List));
            }

            if (!ModelState.IsValid)
            {
                viewModel.Companies = await GetCompaniesAsync();
                return View("Index", viewModel);
            }

            var visitDate = viewModel.VisitDate!.Value.Date;
            var visitTime = viewModel.VisitTime!.Value;

            var hasConflict = await _context.TechnicalVisits
                .AnyAsync(tv => tv.CompanyId == viewModel.CompanyId &&
                                tv.VisitDate == visitDate &&
                                tv.VisitTime == visitTime);

            if (hasConflict)
            {
                ModelState.AddModelError(string.Empty, "Já existe um agendamento para esta empresa neste horário.");
                viewModel.Companies = await GetCompaniesAsync();
                return View("Index", viewModel);
            }

            var visit = new TechnicalVisit
            {
                CompanyId = viewModel.CompanyId!,
                ClientId = currentUser.Id,
                ServiceType = viewModel.ServiceType?.Trim() ?? string.Empty,
                VisitDate = visitDate,
                VisitTime = visitTime,
                Address = string.IsNullOrWhiteSpace(viewModel.Address) ? null : viewModel.Address.Trim(),
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
            var currentUser = await _userManager.GetUserAsync(User);
            SetUserContext(currentUser);

            if (currentUser?.UserType == UserType.Client)
            {
                clientId = currentUser.Id;
            }

            const int pageSize = 10;
            var query = _context.TechnicalVisits
                .AsNoTracking()
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
                .OrderByDescending(tv => tv.VisitDate)
                .ThenByDescending(tv => tv.VisitTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(tv => new TechnicalVisitListItemViewModel
                {
                    Id = tv.Id,
                    CompanyName = tv.Company != null && !string.IsNullOrWhiteSpace(tv.Company.CompanyTradeName)
                        ? tv.Company.CompanyTradeName!
                        : tv.Company != null ? tv.Company.FullName : string.Empty,
                    ClientName = tv.Client != null ? tv.Client.FullName : string.Empty,
                    ServiceType = tv.ServiceType,
                    VisitDate = tv.VisitDate,
                    VisitTime = tv.VisitTime,
                    Status = tv.Status
                })
                .ToListAsync();

            var clientOptions = currentUser?.UserType == UserType.Client
                ? new List<SelectListItem>
                {
                    new SelectListItem
                    {
                        Value = currentUser.Id,
                        Text = !string.IsNullOrWhiteSpace(currentUser.FullName)
                            ? currentUser.FullName
                            : currentUser.Email ?? "Você",
                        Selected = true
                    }
                }
                : await GetClientsAsync(true);

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
                ClientOptions = clientOptions,
                StatusOptions = GetStatusOptions()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(long id)
        {
            var visit = await _context.TechnicalVisits.FindAsync(id);

            if (visit == null)
            {
                TempData["ErrorMessage"] = "Agendamento não encontrado.";
                return RedirectToAction(nameof(List));
            }

            if (visit.Status == TechnicalVisitStatus.Done)
            {
                TempData["WarningMessage"] = "Não é possível confirmar uma visita já concluída.";
                return RedirectToAction(nameof(List));
            }

            if (visit.Status == TechnicalVisitStatus.Confirmed)
            {
                TempData["InfoMessage"] = "O agendamento já está confirmado.";
                return RedirectToAction(nameof(List));
            }

            visit.Status = TechnicalVisitStatus.Confirmed;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Agendamento confirmado com sucesso.";
            return RedirectToAction(nameof(List));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Done(long id)
        {
            var visit = await _context.TechnicalVisits.FindAsync(id);

            if (visit == null)
            {
                TempData["ErrorMessage"] = "Agendamento não encontrado.";
                return RedirectToAction(nameof(List));
            }

            if (visit.Status == TechnicalVisitStatus.Done)
            {
                TempData["InfoMessage"] = "O agendamento já foi concluído.";
                return RedirectToAction(nameof(List));
            }

            visit.Status = TechnicalVisitStatus.Done;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Visita técnica marcada como concluída.";
            return RedirectToAction(nameof(List));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(long id)
        {
            var visit = await _context.TechnicalVisits.FindAsync(id);

            if (visit == null)
            {
                TempData["ErrorMessage"] = "Agendamento não encontrado.";
                return RedirectToAction(nameof(List));
            }

            if (visit.Status == TechnicalVisitStatus.Done)
            {
                TempData["WarningMessage"] = "Não é possível cancelar uma visita concluída.";
                return RedirectToAction(nameof(List));
            }

            visit.Status = TechnicalVisitStatus.Pending;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Agendamento cancelado e marcado como pendente.";
            return RedirectToAction(nameof(List));
        }

        private async Task<IEnumerable<SelectListItem>> GetCompaniesAsync(bool includeEmptyOption = false)
        {
            var companies = await _userManager.Users
                .Where(u => u.UserType == UserType.Company && u.IsActive)
                .OrderBy(u => u.CompanyTradeName ?? u.FullName)
                .AsNoTracking()
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
                .AsNoTracking()
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

        private void SetUserContext(ApplicationUser? user)
        {
            if (user != null)
            {
                ViewData["UserType"] = user.UserType;
            }
        }
    }
}
