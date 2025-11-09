using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SolarEnergy.Data;
using SolarEnergy.Models;
using SolarEnergy.ViewModels;
using SolarEnergy.Services;

namespace SolarEnergy.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILeadService _leadService;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, 
            UserManager<ApplicationUser> userManager, ILeadService leadService)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
            _leadService = leadService;
        }

        // Páginas públicas
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            await SetUserTypeInViewData();
            
            // Se o usuário está logado, redireciona para o dashboard apropriado
            if (User.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.GetUserAsync(User);

                if (user != null)
                {
                    return user.UserType switch
                    {
                        UserType.Company => RedirectToAction(nameof(Leads)), // Empresas vão direto para Leads
                        UserType.Administrator => RedirectToAction(nameof(AdminDashboard)), // Admin mantém AdminDashboard
                        UserType.Client => RedirectToAction(nameof(SearchCompanies)), // Cliente vai direto para buscar empresas
                        _ => RedirectToAction(nameof(SearchCompanies))
                    };
                }

                return RedirectToAction(nameof(SearchCompanies));
            }
            
            return View();
        }

        [AllowAnonymous]
        public async Task<IActionResult> About()
        {
            await SetUserTypeInViewData();
            return View();
        }

        [AllowAnonymous]
        public async Task<IActionResult> Contact()
        {
            await SetUserTypeInViewData();
            return View();
        }

        [AllowAnonymous]
        public async Task<IActionResult> Privacy()
        {
            await SetUserTypeInViewData();
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [AllowAnonymous]
        public async Task<IActionResult> Error()
        {
            await SetUserTypeInViewData();
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // Ações protegidas (exigem login)
        [Authorize]
        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            
            // Redirecionar empresas para a tela de Leads (sua página inicial)
            if (user != null && user.UserType == UserType.Company)
            {
                return RedirectToAction(nameof(Leads));
            }

            await SetUserTypeInViewData();
            return View();
        }

        [Authorize]
        public async Task<IActionResult> Leads()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.UserType != UserType.Company)
            {
                return RedirectToAction(nameof(SearchCompanies));
            }

            // Buscar saldo de leads da empresa
            var leadBalance = await _leadService.GetCompanyLeadBalanceAsync(user.Id);
            var purchaseHistory = await _leadService.GetPurchaseHistoryAsync(user.Id);

            // Buscar todas as solicitações (quotes) recebidas pela empresa
            var quotesQuery = await _context.Quotes
                .Include(q => q.Client)
                .Include(q => q.Proposals)
                .Include(q => q.Messages)
                .Where(q => q.CompanyId == user.Id)
                .OrderByDescending(q => q.RequestDate)
                .ToListAsync();

            var quotes = new List<QuoteLeadViewModel>();
            
            foreach (var q in quotesQuery)
            {
                var hasAccess = await _leadService.HasAccessToLeadAsync(user.Id, q.QuoteId);
                
                var quoteViewModel = new QuoteLeadViewModel
                {
                    QuoteId = q.QuoteId,
                    ClientName = hasAccess ? q.Client.FullName : _leadService.MaskSensitiveData(q.Client.FullName, 1),
                    ClientEmail = hasAccess ? (q.Client.Email ?? "") : _leadService.MaskSensitiveData(q.Client.Email ?? "email", 1),
                    ClientPhone = hasAccess ? (q.Client.Phone ?? "") : _leadService.MaskSensitiveData(q.Client.Phone ?? "telefone", 1),
                    ClientLocation = hasAccess ? (q.Client.Location ?? "Não informado") : _leadService.MaskSensitiveData(q.Client.Location ?? "localização", 1),
                    MonthlyConsumptionKwh = q.MonthlyConsumptionKwh,
                    ServiceType = q.ServiceType,
                    Message = hasAccess ? q.Message : "🔒 Desbloqueie este lead para ver a mensagem completa",
                    RequestDate = q.RequestDate,
                    Status = q.Status,
                    HasProposal = q.Proposals.Any(),
                    ProposalCount = q.Proposals.Count(),
                    UnreadMessagesCount = q.Messages.Where(m => m.SenderId != user.Id && !m.ReadDate.HasValue).Count(),
                    LastMessageDate = q.Messages.Any() ? q.Messages.OrderByDescending(m => m.SentDate).First().SentDate : (DateTime?)null,
                    LastMessage = q.Messages.Any() ? q.Messages.OrderByDescending(m => m.SentDate).First().Message : null,
                    HasAccess = hasAccess,
                    CanPurchaseAccess = leadBalance.AvailableLeads > 0 || hasAccess
                };
                
                quotes.Add(quoteViewModel);
            }

            // Estatísticas para o dashboard
            var stats = new LeadsStatsViewModel
            {
                Total = quotes.Count,
                Novos = quotes.Count(q => q.Status == "Pendente"),
                EmAnalise = quotes.Count(q => q.Status == "Em Análise"),
                PropostasEnviadas = quotes.Count(q => q.HasProposal),
                ConversaoPercentual = quotes.Count > 0 ? (quotes.Count(q => q.HasProposal) * 100 / quotes.Count) : 0
            };

            var model = new CompanyLeadsViewModel
            {
                Stats = stats,
                LeadBalance = new CompanyLeadBalanceInfo
                {
                    AvailableLeads = leadBalance.AvailableLeads,
                    ConsumedLeads = leadBalance.ConsumedLeads,
                    TotalPurchasedLeads = leadBalance.TotalPurchasedLeads,
                    LastPurchaseDate = purchaseHistory.FirstOrDefault()?.PurchaseDate,
                    TotalSpent = purchaseHistory.Sum(p => p.TotalAmount)
                },
                Quotes = quotes,
                CompanyServiceType = user.ServiceType
            };

            await SetUserTypeInViewData();
            return View(model);
        }

        [Authorize]
        public async Task<IActionResult> AdminDashboard()
        {
            await SetUserTypeInViewData();
            return View();
        }

        [Authorize]
        public async Task<IActionResult> Simulation()
        {
            await SetUserTypeInViewData();
            return View();
        }

        [Authorize]
        public async Task<IActionResult> Evaluations()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.UserType != UserType.Company)
            {
                return RedirectToAction(nameof(SearchCompanies));
            }

            // Buscar todas as avaliações recebidas pela empresa
            var reviews = await _context.CompanyReviews
                .Include(r => r.Reviewer)
                .Where(r => r.CompanyId == user.Id)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new CompanyEvaluationViewModel
                {
                    Id = r.Id,
                    ReviewerName = r.Reviewer.FullName,
                    ReviewerLocation = r.Reviewer.Location ?? "Não informado",
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt
                })
                .ToListAsync();

            // Calcular estatísticas
            var ratingsWithValues = reviews.Where(r => r.Rating.HasValue).ToList();
            var stats = new EvaluationStatsViewModel
            {
                TotalReviews = reviews.Count,
                TotalRatings = ratingsWithValues.Count,
                AverageRating = ratingsWithValues.Any() ? ratingsWithValues.Average(r => r.Rating!.Value) : 0,
                FiveStars = ratingsWithValues.Count(r => r.Rating == 5),
                FourStars = ratingsWithValues.Count(r => r.Rating == 4),
                ThreeStars = ratingsWithValues.Count(r => r.Rating == 3),
                TwoStars = ratingsWithValues.Count(r => r.Rating == 2),
                OneStar = ratingsWithValues.Count(r => r.Rating == 1),
                CommentsOnly = reviews.Count(r => !r.Rating.HasValue),
                RecentReviews = reviews.Count(r => r.CreatedAt >= DateTime.Now.AddDays(-30))
            };

            var model = new CompanyEvaluationsViewModel
            {
                Stats = stats,
                Reviews = reviews,
                CompanyName = user.CompanyTradeName ?? user.CompanyLegalName ?? user.FullName
            };

            await SetUserTypeInViewData();
            return View(model);
        }

        [Authorize]
        public async Task<IActionResult> Quotes()
        {
            await SetUserTypeInViewData();
            return View();
        }

        [Authorize]
        public async Task<IActionResult> CompanyPanel()
        {
            await SetUserTypeInViewData();
            return View();
        }

        [Authorize]
        public async Task<IActionResult> UserDashboard()
        {
            await SetUserTypeInViewData();
            return View();
        }

        [Authorize]
        public async Task<IActionResult> SearchCompanies(string? term)
        {
            var searchTerm = term?.Trim();

            var query = _context.Users
                .AsNoTracking()
                .Where(u => u.UserType == UserType.Company && u.IsActive);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(u =>
                    (u.CompanyTradeName != null && EF.Functions.Like(u.CompanyTradeName, $"%{searchTerm}%")) ||
                    (u.CompanyLegalName != null && EF.Functions.Like(u.CompanyLegalName, $"%{searchTerm}%")) ||
                    (u.Location != null && EF.Functions.Like(u.Location, $"%{searchTerm}%")));
            }

            var companies = await query
                .OrderBy(u => u.CompanyTradeName ?? u.CompanyLegalName ?? u.FullName)
                .Select(u => new CompanySummaryViewModel
                {
                    Id = u.Id,
                    Name = u.CompanyTradeName ?? u.CompanyLegalName ?? u.FullName,
                    LegalName = u.CompanyLegalName,
                    TradeName = u.CompanyTradeName,
                    Location = u.Location,
                    Phone = u.CompanyPhone ?? u.PhoneNumber,
                    Website = u.CompanyWebsite,
                    Description = u.CompanyDescription,
                    ServiceType = u.ServiceType,
                    // Calcular média de avaliações para cada empresa
                    AverageRating = _context.CompanyReviews
                        .Where(r => r.CompanyId == u.Id && r.Rating.HasValue)
                        .Average(r => (double?)r.Rating) ?? 0,
                    TotalReviews = _context.CompanyReviews
                        .Count(r => r.CompanyId == u.Id && r.Rating.HasValue)
                })
                .ToListAsync();

            var model = new CompanySearchViewModel
            {
                SearchTerm = searchTerm,
                Companies = companies
            };

            await SetUserTypeInViewData();
            return View(model);
        }

        [Authorize]
        public async Task<IActionResult> CompanyDetails(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var company = await _context.Users
                .AsNoTracking()
                .Where(u => u.Id == id && u.UserType == UserType.Company && u.IsActive)
                .FirstOrDefaultAsync();

            if (company == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            var currentUserId = currentUser?.Id;

            // Verificar se o usuário já tem orçamento com esta empresa
            Quote? existingQuote = null;
            if (currentUser != null && currentUser.UserType == UserType.Client)
            {
                existingQuote = await _context.Quotes
                    .AsNoTracking()
                    .FirstOrDefaultAsync(q => q.ClientId == currentUserId && q.CompanyId == id);
            }

            // Buscar avaliações da empresa
            var reviews = await _context.CompanyReviews
                .AsNoTracking()
                .Include(r => r.Reviewer)
                .Where(r => r.CompanyId == id)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new CompanyReviewViewModel
                {
                    Id = r.Id,
                    ReviewerName = r.Reviewer.FullName,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt,
                    IsCurrentUserReview = r.ReviewerId == currentUserId
                })
                .ToListAsync();

            // Calcular média das avaliações
            var ratingsWithValues = reviews.Where(r => r.Rating.HasValue).ToList();
            var averageRating = ratingsWithValues.Any() ? ratingsWithValues.Average(r => r.Rating!.Value) : 0;

            // Verificar se o usuário atual pode avaliar
            var canUserReview = currentUser != null && 
                               currentUser.UserType == UserType.Client && 
                               currentUser.Id != id;

            // Verificar se o usuário já deu uma nota
            var hasUserAlreadyRated = reviews.Any(r => r.IsCurrentUserReview && r.Rating.HasValue);

            var model = new CompanyDetailsViewModel
            {
                Id = company.Id,
                Name = company.CompanyTradeName ?? company.CompanyLegalName ?? company.FullName,
                LegalName = company.CompanyLegalName,
                TradeName = company.CompanyTradeName,
                Location = company.Location,
                Phone = company.CompanyPhone ?? company.PhoneNumber,
                Website = company.CompanyWebsite,
                Description = company.CompanyDescription,
                ProfileImagePath = company.ProfileImagePath,
                AverageRating = averageRating,
                TotalReviews = ratingsWithValues.Count,
                Reviews = reviews,
                CanUserReview = canUserReview,
                HasUserAlreadyRated = hasUserAlreadyRated,
                HasExistingQuote = existingQuote != null,
                ExistingQuoteId = existingQuote?.QuoteId
            };

            await SetUserTypeInViewData();
            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReview(AddReviewViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Por favor, corrija os erros no formulário.";
                return RedirectToAction(nameof(CompanyDetails), new { id = model.CompanyId });
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null || currentUser.UserType != UserType.Client)
            {
                TempData["ErrorMessage"] = "Apenas clientes podem avaliar empresas.";
                return RedirectToAction(nameof(CompanyDetails), new { id = model.CompanyId });
            }

            // Verificar se a empresa existe
            var companyExists = await _context.Users
                .AnyAsync(u => u.Id == model.CompanyId && u.UserType == UserType.Company && u.IsActive);

            if (!companyExists)
            {
                return NotFound();
            }

            // Verificar se já existe uma avaliação deste usuário para esta empresa
            var existingReview = await _context.CompanyReviews
                .FirstOrDefaultAsync(r => r.CompanyId == model.CompanyId && r.ReviewerId == currentUser.Id);

            if (existingReview != null)
            {
                // Atualizar avaliação existente
                existingReview.Comment = model.Comment;
                existingReview.UpdatedAt = DateTime.Now;
                
                // Só permite alterar a nota se ainda não tinha nota
                if (existingReview.Rating == null && model.Rating.HasValue)
                {
                    existingReview.Rating = model.Rating.Value;
                }

                _context.CompanyReviews.Update(existingReview);
                TempData["SuccessMessage"] = "Sua avaliação foi atualizada com sucesso!";
            }
            else
            {
                // Criar nova avaliação
                var review = new CompanyReview
                {
                    CompanyId = model.CompanyId,
                    ReviewerId = currentUser.Id,
                    Rating = model.Rating,
                    Comment = model.Comment,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _context.CompanyReviews.Add(review);
                TempData["SuccessMessage"] = "Sua avaliação foi adicionada com sucesso!";
            }

            await _context.SaveChangesAsync();
            
            return RedirectToAction(nameof(CompanyDetails), new { id = model.CompanyId });
        }

        [Authorize]
        public async Task<IActionResult> CompanyRedirect()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction(nameof(Index));
            }

            if (user.UserType != UserType.Company)
            {
                return RedirectToAction(nameof(SearchCompanies));
            }

            ViewData["Title"] = "Central da Empresa";
            ViewBag.CompanyName = user.CompanyTradeName ?? user.CompanyLegalName ?? user.FullName;
            await SetUserTypeInViewData();
            return View();
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> ViewQuoteDetails(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.UserType != UserType.Company)
            {
                return Forbid();
            }

            var quote = await _context.Quotes
                .Include(q => q.Client)
                .Include(q => q.Proposals)
                .Include(q => q.Messages)
                .FirstOrDefaultAsync(q => q.QuoteId == id && q.CompanyId == user.Id);

            if (quote == null)
            {
                return NotFound();
            }

            // Verificar se a empresa tem acesso a este lead
            var hasAccess = await _leadService.HasAccessToLeadAsync(user.Id, quote.QuoteId);
            var leadBalance = await _leadService.GetCompanyLeadBalanceAsync(user.Id);

            // Marcar como "Em Análise" se ainda estiver pendente
            if (quote.Status == "Pendente")
            {
                quote.Status = "Em Análise";
                await _context.SaveChangesAsync();
            }

            var model = new QuoteDetailForCompanyViewModel
            {
                QuoteId = quote.QuoteId,
                ClientName = hasAccess ? quote.Client.FullName : _leadService.MaskSensitiveData(quote.Client.FullName, 1),
                ClientEmail = hasAccess ? (quote.Client.Email ?? "") : _leadService.MaskSensitiveData(quote.Client.Email ?? "email", 1),
                ClientPhone = hasAccess ? (quote.Client.Phone ?? "") : _leadService.MaskSensitiveData(quote.Client.Phone ?? "telefone", 1),
                ClientLocation = hasAccess ? (quote.Client.Location ?? "Não informado") : _leadService.MaskSensitiveData(quote.Client.Location ?? "localização", 1),
                MonthlyConsumptionKwh = quote.MonthlyConsumptionKwh,
                ServiceType = quote.ServiceType,
                Message = hasAccess ? quote.Message : "🔒 Desbloqueie este lead para ver a mensagem completa",
                RequestDate = quote.RequestDate,
                Status = quote.Status,
                CompanyServiceType = user.ServiceType,
                HasProposal = quote.Proposals.Any(),
                UnreadMessagesCount = quote.Messages.Where(m => m.SenderId != user.Id && !m.ReadDate.HasValue).Count(),
                HasChatMessages = quote.Messages.Any(),
                HasAccess = hasAccess,
                CanPurchaseAccess = leadBalance.AvailableLeads > 0 || hasAccess,
                Proposals = quote.Proposals.Select(p => new ProposalViewModel
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

            await SetUserTypeInViewData();
            return View(model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendCompanyResponse(CompanyResponseViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.UserType != UserType.Company)
            {
                return Forbid();
            }

            var quote = await _context.Quotes
                .Include(q => q.Client)
                .FirstOrDefaultAsync(q => q.QuoteId == model.QuoteId && q.CompanyId == user.Id);

            if (quote == null)
            {
                return NotFound();
            }

            if (model.ResponseType == "Message")
            {
                // Para empresas de aluguel - enviar mensagem e salvar no CompanyResponseMessage
                if (string.IsNullOrWhiteSpace(model.Message))
                {
                    TempData["ErrorMessage"] = "Por favor, digite uma mensagem para o cliente.";
                    return RedirectToAction(nameof(ViewQuoteDetails), new { id = model.QuoteId });
                }

                // Usar o método existente na model Quote para definir a resposta da empresa
                quote.SetCompanyResponse(model.Message);
                TempData["SuccessMessage"] = "Mensagem enviada com sucesso!";
            }
            else if (model.ResponseType == "Proposal")
            {
                // Para empresas de venda - criar proposta
                if (!ModelState.IsValid)
                {
                    TempData["ErrorMessage"] = "Por favor, preencha todos os campos obrigatórios.";
                    return RedirectToAction(nameof(ViewQuoteDetails), new { id = model.QuoteId });
                }

                var proposal = new Proposal
                {
                    QuoteId = model.QuoteId,
                    Value = model.Value ?? 0,
                    Description = model.Description,
                    InstallationTimeframe = model.InstallationTimeframe,
                    Warranty = model.Warranty,
                    EstimatedMonthlySavings = model.EstimatedMonthlySavings,
                    ValidUntil = model.ValidUntil,
                    ProposalDate = DateTime.Now,
                    Status = "Ativa"
                };

                _context.Proposals.Add(proposal);
                quote.Status = "Proposta Enviada";
                TempData["SuccessMessage"] = "Proposta enviada com sucesso!";
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Leads));
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
