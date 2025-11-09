using System;
using System.IO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SolarEnergy.Models;
using SolarEnergy.ViewModels;

namespace SolarEnergy.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<ProfileController> _logger;

        public ProfileController(
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment environment,
            ILogger<ProfileController> logger)
        {
            _userManager = userManager;
            _environment = environment;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null)
            {
                _logger.LogWarning("Authenticated user not found while accessing profile page.");
                return RedirectToAction("Login", "Auth");
            }

            await SetUserTypeInViewData();

            // Se for empresa, direcionar para view específica
            if (user.UserType == UserType.Company)
            {
                var companyViewModel = CreateCompanyViewModelFromUser(user);
                return View("CompanyProfile", companyViewModel);
            }

            var viewModel = CreateViewModelFromUser(user);
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(UserProfileViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null)
            {
                _logger.LogWarning("Authenticated user not found while updating profile.");
                TempData["ErrorMessage"] = "Não foi possível localizar o usuário logado.";
                return RedirectToAction("Login", "Auth");
            }

            model.Email = model.Email?.Trim() ?? string.Empty;
            model.Phone = string.IsNullOrWhiteSpace(model.Phone) ? null : model.Phone.Trim();
            model.Location = string.IsNullOrWhiteSpace(model.Location) ? null : model.Location.Trim();

            ModelState.Clear();
            if (!TryValidateModel(model))
            {
                PopulateReadOnlyFields(model, user);
                await SetUserTypeInViewData();
                return View("Index", model);
            }

            if (!string.Equals(user.Email, model.Email, StringComparison.OrdinalIgnoreCase))
            {
                var setEmailResult = await _userManager.SetEmailAsync(user, model.Email);
                if (!setEmailResult.Succeeded)
                {
                    AddErrors(setEmailResult);
                    PopulateReadOnlyFields(model, user);
                    await SetUserTypeInViewData();
                    return View("Index", model);
                }

                var setUserNameResult = await _userManager.SetUserNameAsync(user, model.Email);
                if (!setUserNameResult.Succeeded)
                {
                    AddErrors(setUserNameResult);
                    PopulateReadOnlyFields(model, user);
                    await SetUserTypeInViewData();
                    return View("Index", model);
                }
            }

            user.PhoneNumber = model.Phone;
            user.Phone = model.Phone;
            user.Location = model.Location;

            if (model.ProfileImageFile is not null && model.ProfileImageFile.Length > 0)
            {
                var imagePath = await ProcessProfileImage(model.ProfileImageFile, user.ProfileImagePath);
                if (imagePath != null)
                {
                    user.ProfileImagePath = imagePath;
                }
                else
                {
                    ModelState.AddModelError(nameof(model.ProfileImageFile), "Erro ao processar a imagem.");
                    PopulateReadOnlyFields(model, user);
                    await SetUserTypeInViewData();
                    return View("Index", model);
                }
            }

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                AddErrors(updateResult);
                PopulateReadOnlyFields(model, user);
                model.ProfileImagePath = user.ProfileImagePath;
                await SetUserTypeInViewData();
                return View("Index", model);
            }

            TempData["SuccessMessage"] = "Perfil atualizado com sucesso!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCompany(CompanyProfileViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null || user.UserType != UserType.Company)
            {
                _logger.LogWarning("Authenticated company user not found while updating profile.");
                TempData["ErrorMessage"] = "Não foi possível localizar o usuário logado.";
                return RedirectToAction("Login", "Auth");
            }

            // Limpar dados
            model.Email = model.Email?.Trim() ?? string.Empty;
            model.Phone = string.IsNullOrWhiteSpace(model.Phone) ? null : model.Phone.Trim();
            model.CompanyPhone = string.IsNullOrWhiteSpace(model.CompanyPhone) ? null : model.CompanyPhone.Trim();
            model.Location = string.IsNullOrWhiteSpace(model.Location) ? null : model.Location.Trim();
            model.CompanyWebsite = string.IsNullOrWhiteSpace(model.CompanyWebsite) ? null : model.CompanyWebsite.Trim();
            model.CompanyDescription = string.IsNullOrWhiteSpace(model.CompanyDescription) ? null : model.CompanyDescription.Trim();
            model.CompanyLegalName = string.IsNullOrWhiteSpace(model.CompanyLegalName) ? null : model.CompanyLegalName.Trim();
            model.CompanyTradeName = string.IsNullOrWhiteSpace(model.CompanyTradeName) ? null : model.CompanyTradeName.Trim();

            ModelState.Clear();
            if (!TryValidateModel(model))
            {
                PopulateCompanyReadOnlyFields(model, user);
                await SetUserTypeInViewData();
                return View("CompanyProfile", model);
            }

            // Atualizar email se necessário
            if (!string.Equals(user.Email, model.Email, StringComparison.OrdinalIgnoreCase))
            {
                var setEmailResult = await _userManager.SetEmailAsync(user, model.Email);
                if (!setEmailResult.Succeeded)
                {
                    AddErrors(setEmailResult);
                    PopulateCompanyReadOnlyFields(model, user);
                    await SetUserTypeInViewData();
                    return View("CompanyProfile", model);
                }

                var setUserNameResult = await _userManager.SetUserNameAsync(user, model.Email);
                if (!setUserNameResult.Succeeded)
                {
                    AddErrors(setUserNameResult);
                    PopulateCompanyReadOnlyFields(model, user);
                    await SetUserTypeInViewData();
                    return View("CompanyProfile", model);
                }
            }

            // Atualizar dados da empresa
            user.FullName = model.FullName;
            user.PhoneNumber = model.Phone;
            user.Phone = model.Phone;
            user.CompanyPhone = model.CompanyPhone;
            user.Location = model.Location;
            user.CompanyWebsite = model.CompanyWebsite;
            user.CompanyDescription = model.CompanyDescription;
            user.CompanyLegalName = model.CompanyLegalName;
            user.CompanyTradeName = model.CompanyTradeName;
            user.ServiceType = model.ServiceType;
            user.IsActive = model.IsActive;

            // Processar imagem se fornecida
            if (model.ProfileImageFile is not null && model.ProfileImageFile.Length > 0)
            {
                var imagePath = await ProcessProfileImage(model.ProfileImageFile, user.ProfileImagePath);
                if (imagePath != null)
                {
                    user.ProfileImagePath = imagePath;
                }
                else
                {
                    ModelState.AddModelError(nameof(model.ProfileImageFile), "Erro ao processar a imagem.");
                    PopulateCompanyReadOnlyFields(model, user);
                    await SetUserTypeInViewData();
                    return View("CompanyProfile", model);
                }
            }

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                AddErrors(updateResult);
                PopulateCompanyReadOnlyFields(model, user);
                model.ProfileImagePath = user.ProfileImagePath;
                await SetUserTypeInViewData();
                return View("CompanyProfile", model);
            }

            TempData["SuccessMessage"] = "Perfil da empresa atualizado com sucesso!";
            return RedirectToAction(nameof(Index));
        }

        private async Task<string?> ProcessProfileImage(IFormFile imageFile, string? currentPath)
        {
            if (!imageFile.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "profiles");
            Directory.CreateDirectory(uploadsFolder);

            var extension = Path.GetExtension(imageFile.FileName);
            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            await using (var stream = System.IO.File.Create(filePath))
            {
                await imageFile.CopyToAsync(stream);
            }

            // Remover imagem anterior se existir
            if (!string.IsNullOrWhiteSpace(currentPath) &&
                !string.Equals(currentPath, "/images/default-profile.svg", StringComparison.OrdinalIgnoreCase))
            {
                var existingPath = Path.Combine(_environment.WebRootPath, currentPath.TrimStart('/')
                    .Replace('/', Path.DirectorySeparatorChar));
                if (System.IO.File.Exists(existingPath))
                {
                    System.IO.File.Delete(existingPath);
                }
            }

            return $"/uploads/profiles/{fileName}";
        }

        private static void PopulateReadOnlyFields(UserProfileViewModel model, ApplicationUser user)
        {
            model.Id = user.Id;
            model.FullName = user.FullName;
            model.UserType = user.UserType;
            model.IsActive = user.IsActive;
            model.CreatedAt = user.CreatedAt;
            model.DateOfBirth = user.DateOfBirth;
            model.CPF = user.CPF;
            model.CNPJ = user.CNPJ;
            model.ProfileImagePath = user.ProfileImagePath;
        }

        private static void PopulateCompanyReadOnlyFields(CompanyProfileViewModel model, ApplicationUser user)
        {
            model.Id = user.Id;
            model.UserType = user.UserType;
            model.CreatedAt = user.CreatedAt;
            model.CPF = user.CPF;
            model.CNPJ = user.CNPJ;
            model.StateRegistration = user.StateRegistration;
            model.ResponsibleName = user.ResponsibleName;
            model.ResponsibleCPF = user.ResponsibleCPF;
            model.ProfileImagePath = user.ProfileImagePath;
        }

        private static UserProfileViewModel CreateViewModelFromUser(ApplicationUser user) => new()
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            Phone = user.Phone,
            Location = user.Location,
            UserType = user.UserType,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            DateOfBirth = user.DateOfBirth,
            CPF = user.CPF,
            CNPJ = user.CNPJ,
            ProfileImagePath = user.ProfileImagePath
        };

        private static CompanyProfileViewModel CreateCompanyViewModelFromUser(ApplicationUser user) => new()
        {
            Id = user.Id,
            FullName = user.FullName,
            CompanyLegalName = user.CompanyLegalName,
            CompanyTradeName = user.CompanyTradeName,
            Email = user.Email ?? string.Empty,
            Phone = user.Phone,
            CompanyPhone = user.CompanyPhone,
            CompanyWebsite = user.CompanyWebsite,
            CompanyDescription = user.CompanyDescription,
            ServiceType = user.ServiceType,
            Location = user.Location,
            IsActive = user.IsActive,
            ProfileImagePath = user.ProfileImagePath,
            CPF = user.CPF,
            CNPJ = user.CNPJ,
            StateRegistration = user.StateRegistration,
            ResponsibleName = user.ResponsibleName,
            ResponsibleCPF = user.ResponsibleCPF,
            UserType = user.UserType,
            CreatedAt = user.CreatedAt
        };

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
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
