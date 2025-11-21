using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SolarEnergy.Data;
using SolarEnergy.Models;
using SolarEnergy.ViewModels;

namespace SolarEnergy.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<AdminController> logger)
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
                CompletedQuotes = await _context.Quotes.CountAsync(q => q.Status == "Aceito" || q.Status == "Conclu�do"),
                TotalProposals = await _context.Proposals.CountAsync(),
                TotalReviews = await _context.CompanyReviews.CountAsync(),
                PendingCompanies = await _context.Users.CountAsync(u => u.UserType == UserType.Company && !u.IsActive),
                NewUsersLast30Days = await _context.Users.CountAsync(u => u.CreatedAt >= DateTime.Now.AddDays(-30)),
                TotalRevenue = await _context.LeadPurchases
                    .Where(p => p.PaymentStatus == "Completed" || p.PaymentStatus == "Paid")
                    .SumAsync(p => p.TotalAmount)
            };

            // Estat�sticas mensais dos �ltimos 6 meses
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
                        .Where(p => p.PurchaseDate >= startDate &&
                                    p.PurchaseDate <= endDate &&
                                    (p.PaymentStatus == "Completed" || p.PaymentStatus == "Paid"))
                        .SumAsync(p => p.TotalAmount)
                });
            }

            adminStats.MonthlyStats = monthlyStats;

            // Atividades recentes
            var recentActivities = new List<AdminActivityViewModel>();

            // Novos usu�rios
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

            // Novos or�amentos
            var newQuotes = await _context.Quotes
                .Include(q => q.Client)
                .Include(q => q.Company)
                .OrderByDescending(q => q.RequestDate)
                .Take(10)
                .Select(q => new AdminActivityViewModel
                {
                    Type = "quote_requested",
                    Description = $"{q.Client.FullName} solicitou or�amento para {q.Company.CompanyTradeName ?? q.Company.FullName}",
                    Date = q.RequestDate,
                    UserName = q.Client.FullName,
                    UserId = q.ClientId
                })
                .ToListAsync();

            recentActivities.AddRange(newQuotes);

            // Novas avalia��es
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

        // Gerenciamento de usu�rios
        public async Task<IActionResult> Users(string? search, string? userType, string? status, int page = 1, int pageSize = 20)
        {
            var query = _context.Users.AsQueryable();

            // Filtros
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(u =>
                    (u.FullName != null && u.FullName.Contains(search)) ||
                    (u.Email != null && u.Email.Contains(search)) ||
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

        // Detalhes do usu�rio
        public async Task<IActionResult> UserDetails(string id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            var model = new AdminUserDetailsViewModel
            {
                Id = user.Id!,
                FullName = user.FullName ?? "",
                Email = user.Email ?? "",
                Phone = user.Phone ?? "",
                UserType = user.UserType,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                CPF = user.CPF ?? "",
                CNPJ = user.CNPJ ?? "",
                CompanyLegalName = user.CompanyLegalName ?? "",
                CompanyTradeName = user.CompanyTradeName ?? "",
                CompanyPhone = user.CompanyPhone ?? "",
                CompanyWebsite = user.CompanyWebsite ?? "",
                CompanyDescription = user.CompanyDescription ?? "",
                Location = user.Location ?? "",
                ServiceType = user.ServiceType?.ToString() ?? ""

            };



            // Estat�sticas espec�ficas por tipo de usu�rio
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

        // Ativar/Desativar usu�rio
        [HttpPost]
        public async Task<IActionResult> ToggleUserStatus(string id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return Json(new { success = false, message = "Usu�rio n�o encontrado" });
            }

            user.IsActive = !user.IsActive;
            await _context.SaveChangesAsync();

            var admin = await _userManager.GetUserAsync(User);

            _logger.LogInformation(
                "Admin {AdminId} {Action} user {UserId}",
                admin?.Id,
                user.IsActive ? "activated" : "deactivated",
                user.Id
            );

            return Json(new
            {
                success = true,
                message = $"Usu�rio {(user.IsActive ? "ativado" : "desativado")} com sucesso",
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
                query = query.Where(u =>
                    (u.CompanyTradeName != null && u.CompanyTradeName.Contains(search)) ||
                    (u.CompanyLegalName != null && u.CompanyLegalName.Contains(search)) ||
                    (u.Email != null && u.Email.Contains(search)));
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

            var companyViewModels = companies
                .Select(u => new CompanyViewModel
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
                })
                .ToList();

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
            var company = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id && u.UserType == UserType.Company);

            if (company == null)
            {
                return Json(new { success = false, message = "Empresa n�o encontrada" });
            }

            company.IsActive = true;
            await _context.SaveChangesAsync();

            var admin = await _userManager.GetUserAsync(User);

            _logger.LogInformation(
                "Admin {AdminId} verified company {CompanyId}",
                admin?.Id,
                company.Id
            );

            return Json(new { success = true, message = "Empresa verificada com sucesso" });
        }

        // Modera��o de avalia��es
        public async Task<IActionResult> Reviews(string? search, int? rating, int page = 1, int pageSize = 20)
        {
            var query = _context.CompanyReviews
                .Include(r => r.Reviewer)
                .Include(r => r.Company)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(u =>
                    (u.Comment != null && u.Comment.Contains(search)) ||
                    (u.Reviewer.FullName != null && u.Reviewer.FullName.Contains(search)) ||
                    (u.Company.CompanyTradeName != null && u.Company.CompanyTradeName.Contains(search)) ||
                    (u.Company.CompanyLegalName != null && u.Company.CompanyLegalName.Contains(search)));
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

        // Deletar avalia��o
        [HttpPost]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var review = await _context.CompanyReviews.FirstOrDefaultAsync(r => r.Id == id);

            if (review == null)
            {
                return Json(new { success = false, message = "Avalia��o n�o encontrada" });
            }

            _context.CompanyReviews.Remove(review);
            await _context.SaveChangesAsync();

            var admin = await _userManager.GetUserAsync(User);

            _logger.LogInformation(
                "Admin {AdminId} deleted review {ReviewId}",
                admin?.Id,
                review.Id
            );

            return Json(new { success = true, message = "Avalia��o removida com sucesso" });
        }

        // Relat�rios melhorados com dados reais
        public async Task<IActionResult> Reports()
        {
            // Calcular receita real baseada nas compras de leads das empresas
            var totalRevenue = await _context.LeadPurchases
                .Where(p => p.PaymentStatus == "Completed" || p.PaymentStatus == "Paid")
                .SumAsync(p => p.TotalAmount);

            // Usu�rios novos no �ltimo m�s
            var newUsersCount = await _context.Users
                .CountAsync(u => u.CreatedAt >= DateTime.Now.AddMonths(-1));

            // Empresas novas no �ltimo m�s
            var newCompaniesCount = await _context.Users
                .CountAsync(u => u.UserType == UserType.Company && u.CreatedAt >= DateTime.Now.AddMonths(-1));

            // Receita de hoje
            var revenueToday = await _context.LeadPurchases
                .Where(p => p.PurchaseDate.Date == DateTime.Today &&
                            (p.PaymentStatus == "Completed" || p.PaymentStatus == "Paid"))
                .SumAsync(p => p.TotalAmount);

            // Dados mensais dos �ltimos 6 meses
            var monthlyData = new List<MonthlyReportData>();

            for (int i = 5; i >= 0; i--)
            {
                var date = DateTime.Now.AddMonths(-i);
                var startDate = new DateTime(date.Year, date.Month, 1);
                var endDate = startDate.AddMonths(1).AddDays(-1);

                var users = await _context.Users
                    .CountAsync(u => u.CreatedAt >= startDate && u.CreatedAt <= endDate);

                var revenue = await _context.LeadPurchases
                    .Where(p => p.PurchaseDate >= startDate &&
                                p.PurchaseDate <= endDate &&
                                (p.PaymentStatus == "Completed" || p.PaymentStatus == "Paid"))
                    .SumAsync(p => p.TotalAmount);

                monthlyData.Add(new MonthlyReportData
                {
                    Month = startDate.ToString("MMM"),
                    Users = users,
                    Revenue = revenue
                });
            }

            // Top empresas que mais gastaram e receberam or�amentos
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

                var quoteCount = await _context.Quotes
                    .CountAsync(q => q.CompanyId == company.Id);

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

            // M�tricas por regi�o baseadas nos dados reais
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
                    ConversionRate = userCount > 0 && companyCount > 0
                        ? Math.Round((double)companyCount / userCount * 100, 1)
                        : 0
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
                ActiveUsers = 0,    // Placeholder - seria necess�rio rastreamento de sess�o
                ActiveSessions = 0, // Placeholder - seria necess�rio rastreamento de sess�o
                QuotesToday = await _context.Quotes.CountAsync(q => q.RequestDate.Date == DateTime.Today),
                RevenueToday = revenueToday,
                MonthlyData = monthlyData,
                TopCompanies = topCompanyData,
                RegionMetrics = regionMetricData
            };

            return View(model);
        }

        // Configura��es do sistema
        public IActionResult Settings()
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

        // Endpoint para notifica��es (usado pelo JavaScript)
        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            try
            {
                var notifications = new List<object>();

                // Verificar empresas pendentes de aprova��o
                var pendingCompanies = await _context.Users
                    .CountAsync(u => u.UserType == UserType.Company && !u.IsActive);

                if (pendingCompanies > 0)
                {
                    notifications.Add(new
                    {
                        type = "info",
                        message = $"H� {pendingCompanies} empresa(s) aguardando aprova��o."
                    });
                }

                // Verificar or�amentos recentes (�ltimas 24h)
                var recentQuotes = await _context.Quotes
                    .CountAsync(q => q.RequestDate >= DateTime.Now.AddHours(-24));

                if (recentQuotes > 5)
                {
                    notifications.Add(new
                    {
                        type = "success",
                        message = $"{recentQuotes} novos or�amentos nas �ltimas 24 horas."
                    });
                }

                // Verificar poss�veis problemas com usu�rios inativos h� muito tempo
                var staleUsers = await _context.Users
                    .CountAsync(u => !u.IsActive && u.CreatedAt <= DateTime.Now.AddDays(-30));

                if (staleUsers > 10)
                {
                    notifications.Add(new
                    {
                        type = "warning",
                        message = $"{staleUsers} usu�rios inativos h� mais de 30 dias."
                    });
                }

                return Json(notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar notifica��es do admin");
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
