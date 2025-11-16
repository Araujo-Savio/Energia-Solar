using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SolarEnergy.Data;
using SolarEnergy.Models;
using SolarEnergy.ViewModels;

namespace SolarEnergy.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AdminController> _logger;

        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ILogger<AdminController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        // Dashboard principal do admin
        public async Task<IActionResult> Dashboard()
        {
            var adminStats = new AdminDashboardViewModel
            {
                TotalUsers = await _context.Users.CountAsync(),
                TotalClients = await _context.Users.CountAsync(u => u.UserType == UserType.Client),
                TotalCompanies = await _context.Users.CountAsync(u => u.UserType == UserType.Company),
                TotalAdmins = await _context.Users.CountAsync(u => u.UserType == UserType.Administrator),
                ActiveUsers = await _context.Users.CountAsync(u => u.IsActive),
                InactiveUsers = await _context.Users.CountAsync(u => !u.IsActive),
                TotalQuotes = await _context.Quotes.CountAsync(),
                PendingQuotes = await _context.Quotes.CountAsync(q => q.Status == "Pendente"),
                CompletedQuotes = await _context.Quotes.CountAsync(q => q.Status == "Aceito" || q.Status == "Concluído"),
                TotalProposals = await _context.Proposals.CountAsync(),
                TotalReviews = await _context.CompanyReviews.CountAsync(),
                PendingCompanies = await _context.Users.CountAsync(u => u.UserType == UserType.Company && !u.IsActive),
                NewUsersLast30Days = await _context.Users.CountAsync(u => u.CreatedAt >= DateTime.Now.AddDays(-30)),
                TotalRevenue = await _context.LeadPurchases
                    .Where(p => p.PaymentStatus == "Completed" || p.PaymentStatus == "Paid")
                    .SumAsync(p => p.TotalAmount)
            };

            // Estatísticas mensais dos últimos 6 meses
            var monthlyStats = new List<AdminMonthlyStatViewModel>();
            for (int i = 5; i >= 0; i--)
            {
                var date = DateTime.Now.AddMonths(-i);
                var startDate = new DateTime(date.Year, date.Month, 1);
                var endDate = startDate.AddMonths(1).AddDays(-1);

                monthlyStats.Add(new AdminMonthlyStatViewModel
                {
                    Month = startDate.ToString("MMM/yyyy"),
                    NewUsers = await _context.Users.CountAsync(u => u.CreatedAt >= startDate && u.CreatedAt <= endDate),
                    NewQuotes = await _context.Quotes.CountAsync(q => q.RequestDate >= startDate && q.RequestDate <= endDate),
                    Revenue = await _context.LeadPurchases
                        .Where(p => p.PurchaseDate >= startDate && p.PurchaseDate <= endDate && 
                                   (p.PaymentStatus == "Completed" || p.PaymentStatus == "Paid"))
                        .SumAsync(p => p.TotalAmount)
                });
            }

            adminStats.MonthlyStats = monthlyStats;

            // Atividades recentes
            var recentActivities = new List<AdminActivityViewModel>();

            // Novos usuários
            var newUsers = await _context.Users
                .OrderByDescending(u => u.CreatedAt)
                .Take(10)
                .Select(u => new AdminActivityViewModel
                {
                    Type = "user_registered",
                    Description = $"{u.FullName} ({GetUserTypeText(u.UserType)}) se cadastrou",
                    Date = u.CreatedAt,
                    UserName = u.FullName,
                    UserId = u.Id
                })
                .ToListAsync();
            recentActivities.AddRange(newUsers);

            // Novos orçamentos
            var newQuotes = await _context.Quotes
                .Include(q => q.Client)
                .Include(q => q.Company)
                .OrderByDescending(q => q.RequestDate)
                .Take(10)
                .Select(q => new AdminActivityViewModel
                {
                    Type = "quote_requested",
                    Description = $"{q.Client.FullName} solicitou orçamento para {q.Company.CompanyTradeName ?? q.Company.FullName}",
                    Date = q.RequestDate,
                    UserName = q.Client.FullName,
                    UserId = q.ClientId
                })
                .ToListAsync();
            recentActivities.AddRange(newQuotes);

            // Novas avaliações
            var newReviews = await _context.CompanyReviews
                .Include(r => r.Reviewer)
                .Include(r => r.Company)
                .OrderByDescending(r => r.CreatedAt)
                .Take(10)
                .Select(r => new AdminActivityViewModel
                {
                    Type = "review_created",
                    Description = $"{r.Reviewer.FullName} avaliou {r.Company.CompanyTradeName ?? r.Company.FullName}",
                    Date = r.CreatedAt,
                    UserName = r.Reviewer.FullName,
                    UserId = r.ReviewerId
                })
                .ToListAsync();
            recentActivities.AddRange(newReviews);

            adminStats.RecentActivities = recentActivities
                .OrderByDescending(a => a.Date)
                .Take(15)
                .ToList();

            return View(adminStats);
        }

        // Gerenciamento de usuários
        public async Task<IActionResult> Users(string? search, string? userType, string? status, int page = 1, int pageSize = 20)
        {
            var query = _context.Users.AsQueryable();

            // Filtros
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(u => u.FullName.Contains(search) || 
                                        u.Email.Contains(search) || 
                                        (u.CompanyTradeName != null && u.CompanyTradeName.Contains(search)));
            }

            if (!string.IsNullOrEmpty(userType) && Enum.TryParse<UserType>(userType, out var userTypeEnum))
            {
                query = query.Where(u => u.UserType == userTypeEnum);
            }

            if (!string.IsNullOrEmpty(status))
            {
                bool isActive = status == "Active";
                query = query.Where(u => u.IsActive == isActive);
            }

            var totalUsers = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalUsers / (double)pageSize);

            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var model = new AdminUsersViewModel
            {
                Users = users,
                TotalUsers = totalUsers,
                TotalClients = await _context.Users.CountAsync(u => u.UserType == UserType.Client),
                TotalCompanies = await _context.Users.CountAsync(u => u.UserType == UserType.Company),
                TotalAdmins = await _context.Users.CountAsync(u => u.UserType == UserType.Administrator),
                PendingUsers = await _context.Users.CountAsync(u => !u.IsActive),
                CurrentPage = page,
                TotalPages = totalPages,
                SearchTerm = search,
                UserTypeFilter = userType,
                StatusFilter = status
            };

            ViewBag.SearchTerm = search;
            ViewBag.UserTypeFilter = userType;
            ViewBag.StatusFilter = status;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalUsers = totalUsers;
            ViewBag.PageSize = pageSize;

            return View(model);
        }

        // Detalhes do usuário
        public async Task<IActionResult> UserDetails(string id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            var model = new AdminUserDetailsViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                UserType = user.UserType,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                CPF = user.CPF,
                CNPJ = user.CNPJ,
                CompanyLegalName = user.CompanyLegalName,
                CompanyTradeName = user.CompanyTradeName,
                CompanyPhone = user.CompanyPhone,
                CompanyWebsite = user.CompanyWebsite,
                CompanyDescription = user.CompanyDescription,
                Location = user.Location,
                ServiceType = user.ServiceType
            };

            // Estatísticas específicas por tipo de usuário
            if (user.UserType == UserType.Company)
            {
                model.CompanyStats = new AdminCompanyStatsViewModel
                {
                    TotalQuotes = await _context.Quotes.CountAsync(q => q.CompanyId == id),
                    TotalProposals = await _context.Proposals.Include(p => p.Quote).CountAsync(p => p.Quote.CompanyId == id),
                    TotalReviews = await _context.CompanyReviews.CountAsync(r => r.CompanyId == id),
                    AverageRating = await _context.CompanyReviews
                        .Where(r => r.CompanyId == id && r.Rating.HasValue)
                        .AverageAsync(r => (double?)r.Rating) ?? 0
                };
            }
            else if (user.UserType == UserType.Client)
            {
                model.ClientStats = new AdminClientStatsViewModel
                {
                    TotalQuotes = await _context.Quotes.CountAsync(q => q.ClientId == id),
                    TotalReviews = await _context.CompanyReviews.CountAsync(r => r.ReviewerId == id)
                };
            }

            return View(model);
        }

        // Ativar/Desativar usuário
        [HttpPost]
        public async Task<IActionResult> ToggleUserStatus(string id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return Json(new { success = false, message = "Usuário não encontrado" });
            }

            user.IsActive = !user.IsActive;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Admin {AdminId} {Action} user {UserId}", 
                (await _userManager.GetUserAsync(User))?.Id, 
                user.IsActive ? "activated" : "deactivated", 
                user.Id);

            return Json(new { 
                success = true, 
                message = $"Usuário {(user.IsActive ? "ativado" : "desativado")} com sucesso",
                isActive = user.IsActive
            });
        }

        // Gerenciamento de empresas
        public async Task<IActionResult> Companies(string? search, string? status, string? state, int page = 1, int pageSize = 20)
        {
            var query = _context.Users
                .Where(u => u.UserType == UserType.Company)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(u => (u.CompanyTradeName != null && u.CompanyTradeName.Contains(search)) ||
                                        (u.CompanyLegalName != null && u.CompanyLegalName.Contains(search)) ||
                                        u.Email.Contains(search));
            }

            if (!string.IsNullOrEmpty(status))
            {
                if (status == "verified")
                {
                    query = query.Where(u => u.IsActive);
                }
                else if (status == "pending")
                {
                    query = query.Where(u => !u.IsActive);
                }
            }

            if (!string.IsNullOrEmpty(state))
            {
                query = query.Where(u => u.Location != null && u.Location.Contains(state));
            }

            var totalCompanies = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCompanies / (double)pageSize);

            var companies = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var companyViewModels = companies.Select(u => new CompanyViewModel
            {
                Id = u.Id,
                CompanyName = u.CompanyTradeName ?? u.CompanyLegalName ?? u.FullName,
                Email = u.Email,
                Phone = u.CompanyPhone,
                CNPJ = u.CNPJ,
                City = u.Location?.Split(',').FirstOrDefault()?.Trim() ?? "N/A",
                State = u.Location?.Split(',').LastOrDefault()?.Trim() ?? "N/A",
                IsVerified = u.IsActive,
                IsRejected = false,
                CreatedAt = u.CreatedAt,
                AverageRating = null,
                ReviewCount = 0
            }).ToList();

            var model = new AdminCompaniesViewModel
            {
                Companies = companyViewModels,
                TotalCompanies = await _context.Users.CountAsync(u => u.UserType == UserType.Company),
                VerifiedCompanies = await _context.Users.CountAsync(u => u.UserType == UserType.Company && u.IsActive),
                PendingCompanies = await _context.Users.CountAsync(u => u.UserType == UserType.Company && !u.IsActive),
                RejectedCompanies = 0,
                CurrentPage = page,
                TotalPages = totalPages,
                SearchTerm = search,
                StatusFilter = status,
                StateFilter = state
            };

            ViewBag.SearchTerm = search;
            ViewBag.StatusFilter = status;
            ViewBag.StateFilter = state;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(model);
        }

        // Verificar empresa
        [HttpPost]
        public async Task<IActionResult> VerifyCompany(string id)
        {
            var company = await _context.Users.FirstOrDefaultAsync(u => u.Id == id && u.UserType == UserType.Company);
            if (company == null)
            {
                return Json(new { success = false, message = "Empresa não encontrada" });
            }

            company.IsActive = true;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Admin {AdminId} verified company {CompanyId}", 
                (await _userManager.GetUserAsync(User))?.Id, company.Id);

            return Json(new { 
                success = true, 
                message = "Empresa verificada com sucesso"
            });
        }

        // Moderação de avaliações
        public async Task<IActionResult> Reviews(string? search, int? rating, int page = 1, int pageSize = 20)
        {
            var query = _context.CompanyReviews
                .Include(r => r.Reviewer)
                .Include(r => r.Company)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(r => r.Comment.Contains(search) ||
                                        r.Reviewer.FullName.Contains(search) ||
                                        r.Company.CompanyTradeName.Contains(search) ||
                                        r.Company.CompanyLegalName.Contains(search));
            }

            if (rating.HasValue)
            {
                query = query.Where(r => r.Rating == rating.Value);
            }

            var totalReviews = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalReviews / (double)pageSize);

            var reviews = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new AdminReviewListViewModel
                {
                    Id = r.Id,
                    ReviewerName = r.Reviewer.FullName,
                    CompanyName = r.Company.CompanyTradeName ?? r.Company.CompanyLegalName ?? r.Company.FullName,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt,
                    CompanyId = r.CompanyId,
                    ReviewerId = r.ReviewerId
                })
                .ToListAsync();

            var model = new AdminReviewsPageViewModel
            {
                Reviews = reviews,
                SearchTerm = search,
                SelectedRating = rating,
                CurrentPage = page,
                TotalPages = totalPages,
                PageSize = pageSize,
                TotalReviews = totalReviews
            };

            return View(model);
        }

        // Deletar avaliação
        [HttpPost]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var review = await _context.CompanyReviews.FirstOrDefaultAsync(r => r.Id == id);
            if (review == null)
            {
                return Json(new { success = false, message = "Avaliação não encontrada" });
            }

            _context.CompanyReviews.Remove(review);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Admin {AdminId} deleted review {ReviewId}", 
                (await _userManager.GetUserAsync(User))?.Id, review.Id);

            return Json(new { 
                success = true, 
                message = "Avaliação removida com sucesso"
            });
        }

        // Relatórios melhorados com dados reais
        public async Task<IActionResult> Reports()
        {
            // Calcular receita real baseada nas compras de leads das empresas
            var totalRevenue = await _context.LeadPurchases
                .Where(p => p.PaymentStatus == "Completed" || p.PaymentStatus == "Paid")
                .SumAsync(p => p.TotalAmount);

            // Usuários novos no último mês
            var newUsersCount = await _context.Users
                .CountAsync(u => u.CreatedAt >= DateTime.Now.AddMonths(-1));

            // Empresas novas no último mês
            var newCompaniesCount = await _context.Users
                .CountAsync(u => u.UserType == UserType.Company && u.CreatedAt >= DateTime.Now.AddMonths(-1));

            // Receita de hoje
            var revenueToday = await _context.LeadPurchases
                .Where(p => p.PurchaseDate.Date == DateTime.Today && 
                           (p.PaymentStatus == "Completed" || p.PaymentStatus == "Paid"))
                .SumAsync(p => p.TotalAmount);

            // Dados mensais dos últimos 6 meses
            var monthlyData = new List<MonthlyReportData>();
            for (int i = 5; i >= 0; i--)
            {
                var date = DateTime.Now.AddMonths(-i);
                var startDate = new DateTime(date.Year, date.Month, 1);
                var endDate = startDate.AddMonths(1).AddDays(-1);

                var users = await _context.Users
                    .CountAsync(u => u.CreatedAt >= startDate && u.CreatedAt <= endDate);

                var revenue = await _context.LeadPurchases
                    .Where(p => p.PurchaseDate >= startDate && p.PurchaseDate <= endDate && 
                               (p.PaymentStatus == "Completed" || p.PaymentStatus == "Paid"))
                    .SumAsync(p => p.TotalAmount);

                monthlyData.Add(new MonthlyReportData
                {
                    Month = startDate.ToString("MMM"),
                    Users = users,
                    Revenue = revenue
                });
            }

            // Top empresas que mais gastaram e receberam orçamentos
            var companyData = await _context.Users
                .Where(u => u.UserType == UserType.Company)
                .ToListAsync();

            var topCompanies = new List<TopCompanyData>();
            
            foreach (var company in companyData)
            {
                var totalSpent = await _context.LeadPurchases
                    .Where(p => p.CompanyId == company.Id && 
                               (p.PaymentStatus == "Completed" || p.PaymentStatus == "Paid"))
                    .SumAsync(p => p.TotalAmount);
                
                var quoteCount = await _context.Quotes.CountAsync(q => q.CompanyId == company.Id);

                if (totalSpent > 0 || quoteCount > 0)
                {
                    topCompanies.Add(new TopCompanyData
                    {
                        CompanyName = company.CompanyTradeName ?? company.CompanyLegalName ?? company.FullName,
                        QuoteCount = quoteCount,
                        Revenue = totalSpent
                    });
                }
            }

            var topCompanyData = topCompanies
                .OrderByDescending(tc => tc.Revenue)
                .Take(5)
                .ToList();

            // Métricas por região baseadas nos dados reais
            var companyLocations = await _context.Users
                .Where(u => u.UserType == UserType.Company && !string.IsNullOrEmpty(u.Location))
                .ToListAsync();

            var regionMetricData = new List<RegionMetricData>();

            var locationGroups = companyLocations
                .GroupBy(u => u.Location?.Split(',').LastOrDefault()?.Trim() ?? "N/A")
                .Take(5);

            foreach (var group in locationGroups)
            {
                var state = group.Key;
                var companyCount = group.Count();
                var userCount = await _context.Users
                    .CountAsync(u => u.UserType == UserType.Client && 
                               !string.IsNullOrEmpty(u.Location) &&
                               u.Location.Contains(state));

                regionMetricData.Add(new RegionMetricData
                {
                    State = state,
                    UserCount = userCount,
                    CompanyCount = companyCount,
                    ConversionRate = userCount > 0 && companyCount > 0 ? 
                        Math.Round((double)companyCount / userCount * 100, 1) : 0
                });
            }

            var model = new AdminReportsViewModel
            {
                TotalRevenue = totalRevenue,
                NewUsersCount = newUsersCount,
                NewCompaniesCount = newCompaniesCount,
                TotalQuotes = await _context.Quotes.CountAsync(),
                ClientCount = await _context.Users.CountAsync(u => u.UserType == UserType.Client),
                CompanyCount = await _context.Users.CountAsync(u => u.UserType == UserType.Company),
                ActiveUsers = 0, // Placeholder - seria necessário rastreamento de sessão
                ActiveSessions = 0, // Placeholder - seria necessário rastreamento de sessão
                QuotesToday = await _context.Quotes.CountAsync(q => q.RequestDate.Date == DateTime.Today),
                RevenueToday = revenueToday,
                MonthlyData = monthlyData,
                TopCompanies = topCompanyData,
                RegionMetrics = regionMetricData
            };

            return View(model);
        }

        // Configurações do sistema
        public async Task<IActionResult> Settings()
        {
            var model = new AdminSettingsViewModel
            {
                PlatformName = "Solar Energy",
                ContactEmail = "contato@solarenergy.com",
                SupportPhone = "(11) 99999-9999",
                PlatformCommission = 5.0m,
                SmtpServer = "smtp.gmail.com",
                SmtpPort = 587,
                SenderEmail = "noreply@solarenergy.com",
                EnableSsl = true,
                SessionTimeout = 60,
                MaxLoginAttempts = 5,
                RequireTwoFactor = false,
                EnableAuditLog = true,
                LeadPrice = 50.00m,
                FreeLeadsPerCompany = 3,
                LeadValidityDays = 30,
                AutoAssignLeads = true,
                NotifyNewUser = true,
                NotifyNewCompany = true,
                NotifyNewQuote = true
            };

            return View(model);
        }

        // Endpoint para notificações (usado pelo JavaScript)
        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            try
            {
                var notifications = new List<object>();

                // Verificar empresas pendentes de aprovação
                var pendingCompanies = await _context.Users
                    .CountAsync(u => u.UserType == UserType.Company && !u.IsActive);

                if (pendingCompanies > 0)
                {
                    notifications.Add(new
                    {
                        type = "info",
                        message = $"Há {pendingCompanies} empresa(s) aguardando aprovação."
                    });
                }

                // Verificar orçamentos recentes (últimas 24h)
                var recentQuotes = await _context.Quotes
                    .CountAsync(q => q.RequestDate >= DateTime.Now.AddHours(-24));

                if (recentQuotes > 5)
                {
                    notifications.Add(new
                    {
                        type = "success",
                        message = $"{recentQuotes} novos orçamentos nas últimas 24 horas."
                    });
                }

                // Verificar possíveis problemas com usuários inativos há muito tempo
                var staleUsers = await _context.Users
                    .CountAsync(u => !u.IsActive && u.CreatedAt <= DateTime.Now.AddDays(-30));

                if (staleUsers > 10)
                {
                    notifications.Add(new
                    {
                        type = "warning",
                        message = $"{staleUsers} usuários inativos há mais de 30 dias."
                    });
                }

                return Json(notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar notificações do admin");
                return Json(new List<object>());
            }
        }

        private static string GetUserTypeText(UserType userType)
        {
            return userType switch
            {
                UserType.Client => "Cliente",
                UserType.Company => "Empresa",
                UserType.Administrator => "Administrador",
                _ => "Desconhecido"
            };
        }
    }
}