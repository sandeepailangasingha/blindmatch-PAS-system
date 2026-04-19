using BlindMatchPAS.Core.Entities;
using BlindMatchPAS.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlindMatchPAS.Web.Controllers
{
    [Authorize]
    public class SearchController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SearchController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Advanced Search & Filter
        public async Task<IActionResult> Index(
            string? keyword,
            int? researchAreaId,
            string? status,
            string? techStack)
        {
            var query = _context.ProjectProposals
                .Include(p => p.ResearchArea)
                .AsQueryable();

            // Keyword search
            if (!string.IsNullOrEmpty(keyword))
                query = query.Where(p =>
                    p.Title.Contains(keyword) ||
                    p.Abstract.Contains(keyword) ||
                    p.TechnicalStack.Contains(keyword));

            // Research Area filter
            if (researchAreaId.HasValue)
                query = query.Where(p =>
                    p.ResearchAreaId == researchAreaId);

            // Status filter
            if (!string.IsNullOrEmpty(status))
                query = query.Where(p => p.Status == status);

            // Tech Stack filter
            if (!string.IsNullOrEmpty(techStack))
                query = query.Where(p =>
                    p.TechnicalStack.Contains(techStack));

            // Blind filter for Supervisors
            if (User.IsInRole("Supervisor"))
                query = query.Select(p => new ProjectProposal
                {
                    Id = p.Id,
                    Title = p.Title,
                    Abstract = p.Abstract,
                    TechnicalStack = p.TechnicalStack,
                    Status = p.Status,
                    ResearchAreaId = p.ResearchAreaId,
                    ResearchArea = p.ResearchArea,
                    StudentId = string.Empty // Hidden!
                });

            var results = await query.ToListAsync();

            ViewBag.ResearchAreas = await _context.ResearchAreas.ToListAsync();
            ViewBag.Keyword = keyword;
            ViewBag.SelectedArea = researchAreaId;
            ViewBag.SelectedStatus = status;
            ViewBag.TechStack = techStack;

            return View(results);
        }
    }
}