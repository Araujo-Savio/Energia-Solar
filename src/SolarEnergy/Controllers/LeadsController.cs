using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SolarEnergy.Models;
using SolarEnergy.Services;
using SolarEnergy.ViewModels;
using System.Globalization;

namespace SolarEnergy.Controllers
{
    [Authorize]
    public class LeadsController : Controller
    {
        private readonly ILeadService _leadService;
        private readonly UserManager<ApplicationUser> _userManager;

        public LeadsController(ILeadService leadService, UserManager<ApplicationUser> userManager)
        {
            _leadService = leadService;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Purchase()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.UserType != UserType.Company)
            {
                return Forbid();
            }

            // Add UserType to ViewData for navbar display
            ViewData["UserType"] = user.UserType;

            var balance = await _leadService.GetCompanyLeadBalanceAsync(user.Id);
            var purchaseHistory = await _leadService.GetPurchaseHistoryAsync(user.Id);

            var model = new PurchaseLeadsViewModel
            {
                Balance = new LeadBalanceViewModel
                {
                    AvailableLeads = balance.AvailableLeads,
                    ConsumedLeads = balance.ConsumedLeads,
                    TotalPurchasedLeads = balance.TotalPurchasedLeads,
                    LastPurchaseDate = purchaseHistory.FirstOrDefault()?.PurchaseDate,
                    TotalSpent = purchaseHistory.Sum(p => p.TotalAmount)
                },
                PackageOptions = GetPackageOptions(),
                RecentPurchases = purchaseHistory.Take(5).Select(p => new LeadPurchaseHistoryViewModel
                {
                    Id = p.Id,
                    LeadQuantity = p.LeadQuantity,
                    UnitPrice = p.UnitPrice,
                    DiscountPercentage = p.DiscountPercentage,
                    TotalAmount = p.TotalAmount,
                    PurchaseDate = p.PurchaseDate,
                    PaymentStatus = p.PaymentStatus,
                    TransactionId = p.TransactionId
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Purchase(LeadPackageType packageType)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.UserType != UserType.Company)
            {
                return Forbid();
            }

            // Add UserType to ViewData for navbar display
            ViewData["UserType"] = user.UserType;

            try
            {
                var purchase = await _leadService.PurchaseLeadsAsync(user.Id, packageType);
                
                TempData["Success"] = $"Parabéns! Você comprou {purchase.LeadQuantity} leads com sucesso! " +
                                    $"Transação: {purchase.TransactionId}";
                
                return RedirectToAction(nameof(Purchase));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Erro ao processar a compra: {ex.Message}";
                return RedirectToAction(nameof(Purchase));
            }
        }

        [HttpGet]
        public async Task<IActionResult> Management()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.UserType != UserType.Company)
            {
                return Forbid();
            }

            // Add UserType to ViewData for navbar display
            ViewData["UserType"] = user.UserType;

            var balance = await _leadService.GetCompanyLeadBalanceAsync(user.Id);
            var purchaseHistory = await _leadService.GetPurchaseHistoryAsync(user.Id);
            var consumptionHistory = await _leadService.GetConsumptionHistoryAsync(user.Id);

            var thisMonth = DateTime.Now.Month;
            var thisYear = DateTime.Now.Year;

            var model = new LeadManagementViewModel
            {
                Balance = new LeadBalanceViewModel
                {
                    AvailableLeads = balance.AvailableLeads,
                    ConsumedLeads = balance.ConsumedLeads,
                    TotalPurchasedLeads = balance.TotalPurchasedLeads,
                    LastPurchaseDate = purchaseHistory.FirstOrDefault()?.PurchaseDate,
                    TotalSpent = purchaseHistory.Sum(p => p.TotalAmount)
                },
                PurchaseHistory = purchaseHistory.Select(p => new LeadPurchaseHistoryViewModel
                {
                    Id = p.Id,
                    LeadQuantity = p.LeadQuantity,
                    UnitPrice = p.UnitPrice,
                    DiscountPercentage = p.DiscountPercentage,
                    TotalAmount = p.TotalAmount,
                    PurchaseDate = p.PurchaseDate,
                    PaymentStatus = p.PaymentStatus,
                    TransactionId = p.TransactionId
                }).ToList(),
                ConsumptionHistory = consumptionHistory.Select(c => new LeadConsumptionViewModel
                {
                    Id = c.Id,
                    QuoteId = c.QuoteId,
                    ClientName = c.Quote.Client.FullName,
                    ServiceType = c.Quote.ServiceType,
                    MonthlyConsumptionKwh = c.Quote.MonthlyConsumptionKwh,
                    ConsumedAt = c.ConsumedAt,
                    QuoteRequestDate = c.Quote.RequestDate
                }).ToList(),
                TotalInvestment = purchaseHistory.Sum(p => p.TotalAmount),
                AverageLeadCost = purchaseHistory.Any() ? 
                    purchaseHistory.Sum(p => p.TotalAmount) / purchaseHistory.Sum(p => p.LeadQuantity) : 0,
                LeadsThisMonth = purchaseHistory
                    .Where(p => p.PurchaseDate.Month == thisMonth && p.PurchaseDate.Year == thisYear)
                    .Sum(p => p.LeadQuantity),
                ConsumedThisMonth = consumptionHistory
                    .Count(c => c.ConsumedAt.Month == thisMonth && c.ConsumedAt.Year == thisYear)
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConsumeLead(int quoteId)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null || user.UserType != UserType.Company)
                {
                    return Json(new { 
                        success = false, 
                        message = "Acesso não autorizado",
                        code = "UNAUTHORIZED"
                    });
                }

                // Quick validation - check if already has access first
                var hasAccess = await _leadService.HasAccessToLeadAsync(user.Id, quoteId);
                if (hasAccess)
                {
                    return Json(new { 
                        success = true, 
                        message = "Você já tem acesso a este lead",
                        hasAccess = true,
                        alreadyUnlocked = true
                    });
                }

                // Fast balance check
                var balance = await _leadService.GetCompanyLeadBalanceAsync(user.Id);
                if (balance.AvailableLeads <= 0)
                {
                    return Json(new { 
                        success = false, 
                        message = "Saldo insuficiente. Compre mais leads para acessar este cliente.",
                        hasAccess = false,
                        needsPurchase = true,
                        availableLeads = 0
                    });
                }

                // Consume the lead
                var consumed = await _leadService.ConsumeLeadAsync(user.Id, quoteId);
                if (consumed)
                {
                    // Get updated balance
                    var newBalance = await _leadService.GetCompanyLeadBalanceAsync(user.Id);
                    
                    return Json(new { 
                        success = true, 
                        message = "Lead desbloqueado com sucesso! Agora você tem acesso aos dados completos do cliente.",
                        hasAccess = true,
                        newBalance = newBalance.AvailableLeads,
                        consumedSuccessfully = true
                    });
                }
                else
                {
                    return Json(new { 
                        success = false, 
                        message = "Erro ao processar o desbloqueio. Tente novamente em alguns instantes.",
                        hasAccess = false,
                        technicalError = true
                    });
                }
            }
            catch (Exception ex)
            {
                // Log the error (you can implement proper logging here)
                System.Diagnostics.Debug.WriteLine($"Error consuming lead: {ex.Message}");
                
                return Json(new { 
                    success = false, 
                    message = "Erro interno do servidor. Nossa equipe foi notificada.",
                    hasAccess = false,
                    serverError = true
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetLeadAccess(int quoteId)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null || user.UserType != UserType.Company)
                {
                    return Json(new { hasAccess = false, availableLeads = 0, error = "Unauthorized" });
                }

                // Parallel execution for better performance
                var hasAccessTask = _leadService.HasAccessToLeadAsync(user.Id, quoteId);
                var balanceTask = _leadService.GetCompanyLeadBalanceAsync(user.Id);

                await Task.WhenAll(hasAccessTask, balanceTask);

                var hasAccess = await hasAccessTask;
                var balance = await balanceTask;

                return Json(new { 
                    hasAccess = hasAccess, 
                    availableLeads = balance.AvailableLeads,
                    canPurchase = balance.AvailableLeads > 0,
                    totalConsumed = balance.ConsumedLeads
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting lead access: {ex.Message}");
                return Json(new { hasAccess = false, availableLeads = 0, error = "ServerError" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetLeadBalance()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null || user.UserType != UserType.Company)
                {
                    return Json(new { availableLeads = 0, consumedLeads = 0, error = "Unauthorized" });
                }

                var balance = await _leadService.GetCompanyLeadBalanceAsync(user.Id);
                return Json(new { 
                    availableLeads = balance.AvailableLeads, 
                    consumedLeads = balance.ConsumedLeads,
                    totalPurchased = balance.TotalPurchasedLeads,
                    lastUpdate = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting lead balance: {ex.Message}");
                return Json(new { availableLeads = 0, consumedLeads = 0, error = "ServerError" });
            }
        }

        private List<LeadPackageOption> GetPackageOptions()
        {
            var packages = new List<LeadPackageOption>();
            const decimal basePrice = 14.99m;

            foreach (LeadPackageType packageType in Enum.GetValues<LeadPackageType>())
            {
                var quantity = (int)packageType;
                var price = _leadService.CalculatePrice(packageType, out decimal discount);
                var subtotal = quantity * basePrice;
                var savings = subtotal - price;

                // Calcular preço por lead após desconto
                var finalPricePerLead = price / quantity;

                var package = new LeadPackageOption
                {
                    Type = packageType,
                    Name = GetPackageName(packageType),
                    Description = GetPackageDescription(packageType),
                    Quantity = quantity,
                    UnitPrice = basePrice, // Preço base por lead
                    DiscountPercentage = discount,
                    TotalPrice = price, // Preço final do pacote
                    Savings = savings,
                    IsPopular = packageType == LeadPackageType.Pack50,
                    Badge = GetPackageBadge(packageType)
                };

                packages.Add(package);
            }

            return packages;
        }

        private static string GetPackageName(LeadPackageType packageType) => packageType switch
        {
            LeadPackageType.Single => "1 Lead",
            LeadPackageType.Pack20 => "Pacote Básico",
            LeadPackageType.Pack50 => "Pacote Profissional",
            LeadPackageType.Pack100 => "Pacote Empresarial",
            _ => "Pacote"
        };

        private static string GetPackageDescription(LeadPackageType packageType) => packageType switch
        {
            LeadPackageType.Single => "Perfeito para testar a plataforma",
            LeadPackageType.Pack20 => "20 Leads com 5% de desconto",
            LeadPackageType.Pack50 => "50 Leads com 10% de desconto - Mais vendido!",
            LeadPackageType.Pack100 => "100 Leads com 15% de desconto - Melhor custo-benefício",
            _ => "Pacote de leads"
        };

        private static string GetPackageBadge(LeadPackageType packageType) => packageType switch
        {
            LeadPackageType.Single => "",
            LeadPackageType.Pack20 => "Economia 5%",
            LeadPackageType.Pack50 => "Mais Popular",
            LeadPackageType.Pack100 => "Melhor Valor",
            _ => ""
        };
    }
}