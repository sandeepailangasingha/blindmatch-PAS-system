using BlindMatchPAS.Core.Entities;
using BlindMatchPAS.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlindMatchPAS.Web.Controllers
{
    [Authorize(Roles = "Supervisor")]
    public class SupervisorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public SupervisorController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Blind Dashboard - Anonymous proposals
        public async Task<IActionResult> Index(int? researchAreaId)
        {
            var user = await _userManager.GetUserAsync(User);

            // Anonymity Filter - Student identity hidden
            var query = _context.ProjectProposals
                .Include(p => p.ResearchArea)
                .Where(p => p.Status == "Pending" ||
                            p.Status == "Under Review")
                .Select(p => new
                {
                    p.Id,
                    p.Title,
                    p.Abstract,
                    p.TechnicalStack,
                    p.Status,
                    ResearchAreaName = p.ResearchArea!.Name,
                    p.ResearchAreaId
                    // StudentId deliberately excluded - Blind Match!
                });

            if (researchAreaId.HasValue)
                query = query.Where(p => p.ResearchAreaId == researchAreaId);

            var proposals = await query.ToListAsync();

            ViewBag.ResearchAreas = await _context.ResearchAreas.ToListAsync();
            ViewBag.SelectedArea = researchAreaId;

            return View(proposals);
        }

        // Express Interest
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExpressInterest(int proposalId)
        {
            var user = await _userManager.GetUserAsync(User);

            // Check if already interested
            var existing = await _context.MatchRecords
                .FirstOrDefaultAsync(m => m.ProjectProposalId == proposalId
                    && m.SupervisorId == user!.Id);

            if (existing == null)
            {
                var match = new MatchRecord
                {
                    ProjectProposalId = proposalId,
                    SupervisorId = user!.Id,
                    IsConfirmed = false
                };
                _context.MatchRecords.Add(match);

                // Update status to Under Review
                var proposal = await _context.ProjectProposals
                    .FindAsync(proposalId);
                if (proposal != null)
                    proposal.Status = "Under Review";

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(MyInterests));
        }

        // My Interests Dashboard
        public async Task<IActionResult> MyInterests()
        {
            var user = await _userManager.GetUserAsync(User);

            var interests = await _context.MatchRecords
                .Include(m => m.ProjectProposal)
                    .ThenInclude(p => p!.ResearchArea)
                .Where(m => m.SupervisorId == user!.Id)
                .ToListAsync();

            return View(interests);
        }

        // Confirm Match - Triggers Identity Reveal
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmMatch(int matchId)
        {
            var user = await _userManager.GetUserAsync(User);

            var match = await _context.MatchRecords
                .Include(m => m.ProjectProposal)
                .FirstOrDefaultAsync(m => m.Id == matchId
                    && m.SupervisorId == user!.Id);

            if (match != null)
            {
                // Identity Reveal triggered!
                match.IsConfirmed = true;
                match.MatchedDate = DateTime.UtcNow;
                match.ProjectProposal!.Status = "Matched";

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(MyInterests));
        }

        // View Matched Student Details (After Reveal)
        public async Task<IActionResult> MatchDetails(int matchId)
        {
            var user = await _userManager.GetUserAsync(User);

            var match = await _context.MatchRecords
                .Include(m => m.ProjectProposal)
                    .ThenInclude(p => p!.Student)
                .Include(m => m.ProjectProposal)
                    .ThenInclude(p => p!.ResearchArea)
                .FirstOrDefaultAsync(m => m.Id == matchId
                    && m.SupervisorId == user!.Id
                    && m.IsConfirmed == true);

            if (match == null) return NotFound();

            return View(match);
        }
    }
}