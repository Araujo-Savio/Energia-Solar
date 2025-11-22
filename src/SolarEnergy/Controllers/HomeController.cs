using System.Diagnostics;
using System.Text.Json;
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

            // Redirecionar admin para AdminDashboard
            if (user != null && user.UserType == UserType.Administrator)
            {
                return RedirectToAction(nameof(AdminDashboard));
            }

            if (user == null || user.UserType != UserType.Client)
            {
                return RedirectToAction(nameof(SearchCompanies));
            }

            await SetUserTypeInViewData();

            // Buscar estatísticas dos orçamentos do cliente
            var userQuotes = await _context.Quotes
                .Include(q => q.Company)
                .Include(q => q.Proposals)
                .Include(q => q.Messages)
                .Where(q => q.ClientId == user.Id)
                // .OrderByDescending(q => q.RequestDate)
                .ToListAsync();

            // Calcular estatísticas
            var statistics = new ClientStatisticsViewModel
            {
                TotalQuotes = userQuotes.Count,
                PendingQuotes = userQuotes.Count(q => q.Status == "Pendente"),
                ReceivedQuotes = userQuotes.Count(q => q.Status == "Respondido" || q.Status == "Proposta Enviada" || q.Proposals.Any()),
                AcceptedQuotes = userQuotes.Count(q => q.Status == "Aceito")
            };

            // Preparar lista dos orçamentos mais recentes (apenas empresa, status e data)
            var recentQuotes = userQuotes.Take(5).Select(q => new QuoteListViewModel
            {
                QuoteId = q.QuoteId,
                CompanyName = q.Company.CompanyTradeName ?? q.Company.CompanyLegalName ?? q.Company.FullName,
                RequestDate = q.RequestDate,
                Status = GetStatusDescription(q),
                HasProposal = q.Proposals.Any(),
                ProposalCount = q.Proposals.Count(),
                UnreadMessagesCount = q.Messages.Where(m => m.SenderId != user.Id && !m.ReadDate.HasValue).Count(),
                LastMessageDate = q.Messages.Any() ? q.Messages.OrderByDescending(m => m.SentDate).First().SentDate : null,
                CompanyResponseMessage = q.CompanyResponseMessage
            }).ToList();

            // Buscar atividades recentes do usuário
            var recentActivities = new List<ActivityItemViewModel>();

            // Adicionar atividades de orçamentos solicitados
            foreach (var quote in userQuotes.OrderByDescending(q => q.RequestDate).Take(10))
            {
                recentActivities.Add(new ActivityItemViewModel
                {
                    Type = ActivityType.QuoteRequested,
                    Title = "Orçamento solicitado",
                    Description = $"Solicitação enviada para {quote.Company.CompanyTradeName ?? quote.Company.CompanyLegalName ?? quote.Company.FullName}",
                    Date = quote.RequestDate,
                    RelatedCompanyName = quote.Company.CompanyTradeName ?? quote.Company.CompanyLegalName ?? quote.Company.FullName,
                    RelatedQuoteId = quote.QuoteId
                });

                // Adicionar atividades de resposta da empresa
                if (quote.CompanyResponseDate.HasValue)
                {
                    recentActivities.Add(new ActivityItemViewModel
                    {
                        Type = ActivityType.QuoteReceived,
                        Title = "Orçamento respondido",
                        Description = $"Resposta recebida da {quote.Company.CompanyTradeName ?? quote.Company.CompanyLegalName ?? quote.Company.FullName}",
                        Date = quote.CompanyResponseDate.Value,
                        RelatedCompanyName = quote.Company.CompanyTradeName ?? quote.Company.CompanyLegalName ?? quote.Company.FullName,
                        RelatedQuoteId = quote.QuoteId
                    });
                }

                // Adicionar atividades de propostas recebidas
                foreach (var proposal in quote.Proposals.OrderByDescending(p => p.ProposalDate).Take(3))
                {
                    recentActivities.Add(new ActivityItemViewModel
                    {
                        Type = ActivityType.ProposalReceived,
                        Title = "Proposta recebida",
                        Description = $"Proposta de R$ {proposal.Value:N2} recebida da {quote.Company.CompanyTradeName ?? quote.Company.CompanyLegalName ?? quote.Company.FullName}",
                        Date = proposal.ProposalDate,
                        RelatedCompanyName = quote.Company.CompanyTradeName ?? quote.Company.CompanyLegalName ?? quote.Company.FullName,
                        RelatedQuoteId = quote.QuoteId
                    });
                }
            }

            // Buscar avaliações enviadas pelo usuário
            var userReviews = await _context.CompanyReviews
                .Include(r => r.Company)
                .Where(r => r.ReviewerId == user.Id)
                .OrderByDescending(r => r.CreatedAt)
                .Take(5)
                .ToListAsync();

            foreach (var review in userReviews)
            {
                recentActivities.Add(new ActivityItemViewModel
                {
                    Type = ActivityType.ReviewSubmitted,
                    Title = "Avaliação enviada",
                    Description = $"Você avaliou a {review.Company.CompanyTradeName ?? review.Company.CompanyLegalName ?? review.Company.FullName}" + 
                                 (review.Rating.HasValue ? $" com {review.Rating} estrelas" : ""),
                    Date = review.CreatedAt,
                    RelatedCompanyName = review.Company.CompanyTradeName ?? review.Company.CompanyLegalName ?? review.Company.FullName
                });
            }

            // Buscar mensagens recentes
            var recentMessages = await _context.QuoteMessages
                .Include(m => m.Quote)
                    .ThenInclude(q => q.Company)
                .Where(m => m.Quote.ClientId == user.Id)
                .OrderByDescending(m => m.SentDate)
                .Take(10)
                .ToListAsync();

            foreach (var message in recentMessages)
            {
                var isFromUser = message.SenderId == user.Id;
                recentActivities.Add(new ActivityItemViewModel
                {
                    Type = isFromUser ? ActivityType.MessageSent : ActivityType.MessageReceived,
                    Title = isFromUser ? "Mensagem enviada" : "Mensagem recebida",
                    Description = isFromUser 
                        ? $"Você enviou uma mensagem para {message.Quote.Company.CompanyTradeName ?? message.Quote.Company.CompanyLegalName ?? message.Quote.Company.FullName}"
                        : $"Mensagem recebida de {message.Quote.Company.CompanyTradeName ?? message.Quote.Company.CompanyLegalName ?? message.Quote.Company.FullName}",
                    Date = message.SentDate,
                    RelatedCompanyName = message.Quote.Company.CompanyTradeName ?? message.Quote.Company.CompanyLegalName ?? message.Quote.Company.FullName,
                    RelatedQuoteId = message.QuoteId
                });
            }

            // Ordenar atividades por data e pegar as 15 mais recentes
            recentActivities = recentActivities
                .OrderByDescending(a => a.Date)
                .Take(15)
                .ToList();

            var model = new ClientDashboardViewModel
            {
                ClientName = user.FullName,
                Statistics = statistics,
                RecentQuotes = recentQuotes,
                RecentActivities = recentActivities
            };

            return View(model);
        }

        private string GetStatusDescription(Quote quote)
        {
            // Se tem proposta, mostrar que foi respondido
            if (quote.Proposals.Any())
            {
                return "Respondido";
            }

            // Se tem resposta da empresa
            if (!string.IsNullOrEmpty(quote.CompanyResponseMessage))
            {
                return "Respondido";
            }

            // Se está em análise
            if (quote.Status == "Em Análise")
            {
                return "Pendente";
            }

            // Outros status
            return quote.Status switch
            {
                "Pendente" => "Pendente",
                "Respondido" => "Respondido",
                "Proposta Enviada" => "Respondido",
                "Aceito" => "Aceito",
                "Recusado" => "Recusado",
                _ => "Pendente"
            };
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
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.UserType != UserType.Administrator)
            {
                return RedirectToAction(nameof(SearchCompanies));
            }

            await SetUserTypeInViewData();
            
            // Redirecionar para o novo AdminController
            return RedirectToAction("Dashboard", "Admin");
        }

        [Authorize]
        public async Task<IActionResult> Simulation(string? companyId)
        {
            await SetUserTypeInViewData();

            var isCompanyUser = User.IsInRole("Company");
            var model = new SimulationViewModel
            {
                IsCompanyUser = isCompanyUser,
                SelectedCompanyId = companyId,
                CompanyParametersJson = "null"
            };

            if (isCompanyUser)
            {
                var companyUser = await _userManager.GetUserAsync(User);
                if (companyUser is not null)
                {
                    var parameters = await _context.CompanyParameters
                        .AsNoTracking()
                        .SingleOrDefaultAsync(p => p.CompanyId == companyUser.Id);

                    var parametersDto = parameters is not null
                        ? MapToParametersInputModel(parameters)
                        : CreateDefaultParameters();

                    model.CompanyParameters = parametersDto;
                    model.CompanyParametersJson = JsonSerializer.Serialize(parametersDto, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });
                }

                return View(model);
            }

            model.Companies = await _context.Users
                .AsNoTracking()
                .Where(u => u.UserType == UserType.Company && u.IsActive)
                .OrderBy(u => u.CompanyTradeName ?? u.CompanyLegalName ?? u.FullName)
                .Select(u => new CompanyOptionViewModel
                {
                    Id = u.Id,
                    Name = u.CompanyTradeName ?? u.CompanyLegalName ?? u.FullName
                })
                .ToListAsync();

            model.SelectedCompanyName = model.Companies.FirstOrDefault(c => c.Id == companyId)?.Name;

            if (!string.IsNullOrWhiteSpace(companyId))
            {
                var parameters = await _context.CompanyParameters
                    .AsNoTracking()
                    .SingleOrDefaultAsync(p => p.CompanyId == companyId);

                var parametersDto = parameters != null
                    ? MapToParametersInputModel(parameters)
                    : CreateDefaultParameters();

                model.CompanyParameters = parametersDto;
                model.CompanyParametersJson = JsonSerializer.Serialize(parametersDto, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
            }

            return View(model);
        }

        private static CompanyParametersInputModel CreateDefaultParameters()
        {
            return new CompanyParametersInputModel
            {
                SystemPricePerKwp = 0m,
                MaintenancePercent = 0m,
                InstallDiscountPercent = 0m,
                RentalFactorPercent = 0m,
                RentalMinMonthly = 0m,
                RentalSetupPerKwp = 0m,
                RentalAnnualIncreasePercent = 0m,
                RentalDiscountPercent = 0m,
                ConsumptionPerKwp = 0m,
                MinSystemSizeKwp = 0m
            };
        }

        private static CompanyParametersInputModel MapToParametersInputModel(CompanyParameters parameters)
        {
            return new CompanyParametersInputModel
            {
                SystemPricePerKwp = parameters.SystemPricePerKwp,
                MaintenancePercent = parameters.MaintenancePercent,
                InstallDiscountPercent = parameters.InstallDiscountPercent,
                RentalFactorPercent = parameters.RentalFactorPercent,
                RentalMinMonthly = parameters.RentalMinMonthly,
                RentalSetupPerKwp = parameters.RentalSetupPerKwp,
                RentalAnnualIncreasePercent = parameters.RentalAnnualIncreasePercent,
                RentalDiscountPercent = parameters.RentalDiscountPercent,
                ConsumptionPerKwp = parameters.ConsumptionPerKwp,
                MinSystemSizeKwp = parameters.MinSystemSizeKwp
            };
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

        [Authorize]
        public async Task<IActionResult> Settings()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction(nameof(Index));
            }

            await SetUserTypeInViewData();

            var model = new UserSettingsViewModel
            {
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? "",
                Phone = user.Phone,
                Location = user.Location,
                ProfileImagePath = user.ProfileImagePath,
                UserType = user.UserType,
                
                // Campos específicos para empresas
                CompanyLegalName = user.CompanyLegalName,
                CompanyTradeName = user.CompanyTradeName,
                CompanyPhone = user.CompanyPhone,
                CompanyWebsite = user.CompanyWebsite,
                CompanyDescription = user.CompanyDescription,
                ServiceType = user.ServiceType,
                
                // Configurações de notificação (valores padrão)
                EmailNotifications = true,
                SmsNotifications = false,
                ProposalNotifications = true,
                MessageNotifications = true,
                MarketingEmails = false,
                SecurityAlerts = true
            };

            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateSettings(UserSettingsViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction(nameof(Index));
            }

            // Atualizar dados da empresa se for empresa (apenas campos da empresa são editáveis nas configurações)
            if (user.UserType == UserType.Company && !string.IsNullOrEmpty(model.CompanyLegalName))
            {
                user.CompanyLegalName = model.CompanyLegalName;
                user.CompanyTradeName = model.CompanyTradeName;
                user.CompanyPhone = model.CompanyPhone;
                user.CompanyWebsite = model.CompanyWebsite;
                user.CompanyDescription = model.CompanyDescription;
                user.ServiceType = model.ServiceType;

                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = "Configurações da empresa atualizadas com sucesso!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Erro ao atualizar configurações: " + string.Join(", ", result.Errors.Select(e => e.Description));
                }
            }
            else
            {
                // Para notificações, apenas salvamos uma mensagem de sucesso
                // (as preferências de notificação seriam salvas em uma tabela separada futuramente)
                TempData["SuccessMessage"] = "Preferências de notificação atualizadas com sucesso!";
            }

            return RedirectToAction(nameof(Settings));
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Por favor, corrija os erros no formulário de senha.";
                return RedirectToAction(nameof(Settings));
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction(nameof(Index));
            }

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Senha alterada com sucesso!";
            }
            else
            {
                TempData["ErrorMessage"] = "Erro ao alterar senha: " + string.Join(", ", result.Errors.Select(e => e.Description));
            }

            return RedirectToAction(nameof(Settings));
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
