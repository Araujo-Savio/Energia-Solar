using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SolarEnergy.Models;
using SolarEnergy.ViewModels;
using System;

namespace SolarEnergy.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<UsersController> _logger;

        public UsersController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<UsersController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            var roles = await _roleManager.Roles
                .Select(r => r.Name ?? string.Empty)
                .ToListAsync();

            var model = new UserEditViewModel
            {
                Id = user.Id!,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                SelectedRole = currentRoles.FirstOrDefault(),
                AvailableRoles = roles
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.AvailableRoles = await _roleManager.Roles
                    .Select(r => r.Name ?? string.Empty)
                    .ToListAsync();
                return View(model);
            }

            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null)
            {
                return NotFound();
            }

            user.FullName = model.FullName;
            user.Email = model.Email;
            user.UserName = model.Email;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                foreach (var error in updateResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                model.AvailableRoles = await _roleManager.Roles
                    .Select(r => r.Name ?? string.Empty)
                    .ToListAsync();
                return View(model);
            }

            var existingRoles = await _userManager.GetRolesAsync(user);
            if (existingRoles.Any())
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, existingRoles);
                if (!removeResult.Succeeded)
                {
                    foreach (var error in removeResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }

                    model.AvailableRoles = await _roleManager.Roles
                        .Select(r => r.Name ?? string.Empty)
                        .ToListAsync();
                    return View(model);
                }
            }

            if (!string.IsNullOrWhiteSpace(model.SelectedRole))
            {
                if (!await _roleManager.RoleExistsAsync(model.SelectedRole))
                {
                    ModelState.AddModelError(nameof(model.SelectedRole), "A role selecionada não existe.");
                    model.AvailableRoles = await _roleManager.Roles
                        .Select(r => r.Name ?? string.Empty)
                        .ToListAsync();
                    return View(model);
                }

                var addResult = await _userManager.AddToRoleAsync(user, model.SelectedRole);
                if (!addResult.Succeeded)
                {
                    foreach (var error in addResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }

                    model.AvailableRoles = await _roleManager.Roles
                        .Select(r => r.Name ?? string.Empty)
                        .ToListAsync();
                    return View(model);
                }
            }

            TempData["Success"] = "Usuário atualizado com sucesso.";
            _logger.LogInformation("User {UserId} updated by {AdminId}", user.Id, _userManager.GetUserId(User));

            return RedirectToAction("Users", "Admin", new { area = string.Empty });
        }

        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            if (user.Email == User.Identity?.Name)
            {
                TempData["ErrorMessage"] = "Você não pode excluir seu próprio usuário.";
                return RedirectToAction("Users", "Admin", new { area = string.Empty });
            }

            var model = new UserDeleteViewModel
            {
                Id = user.Id!,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty
            };

            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            if (user.Email == User.Identity?.Name)
            {
                TempData["ErrorMessage"] = "Você não pode excluir o próprio usuário logado.";
                return RedirectToAction("Users", "Admin", new { area = string.Empty });
            }

            user.IsDeleted = true;
            user.DeletedAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            TempData["SuccessMessage"] = $"Usuário {user.Email} foi marcado como excluído.";
            _logger.LogInformation("User {UserId} soft deleted by {AdminId}", user.Id, _userManager.GetUserId(User));

            return RedirectToAction("Users", "Admin", new { area = string.Empty });
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Deleted()
        {
            var deletedUsers = _userManager.Users
                .Where(u => u.IsDeleted)
                .OrderByDescending(u => u.DeletedAt)
                .ToList();

            return View(deletedUsers);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null || !user.IsDeleted)
            {
                return NotFound();
            }

            user.IsDeleted = false;
            user.DeletedAt = null;
            await _userManager.UpdateAsync(user);

            TempData["Success"] = $"Usuário {user.Email} foi restaurado com sucesso.";
            return RedirectToAction(nameof(Deleted));
        }
    }
}
