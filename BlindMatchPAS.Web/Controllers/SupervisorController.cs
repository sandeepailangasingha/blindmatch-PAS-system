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

        // Blind Dashboard
        public async Task<IActionResult> Index(int? researchAreaId)
        {
            var user = await _userManager.GetUserAsync(User);

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

                var proposal = await _context.ProjectProposals
                    .FindAsync(proposalId);
                if (proposal != null)
                    proposal.Status = "Under Review";

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(MyInterests));
        }

        // My Interests
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

        // Confirm Match
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
                match.IsConfirmed = true;
                match.MatchedDate = DateTime.UtcNow;
                match.ProjectProposal!.Status = "Matched";

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(MyInterests));
        }

        // Match Details - Identity Reveal
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

        // Manage Research Areas
        public async Task<IActionResult> ManageAreas()
        {
            var user = await _userManager.GetUserAsync(User);

            var allAreas = await _context.ResearchAreas.ToListAsync();

            var myAreaIds = await _context.SupervisorResearchAreas
                .Where(s => s.SupervisorId == user!.Id)
                .Select(s => s.ResearchAreaId)
                .ToListAsync();

            ViewBag.MyAreaIds = myAreaIds;
            return View(allAreas);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddArea(int researchAreaId)
        {
            var user = await _userManager.GetUserAsync(User);

            var exists = await _context.SupervisorResearchAreas
                .AnyAsync(s => s.SupervisorId == user!.Id
                    && s.ResearchAreaId == researchAreaId);

            if (!exists)
            {
                _context.SupervisorResearchAreas.Add(new SupervisorResearchArea
                {
                    SupervisorId = user!.Id,
                    ResearchAreaId = researchAreaId
                });
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(ManageAreas));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveArea(int researchAreaId)
        {
            var user = await _userManager.GetUserAsync(User);

            var area = await _context.SupervisorResearchAreas
                .FirstOrDefaultAsync(s => s.SupervisorId == user!.Id
                    && s.ResearchAreaId == researchAreaId);

            if (area != null)
            {
                _context.SupervisorResearchAreas.Remove(area);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(ManageAreas));
        }
    }
}