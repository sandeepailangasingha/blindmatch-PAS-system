using BlindMatchPAS.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using BlindMatchPAS.Core.Entities;
using Microsoft.AspNetCore.Mvc;

namespace BlindMatchPAS.Web.Controllers
{
    [Authorize]
    public class MatchController : Controller
    {
        private readonly IBlindMatchService _blindMatchService;
        private readonly UserManager<ApplicationUser> _userManager;

        public MatchController(
            IBlindMatchService blindMatchService,
            UserManager<ApplicationUser> userManager)
        {
            _blindMatchService = blindMatchService;
            _userManager = userManager;
        }

        // Supervisor - Express Interest
        [Authorize(Roles = "Supervisor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExpressInterest(int proposalId)
        {
            var user = await _userManager.GetUserAsync(User);
            await _blindMatchService.ExpressInterestAsync(user!.Id, proposalId);
            return RedirectToAction("MyInterests", "Supervisor");
        }

        // Supervisor - Confirm Match (Triggers Identity Reveal)
        [Authorize(Roles = "Supervisor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmMatch(int matchId)
        {
            var user = await _userManager.GetUserAsync(User);

            try
            {
                await _blindMatchService.ConfirmMatchAsync(user!.Id, matchId);
            }
            catch (InvalidOperationException)
            {
                return NotFound();
            }

            return RedirectToAction("MatchDetails", "Supervisor",
                new { matchId });
        }

        // Supervisor - View Revealed Match
        [Authorize(Roles = "Supervisor")]
        public async Task<IActionResult> RevealedMatch(int matchId)
        {
            var user = await _userManager.GetUserAsync(User);

            var match = await _blindMatchService
                .GetRevealedMatchAsync(user!.Id, matchId);

            if (match == null) return NotFound();

            return View(match);
        }

        // Student - View Revealed Supervisor
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> StudentReveal(int proposalId)
        {
            var user = await _userManager.GetUserAsync(User);

            var match = await _blindMatchService
                .GetStudentMatchAsync(user!.Id, proposalId);

            if (match == null)
                return RedirectToAction("Index", "Student");

            return View(match);
        }
    }
}