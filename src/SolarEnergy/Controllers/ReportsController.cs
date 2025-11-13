using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using SolarEnergy.Models;

namespace SolarEnergy.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ReportsController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> MonthlyReport()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null || user.UserType != UserType.Company)
            {
                TempData["ErrorMessage"] = "Relatórios disponíveis apenas para empresas.";
                return RedirectToAction("SearchCompanies", "Home");
            }

            ViewData["CompanyName"] = user.CompanyTradeName ?? user.CompanyLegalName ?? user.FullName;
            ViewData["UserType"] = user.UserType;

            return View();
        }
    }
}