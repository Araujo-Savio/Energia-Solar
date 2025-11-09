using SolarEnergy.Data;
using SolarEnergy.Models;
using Microsoft.EntityFrameworkCore;

namespace SolarEnergy.Services
{
    public interface ILeadService
    {
        Task<CompanyLeadBalance> GetCompanyLeadBalanceAsync(string companyId);
        Task<bool> HasAccessToLeadAsync(string companyId, int quoteId);
        Task<bool> ConsumeLeadAsync(string companyId, int quoteId);
        Task<LeadPurchase> PurchaseLeadsAsync(string companyId, LeadPackageType packageType);
        Task<List<LeadPurchase>> GetPurchaseHistoryAsync(string companyId);
        Task<List<LeadConsumption>> GetConsumptionHistoryAsync(string companyId);
        decimal CalculatePrice(LeadPackageType packageType, out decimal discount);
        string MaskSensitiveData(string data, int visibleChars = 1);
    }

    public class LeadService : ILeadService
    {
        private readonly ApplicationDbContext _context;
        private const decimal BASE_PRICE = 14.99m;

        public LeadService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<CompanyLeadBalance> GetCompanyLeadBalanceAsync(string companyId)
        {
            var balance = await _context.CompanyLeadBalances
                .FirstOrDefaultAsync(b => b.CompanyId == companyId);

            if (balance == null)
            {
                balance = new CompanyLeadBalance
                {
                    CompanyId = companyId,
                    AvailableLeads = 0,
                    ConsumedLeads = 0,
                    TotalPurchasedLeads = 0,
                    CreatedAt = DateTime.Now
                };

                _context.CompanyLeadBalances.Add(balance);
                await _context.SaveChangesAsync();
            }

            return balance;
        }

        public async Task<bool> HasAccessToLeadAsync(string companyId, int quoteId)
        {
            return await _context.LeadConsumptions
                .AnyAsync(c => c.CompanyId == companyId && c.QuoteId == quoteId);
        }

        public async Task<bool> ConsumeLeadAsync(string companyId, int quoteId)
        {
            // Verificar se já consumiu este lead
            if (await HasAccessToLeadAsync(companyId, quoteId))
            {
                return true; // Já tem acesso
            }

            var balance = await GetCompanyLeadBalanceAsync(companyId);

            if (balance.AvailableLeads <= 0)
            {
                return false; // Sem leads disponíveis
            }

            // Consumir o lead
            balance.AvailableLeads--;
            balance.ConsumedLeads++;
            balance.UpdatedAt = DateTime.Now;

            var consumption = new LeadConsumption
            {
                CompanyId = companyId,
                QuoteId = quoteId,
                ConsumedAt = DateTime.Now
            };

            _context.LeadConsumptions.Add(consumption);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<LeadPurchase> PurchaseLeadsAsync(string companyId, LeadPackageType packageType)
        {
            var balance = await GetCompanyLeadBalanceAsync(companyId);
            var price = CalculatePrice(packageType, out decimal discountPercentage);

            var purchase = new LeadPurchase
            {
                CompanyId = companyId,
                LeadQuantity = (int)packageType,
                UnitPrice = BASE_PRICE,
                DiscountPercentage = discountPercentage,
                TotalAmount = price,
                PurchaseDate = DateTime.Now,
                PaymentStatus = "Completed", // Para desenvolvimento
                TransactionId = Guid.NewGuid().ToString("N")[..8].ToUpper(),
                Notes = "Compra em desenvolvimento - Créditos adicionados automaticamente"
            };

            _context.LeadPurchases.Add(purchase);

            // Atualizar saldo
            balance.AvailableLeads += (int)packageType;
            balance.TotalPurchasedLeads += (int)packageType;
            balance.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return purchase;
        }

        public async Task<List<LeadPurchase>> GetPurchaseHistoryAsync(string companyId)
        {
            return await _context.LeadPurchases
                .Where(p => p.CompanyId == companyId)
                .OrderByDescending(p => p.PurchaseDate)
                .ToListAsync();
        }

        public async Task<List<LeadConsumption>> GetConsumptionHistoryAsync(string companyId)
        {
            return await _context.LeadConsumptions
                .Include(c => c.Quote)
                    .ThenInclude(q => q.Client)
                .Where(c => c.CompanyId == companyId)
                .OrderByDescending(c => c.ConsumedAt)
                .ToListAsync();
        }

        public decimal CalculatePrice(LeadPackageType packageType, out decimal discount)
        {
            int quantity = (int)packageType;
            decimal subtotal = quantity * BASE_PRICE;
            
            discount = packageType switch
            {
                LeadPackageType.Pack20 => 5.0m,
                LeadPackageType.Pack50 => 10.0m,
                LeadPackageType.Pack100 => 15.0m,
                _ => 0m
            };

            decimal discountAmount = subtotal * (discount / 100);
            return subtotal - discountAmount;
        }

        public string MaskSensitiveData(string data, int visibleChars = 1)
        {
            if (string.IsNullOrEmpty(data))
                return data;

            if (data.Length <= visibleChars)
                return data;

            string visible = data.Substring(0, visibleChars);
            string masked = new string('*', data.Length - visibleChars);
            return visible + masked;
        }
    }
}