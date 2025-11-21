using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SolarEnergy.Models;
using System.Collections.Generic;

namespace SolarEnergy.Controllers
{
    public class AuthController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<AuthController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(
                model.Email, 
                model.Password, 
                model.RememberMe, 
                lockoutOnFailure: true);

            if (result.Succeeded)
            {
                _logger.LogInformation("User {Email} logged in successfully.", model.Email);
                var loggedUser = await _userManager.FindByEmailAsync(model.Email);
                return await RedirectAfterLoginAsync(returnUrl, loggedUser);
            }

            if (result.IsLockedOut)
            {
                _logger.LogWarning("User account {Email} locked out.", model.Email);
                ModelState.AddModelError(string.Empty, "Sua conta foi temporariamente bloqueada devido a muitas tentativas de login. Tente novamente em alguns minutos.");
                return View(model);
            }

            if (result.IsNotAllowed)
            {
                ModelState.AddModelError(string.Empty, "Login não permitido. Verifique se sua conta foi confirmada.");
                return View(model);
            }

            ModelState.AddModelError(string.Empty, "Email ou senha incorretos. Verifique seus dados e tente novamente.");
            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Custom validation based on user type
            if (model.UserType == UserType.Company)
            {
                if (string.IsNullOrEmpty(model.CNPJ))
                {
                    ModelState.AddModelError("CNPJ", "CNPJ é obrigatório para empresas.");
                    return View(model);
                }

                // Check if CNPJ already exists
                var existingUserWithCNPJ = await _userManager.Users
                    .FirstOrDefaultAsync(u => u.CNPJ == model.CNPJ);
                if (existingUserWithCNPJ != null)
                {
                    ModelState.AddModelError("CNPJ", "Este CNPJ já está cadastrado em nosso sistema.");
                    return View(model);
                }

                var missingFields = false;
                if (string.IsNullOrWhiteSpace(model.CompanyLegalName))
                {
                    ModelState.AddModelError("CompanyLegalName", "Informe a razão social da empresa.");
                    missingFields = true;
                }
                if (string.IsNullOrWhiteSpace(model.CompanyTradeName))
                {
                    ModelState.AddModelError("CompanyTradeName", "Informe o nome fantasia da empresa.");
                    missingFields = true;
                }
                if (string.IsNullOrWhiteSpace(model.CompanyPhone))
                {
                    ModelState.AddModelError("CompanyPhone", "Informe um telefone comercial para contato.");
                    missingFields = true;
                }
                if (string.IsNullOrWhiteSpace(model.ResponsibleName))
                {
                    ModelState.AddModelError("ResponsibleName", "Informe o responsável comercial pela empresa.");
                    missingFields = true;
                }
                if (string.IsNullOrWhiteSpace(model.ResponsibleCPF))
                {
                    ModelState.AddModelError("ResponsibleCPF", "Informe o CPF do responsável comercial.");
                    missingFields = true;
                }

                if (missingFields)
                {
                    return View(model);
                }

                if (string.IsNullOrWhiteSpace(model.FullName))
                {
                    model.FullName = model.ResponsibleName ?? model.CompanyLegalName ?? string.Empty;
                }

                model.Phone = model.CompanyPhone;
                // Clear CPF if filled by mistake
                model.CPF = null;
            }
            else
            {
                if (string.IsNullOrEmpty(model.CPF))
                {
                    ModelState.AddModelError("CPF", "CPF é obrigatório para consumidores.");
                    return View(model);
                }

                // Check if CPF already exists
                var existingUserWithCPF = await _userManager.Users
                    .FirstOrDefaultAsync(u => u.CPF == model.CPF);
                if (existingUserWithCPF != null)
                {
                    ModelState.AddModelError("CPF", "Este CPF já está cadastrado em nosso sistema.");
                    return View(model);
                }

                // Clear CNPJ if filled by mistake
                model.CNPJ = null;
                model.CompanyLegalName = null;
                model.CompanyTradeName = null;
                model.CompanyPhone = null;
                model.CompanyWebsite = null;
                model.CompanyDescription = null;
                model.ResponsibleName = null;
                model.ResponsibleCPF = null;
                model.StateRegistration = null;
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                CPF = model.CPF,
                CNPJ = model.CNPJ,
                Phone = model.Phone,
                PhoneNumber = model.Phone,
                CompanyLegalName = model.CompanyLegalName,
                CompanyTradeName = model.CompanyTradeName,
                StateRegistration = model.StateRegistration,
                CompanyPhone = model.CompanyPhone,
                CompanyWebsite = model.CompanyWebsite,
                CompanyDescription = model.CompanyDescription,
                ServiceType = model.ServiceType,
                ResponsibleName = model.ResponsibleName,
                ResponsibleCPF = model.ResponsibleCPF,
                UserType = model.UserType,
                CreatedAt = DateTime.Now,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                _logger.LogInformation("User {Email} created successfully.", model.Email);
                
                // Add role based on user type
                var rolesToAdd = model.UserType switch
                {
                    UserType.Company => new List<string> { "Company" },
                    UserType.Administrator => new List<string> { "Admin", "Administrator" },
                    _ => new List<string> { "Client" }
                };

                // Check if role exists before adding
                foreach (var roleName in rolesToAdd)
                {
                    if (await _roleManager.RoleExistsAsync(roleName))
                    {
                        await _userManager.AddToRoleAsync(user, roleName);
                    }
                }

                // Show success message
                TempData["SuccessMessage"] = "Conta criada com sucesso! Bem-vindo à Solar Energy.";
                
                await _signInManager.SignInAsync(user, isPersistent: false);
                return await RedirectAfterLoginAsync(null, user);
            }

            // Translate Identity errors to more friendly messages
            foreach (var error in result.Errors)
            {
                string friendlyMessage = error.Code switch
                {
                    "DuplicateEmail" => "Este email já está cadastrado. Tente fazer login ou use outro email.",
                    "InvalidEmail" => "Por favor, insira um email válido.",
                    "PasswordTooShort" => "A senha deve ter pelo menos 8 caracteres.",
                    "PasswordRequiresDigit" => "A senha deve conter pelo menos um número.",
                    "PasswordRequiresLower" => "A senha deve conter pelo menos uma letra minúscula.",
                    "PasswordRequiresUpper" => "A senha deve conter pelo menos uma letra maiúscula.",
                    "PasswordRequiresNonAlphanumeric" => "A senha deve conter pelo menos um caractere especial.",
                    _ => error.Description
                };
                
                ModelState.AddModelError(nameof(RegisterViewModel.Password), friendlyMessage);
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            TempData["InfoMessage"] = "Você foi desconectado com sucesso.";
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        private Task<IActionResult> RedirectAfterLoginAsync(string? returnUrl, ApplicationUser? user)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Task.FromResult<IActionResult>(Redirect(returnUrl));
            }

            if (user == null)
            {
                return Task.FromResult<IActionResult>(RedirectToAction("Index", "Home"));
            }

            IActionResult destination = user.UserType switch
            {
                UserType.Company => RedirectToAction("Leads", "Home"), // Empresas vão direto para Leads
                UserType.Administrator => RedirectToAction("AdminDashboard", "Home"), // Admin mantém AdminDashboard
                _ => RedirectToAction("SearchCompanies", "Home")
            };

            return Task.FromResult(destination);
        }
    }
}