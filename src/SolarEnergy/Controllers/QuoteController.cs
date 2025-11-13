using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SolarEnergy.Data;
using SolarEnergy.Models;
using SolarEnergy.ViewModels;
using SolarEnergy.Extensions;

namespace SolarEnergy.Controllers
{
    [Authorize]
    public class QuoteController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<QuoteController> _logger;

        public QuoteController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<QuoteController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        [HttpGet]
        [ActionName("Request")]
        public async Task<IActionResult> RequestQuote(string companyId)
        {
            await SetUserTypeInViewData();
            
            if (string.IsNullOrEmpty(companyId))
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null || currentUser.UserType != UserType.Client)
            {
                TempData["ErrorMessage"] = "Apenas clientes podem solicitar orçamentos.";
                return RedirectToAction("SearchCompanies", "Home");
            }

            // Verificar se já existe um orçamento para esta empresa
            var existingQuote = await _context.Quotes
                .FirstOrDefaultAsync(q => q.ClientId == currentUser.Id && q.CompanyId == companyId);

            if (existingQuote != null)
            {
                TempData["InfoMessage"] = "Você já solicitou um orçamento para esta empresa. Aguarde a resposta.";
                return RedirectToAction("MyQuotes");
            }

            var company = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == companyId && u.UserType == UserType.Company && u.IsActive);

            if (company == null)
            {
                return NotFound();
            }

            var model = new RequestQuoteViewModel
            {
                CompanyId = company.Id,
                CompanyName = company.CompanyTradeName ?? company.CompanyLegalName ?? company.FullName,
                CompanyDescription = company.CompanyDescription,
                CompanyLocation = company.Location,
                CompanyWebsite = company.CompanyWebsite,
                ServiceTypeEnum = company.ServiceType
            };

            return View("Request", model);
        }

        [HttpPost]
        [ActionName("Request")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestQuotePost(RequestQuoteViewModel model)
        {
            await SetUserTypeInViewData();
            
            if (!ModelState.IsValid)
            {
                // Recarregar dados da empresa se houver erro
                var company = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == model.CompanyId);
                if (company != null)
                {
                    model.CompanyName = company.CompanyTradeName ?? company.CompanyLegalName ?? company.FullName;
                    model.CompanyDescription = company.CompanyDescription;
                    model.CompanyLocation = company.Location;
                    model.CompanyWebsite = company.CompanyWebsite;
                    model.ServiceTypeEnum = company.ServiceType;
                }
                return View("Request", model);
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null || currentUser.UserType != UserType.Client)
            {
                TempData["ErrorMessage"] = "Apenas clientes podem solicitar orçamentos.";
                return RedirectToAction("SearchCompanies", "Home");
            }

            // Verificar novamente se já existe um orçamento
            var existingQuote = await _context.Quotes
                .FirstOrDefaultAsync(q => q.ClientId == currentUser.Id && q.CompanyId == model.CompanyId);

            if (existingQuote != null)
            {
                TempData["InfoMessage"] = "Você já solicitou um orçamento para esta empresa.";
                return RedirectToAction("MyQuotes");
            }

            var quote = new Quote
            {
                ClientId = currentUser.Id,
                CompanyId = model.CompanyId,
                MonthlyConsumptionKwh = model.MonthlyConsumptionKwh,
                ServiceType = model.ServiceType,
                Message = model.Message,
                RequestDate = DateTime.Now,
                Status = "Pendente"
            };

            _context.Quotes.Add(quote);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Quote requested by client {ClientId} for company {CompanyId}", 
                currentUser.Id, model.CompanyId);

            TempData["SuccessMessage"] = "Orçamento solicitado com sucesso! A empresa entrará em contato em breve.";
            return RedirectToAction("MyQuotes");
        }

        [HttpGet]
        public async Task<IActionResult> MyQuotes()
        {
            await SetUserTypeInViewData();
            
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            // Redirecionar empresas para a tela de Leads
            if (currentUser.UserType == UserType.Company)
            {
                return RedirectToAction("Leads", "Home");
            }

            // Administradores podem ver todos os orçamentos
            List<QuoteListViewModel> quotes;

            if (currentUser.UserType == UserType.Administrator)
            {
                // Administrador vê todos os orçamentos
                quotes = await _context.Quotes
                    .Include(q => q.Client)
                    .Include(q => q.Company)
                    .Include(q => q.Proposals)
                    .OrderByDescending(q => q.RequestDate)
                    .Select(q => new QuoteListViewModel
                    {
                        QuoteId = q.QuoteId,
                        CompanyName = $"{q.Client.FullName} ? {(q.Company.CompanyTradeName ?? q.Company.CompanyLegalName ?? q.Company.FullName)}",
                        MonthlyConsumptionKwh = q.MonthlyConsumptionKwh,
                        ServiceType = q.ServiceType,
                        RequestDate = q.RequestDate,
                        Status = q.Status,
                        HasProposal = q.Proposals.Any(),
                        ProposalCount = q.Proposals.Count
                    })
                    .ToListAsync();
            }
            else
            {
                // Cliente vê apenas seus orçamentos solicitados
                quotes = await _context.Quotes
                    .Include(q => q.Company)
                    .Include(q => q.Proposals)
                    .Where(q => q.ClientId == currentUser.Id)
                    .OrderByDescending(q => q.RequestDate)
                    .Select(q => new QuoteListViewModel
                    {
                        QuoteId = q.QuoteId,
                        CompanyName = q.Company.CompanyTradeName ?? q.Company.CompanyLegalName ?? q.Company.FullName,
                        MonthlyConsumptionKwh = q.MonthlyConsumptionKwh,
                        ServiceType = q.ServiceType,
                        RequestDate = q.RequestDate,
                        Status = q.Status,
                        HasProposal = q.Proposals.Any(),
                        ProposalCount = q.Proposals.Count
                    })
                    .ToListAsync();
            }

            return View(quotes);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            await SetUserTypeInViewData();
            
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var quote = await _context.Quotes
                .Include(q => q.Client)
                .Include(q => q.Company)
                .Include(q => q.Proposals)
                .FirstOrDefaultAsync(q => q.QuoteId == id);

            if (quote == null)
            {
                return NotFound();
            }

            // Verificar permissões
            if (currentUser.UserType == UserType.Client && quote.ClientId != currentUser.Id)
            {
                return Forbid();
            }
            else if (currentUser.UserType == UserType.Company && quote.CompanyId != currentUser.Id)
            {
                return Forbid();
            }

            var model = new QuoteDetailsViewModel
            {
                QuoteId = quote.QuoteId,
                CompanyName = quote.Company.CompanyTradeName ?? quote.Company.CompanyLegalName ?? quote.Company.FullName,
                CompanyId = quote.CompanyId,
                CompanyProfileImagePath = quote.Company.ProfileImagePath,
                MonthlyConsumptionKwh = quote.MonthlyConsumptionKwh,
                ServiceType = quote.ServiceType,
                Message = quote.Message,
                RequestDate = quote.RequestDate,
                Status = quote.Status,
                CompanyResponseMessage = quote.CompanyResponseMessage,
                CompanyResponseDate = quote.CompanyResponseDate,
                Proposals = quote.Proposals.OrderByDescending(p => p.ProposalDate).Select(p => new ProposalViewModel
                {
                    ProposalId = p.ProposalId,
                    Value = p.Value,
                    Description = p.Description,
                    InstallationTimeframe = p.InstallationTimeframe,
                    Warranty = p.Warranty,
                    EstimatedMonthlySavings = p.EstimatedMonthlySavings,
                    ProposalDate = p.ProposalDate,
                    ValidUntil = p.ValidUntil,
                    Status = p.Status
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateQuote(int quoteId, int monthlyConsumptionKwh, string serviceType, string? message)
        {
            await SetUserTypeInViewData();
            
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null || currentUser.UserType != UserType.Client)
            {
                TempData["ErrorMessage"] = "Apenas clientes podem editar orçamentos.";
                return RedirectToAction("MyQuotes");
            }

            var quote = await _context.Quotes
                .Include(q => q.Proposals)
                .FirstOrDefaultAsync(q => q.QuoteId == quoteId && q.ClientId == currentUser.Id);

            if (quote == null)
            {
                return NotFound();
            }

            // Verificar se ainda pode editar (não pode ter propostas ativas)
            if (quote.Proposals.Any(p => p.Status == "Ativa"))
            {
                TempData["ErrorMessage"] = "Não é possível editar orçamentos que já possuem propostas ativas.";
                return RedirectToAction("Details", new { id = quoteId });
            }

            // Validações
            if (monthlyConsumptionKwh < 1 || monthlyConsumptionKwh > 99999)
            {
                TempData["ErrorMessage"] = "O consumo mensal deve estar entre 1 e 99.999 kWh.";
                return RedirectToAction("Details", new { id = quoteId });
            }

            if (string.IsNullOrEmpty(serviceType))
            {
                TempData["ErrorMessage"] = "O tipo de serviço é obrigatório.";
                return RedirectToAction("Details", new { id = quoteId });
            }

            // Atualizar dados
            quote.MonthlyConsumptionKwh = monthlyConsumptionKwh;
            quote.ServiceType = serviceType;
            quote.Message = message;

            // Se estava em análise, volta para pendente
            if (quote.Status == "Em Análise")
            {
                quote.Status = "Pendente";
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Quote {QuoteId} updated by client {ClientId}", quoteId, currentUser.Id);

            TempData["SuccessMessage"] = "Orçamento atualizado com sucesso!";
            return RedirectToAction("Details", new { id = quoteId });
        }

        private async Task SetUserTypeInViewData()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    ViewData["UserType"] = user.UserType;
                }
            }
        }
    }
}