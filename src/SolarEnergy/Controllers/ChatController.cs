using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SolarEnergy.Data;
using SolarEnergy.Models;
using SolarEnergy.ViewModels;

namespace SolarEnergy.Controllers
{
    [Authorize]
    public class ChatController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ChatController> _logger;

        public ChatController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<ChatController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int quoteId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var quote = await _context.Quotes
                .Include(q => q.Client)
                .Include(q => q.Company)
                .Include(q => q.Messages)
                    .ThenInclude(m => m.Sender)
                .Include(q => q.Proposals)
                .FirstOrDefaultAsync(q => q.QuoteId == quoteId);

            if (quote == null)
            {
                return NotFound();
            }

            // Verificar permissões - apenas cliente ou empresa envolvidos podem acessar o chat
            if (currentUser.UserType == UserType.Client && quote.ClientId != currentUser.Id)
            {
                return Forbid();
            }
            else if (currentUser.UserType == UserType.Company && quote.CompanyId != currentUser.Id)
            {
                return Forbid();
            }
            else if (currentUser.UserType == UserType.Administrator)
            {
                // Administrador pode ver todos os chats
            }
            else if (currentUser.UserType != UserType.Client && currentUser.UserType != UserType.Company && currentUser.UserType != UserType.Administrator)
            {
                return Forbid();
            }

            // Marcar mensagens como lidas
            var unreadMessages = quote.Messages
                .Where(m => m.SenderId != currentUser.Id && !m.ReadDate.HasValue)
                .ToList();

            foreach (var message in unreadMessages)
            {
                message.MarkAsRead();
            }

            if (unreadMessages.Any())
            {
                await _context.SaveChangesAsync();
            }

            var viewModel = new QuoteChatViewModel
            {
                QuoteId = quote.QuoteId,
                CompanyName = quote.Company.CompanyTradeName ?? quote.Company.CompanyLegalName ?? quote.Company.FullName,
                ClientName = quote.Client.FullName,
                CurrentUserType = currentUser.UserType.ToString(),
                Status = quote.Status,
                RequestDate = quote.RequestDate,
                MonthlyConsumptionKwh = quote.MonthlyConsumptionKwh,
                ServiceType = quote.ServiceType,
                InitialMessage = quote.Message,
                CompanyResponseMessage = quote.CompanyResponseMessage,
                CompanyResponseDate = quote.CompanyResponseDate,
                HasProposal = quote.Proposals.Any(),
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
                }).ToList(),
                Messages = quote.Messages.OrderBy(m => m.SentDate).Select(m => new ChatMessageViewModel
                {
                    MessageId = m.MessageId,
                    Message = m.Message,
                    SentDate = m.SentDate,
                    SenderType = m.SenderType.ToString(),
                    SenderName = m.SenderType == MessageSenderType.Client ? quote.Client.FullName : (quote.Company.CompanyTradeName ?? quote.Company.CompanyLegalName ?? quote.Company.FullName),
                    IsRead = m.IsRead,
                    IsCurrentUser = m.SenderId == currentUser.Id
                }).ToList()
            };

            await SetUserTypeInViewData();
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMessage(SendChatMessageViewModel model)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Mensagem inválida. Verifique se preencheu corretamente.";
                return RedirectToAction("Index", new { quoteId = model.QuoteId });
            }

            var quote = await _context.Quotes
                .FirstOrDefaultAsync(q => q.QuoteId == model.QuoteId);

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

            // Determinar o tipo de remetente
            MessageSenderType senderType;
            if (currentUser.UserType == UserType.Client)
            {
                senderType = MessageSenderType.Client;
            }
            else if (currentUser.UserType == UserType.Company)
            {
                senderType = MessageSenderType.Company;
            }
            else
            {
                TempData["ErrorMessage"] = "Tipo de usuário inválido para envio de mensagens.";
                return RedirectToAction("Index", new { quoteId = model.QuoteId });
            }

            var message = new QuoteMessage
            {
                QuoteId = model.QuoteId,
                SenderId = currentUser.Id,
                Message = model.Message.Trim(),
                SentDate = DateTime.Now,
                SenderType = senderType
            };

            _context.QuoteMessages.Add(message);
            
            // Atualizar status do orçamento se necessário
            if (quote.Status == "Pendente" && senderType == MessageSenderType.Company)
            {
                quote.Status = "Em Análise";
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Message sent by user {UserId} for quote {QuoteId}", currentUser.Id, model.QuoteId);

            TempData["SuccessMessage"] = "Mensagem enviada com sucesso!";
            return RedirectToAction("Index", new { quoteId = model.QuoteId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsRead(int quoteId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Json(new { success = false, message = "Usuário não autenticado" });
            }

            var unreadMessages = await _context.QuoteMessages
                .Where(m => m.QuoteId == quoteId && m.SenderId != currentUser.Id && !m.ReadDate.HasValue)
                .ToListAsync();

            foreach (var message in unreadMessages)
            {
                message.MarkAsRead();
            }

            if (unreadMessages.Any())
            {
                await _context.SaveChangesAsync();
            }

            return Json(new { success = true, count = unreadMessages.Count });
        }

        [HttpGet]
        public async Task<IActionResult> GetUnreadCount(int quoteId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Json(new { count = 0 });
            }

            var count = await _context.QuoteMessages
                .Where(m => m.QuoteId == quoteId && m.SenderId != currentUser.Id && !m.ReadDate.HasValue)
                .CountAsync();

            return Json(new { count = count });
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