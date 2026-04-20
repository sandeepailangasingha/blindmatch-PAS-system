using BlindMatchPAS.Core.Entities;
using BlindMatchPAS.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BlindMatchPAS.Web.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public StudentController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Dashboard - Status Tracking
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var proposals = await _context.ProjectProposals
                .Include(p => p.ResearchArea)
                .Include(p => p.Student)
                .Where(p => p.StudentId == user!.Id)
                .ToListAsync();
            return View(proposals);
        }

        // Submit New Proposal
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.ResearchAreas = new SelectList(
                await _context.ResearchAreas.ToListAsync(), "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProjectProposal model)
        {
            var user = await _userManager.GetUserAsync(User);

            ModelState.Remove("StudentId");
            ModelState.Remove("Student");
            ModelState.Remove("ResearchArea");

            if (ModelState.IsValid)
            {
                model.StudentId = user!.Id;
                model.Status = "Pending";
                _context.ProjectProposals.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.ResearchAreas = new SelectList(
                await _context.ResearchAreas.ToListAsync(), "Id", "Name");
            return View(model);
        }

        // Edit Proposal
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var proposal = await _context.ProjectProposals
                .FirstOrDefaultAsync(p => p.Id == id && p.StudentId == user!.Id);

            if (proposal == null || proposal.Status == "Matched")
                return RedirectToAction(nameof(Index));

            ViewBag.ResearchAreas = new SelectList(
                await _context.ResearchAreas.ToListAsync(), "Id", "Name");
            return View(proposal);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProjectProposal model)
        {
            ModelState.Remove("StudentId");
            ModelState.Remove("Student");
            ModelState.Remove("ResearchArea");

            if (ModelState.IsValid)
            {
                _context.ProjectProposals.Update(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.ResearchAreas = new SelectList(
                await _context.ResearchAreas.ToListAsync(), "Id", "Name");
            return View(model);
        }

        // Withdraw Proposal
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Withdraw(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var proposal = await _context.ProjectProposals
                .FirstOrDefaultAsync(p => p.Id == id && p.StudentId == user!.Id);

            if (proposal != null && proposal.Status != "Matched")
            {
                _context.ProjectProposals.Remove(proposal);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // Details + Identity Reveal
        public async Task<IActionResult> Details(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var proposal = await _context.ProjectProposals
                .Include(p => p.ResearchArea)
                .FirstOrDefaultAsync(p => p.Id == id && p.StudentId == user!.Id);

            if (proposal == null) return NotFound();

            // Identity Reveal - only if Matched
            if (proposal.Status == "Matched")
            {
                var match = await _context.MatchRecords
                    .Include(m => m.Supervisor)
                    .FirstOrDefaultAsync(m => m.ProjectProposalId == id
                        && m.IsConfirmed == true);
                ViewBag.Supervisor = match?.Supervisor;
            }

            return View(proposal);
        }
    }
}