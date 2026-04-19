using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using BlindMatchPAS.Core.Entities;

namespace BlindMatchPAS.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var roles = await _userManager.GetRolesAsync(user);

            if (roles.Contains("Admin"))
                return RedirectToAction("Index", "Admin");
            else if (roles.Contains("Supervisor"))
                return RedirectToAction("Index", "Supervisor");
            else
                return RedirectToAction("Index", "Student");
        }

        public IActionResult Privacy() => View();
    }
}