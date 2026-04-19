using BlindMatchPAS.Data;
using BlindMatchPAS.Models;
using BlindMatchPAS.Models.ViewModels;
using BlindMatchPAS.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlindMatchPAS.Controllers
{
    public class SupervisorController : Controller
    {
        private readonly IProjectService _projects;
        private readonly ApplicationDbContext _db;

        public SupervisorController(IProjectService projects, ApplicationDbContext db)
        {
            _projects = projects;
            _db = db;
        }

        private int? CurrentUserId => HttpContext.Session.GetInt32("UserId");
        private bool IsSupervisor => HttpContext.Session.GetString("UserRole") == "Supervisor";

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            if (!IsSupervisor || CurrentUserId == null)
                return RedirectToAction("Login", "Account");

            var supervisorId = CurrentUserId.Value;

            var projects = await _projects.GetBlindProjectsForSupervisorAsync(supervisorId);
            var pinnedProjects = await _projects.GetPinnedProjectsForSupervisorAsync(supervisorId);

            ViewBag.PinnedProjects = pinnedProjects;

            return View(projects);
        }

        [HttpGet]
        public async Task<IActionResult> PinnedProjects()
        {
            if (!IsSupervisor || CurrentUserId == null)
                return RedirectToAction("Login", "Account");

            var pinnedProjects = await _projects.GetPinnedProjectsForSupervisorAsync(CurrentUserId.Value);
            return View(pinnedProjects);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Match(int id)
        {
            if (!IsSupervisor || CurrentUserId == null)
                return RedirectToAction("Login", "Account");

            var success = await _projects.MatchProjectAsync(id, CurrentUserId.Value);

            TempData[success ? "Success" : "Error"] = success
                ? "Match confirmed! Student identity revealed."
                : "This project cannot be matched.";

            return RedirectToAction(nameof(Dashboard));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PinProject(int id)
        {
            if (!IsSupervisor || CurrentUserId == null)
                return RedirectToAction("Login", "Account");

            var success = await _projects.PinProjectAsync(id, CurrentUserId.Value);

            TempData[success ? "Success" : "Error"] = success
                ? "Project pinned successfully."
                : "Already pinned or failed.";

            return RedirectToAction(nameof(Dashboard));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnpinProject(int id)
        {
            if (!IsSupervisor || CurrentUserId == null)
                return RedirectToAction("Login", "Account");

            var success = await _projects.UnpinProjectAsync(id, CurrentUserId.Value);

            TempData[success ? "Success" : "Error"] = success
                ? "Project unpinned."
                : "Failed to unpin.";

            return RedirectToAction(nameof(Dashboard));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddFeedback(int projectId, string comment)
        {
            if (!IsSupervisor || CurrentUserId == null)
                return RedirectToAction("Login", "Account");

            if (string.IsNullOrWhiteSpace(comment))
            {
                TempData["Error"] = "Feedback cannot be empty.";
                return RedirectToAction(nameof(Dashboard));
            }

            var success = await _projects.AddFeedbackAsync(projectId, CurrentUserId.Value, comment.Trim());

            TempData[success ? "Success" : "Error"] = success
                ? "Feedback submitted."
                : "Failed to submit feedback.";

            return RedirectToAction(nameof(Dashboard));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkRevisionNeeded(int id)
        {
            if (!IsSupervisor || CurrentUserId == null)
                return RedirectToAction("Login", "Account");

            var success = await _projects.MarkRevisionNeededAsync(id, CurrentUserId.Value);

            TempData[success ? "Success" : "Error"] = success
                ? "Marked as needing revision."
                : "Failed to update.";

            return RedirectToAction(nameof(Dashboard));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ClearRevisionNeeded(int id)
        {
            if (!IsSupervisor || CurrentUserId == null)
                return RedirectToAction("Login", "Account");

            var success = await _projects.ClearRevisionNeededAsync(id, CurrentUserId.Value);

            TempData[success ? "Success" : "Error"] = success
                ? "Revision cleared."
                : "Failed to clear.";

            return RedirectToAction(nameof(Dashboard));
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            if (!IsSupervisor || CurrentUserId == null)
                return RedirectToAction("Login", "Account");

            var supervisorId = CurrentUserId.Value;

            var supervisor = await _db.Users
                .FirstOrDefaultAsync(u => u.Id == supervisorId && u.Role == UserRole.Supervisor);

            if (supervisor == null)
                return RedirectToAction("Login", "Account");

            var vm = new SupervisorProfileViewModel
            {
                Name = supervisor.Name,
                Email = supervisor.Email,
                Department = supervisor.Department,
                ResearchAreaId = supervisor.ResearchAreaId,
                ResearchAreas = await _db.ResearchAreas.OrderBy(r => r.Name).ToListAsync()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(SupervisorProfileViewModel vm)
        {
            if (!IsSupervisor || CurrentUserId == null)
                return RedirectToAction("Login", "Account");

            var supervisorId = CurrentUserId.Value;

            var supervisor = await _db.Users
                .FirstOrDefaultAsync(u => u.Id == supervisorId && u.Role == UserRole.Supervisor);

            if (supervisor == null)
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
            {
                vm.ResearchAreas = await _db.ResearchAreas.OrderBy(r => r.Name).ToListAsync();
                return View(vm);
            }

            var normalizedEmail = vm.Email.Trim().ToLower();

            var emailExists = await _db.Users
                .AnyAsync(u => u.Email.ToLower() == normalizedEmail && u.Id != supervisor.Id);

            if (emailExists)
            {
                ModelState.AddModelError("Email", "Email already in use.");
                vm.ResearchAreas = await _db.ResearchAreas.OrderBy(r => r.Name).ToListAsync();
                return View(vm);
            }

            supervisor.Name = vm.Name.Trim();
            supervisor.Email = normalizedEmail;
            supervisor.Department = string.IsNullOrWhiteSpace(vm.Department) ? null : vm.Department.Trim();
            supervisor.ResearchAreaId = vm.ResearchAreaId;

            await _db.SaveChangesAsync();

            HttpContext.Session.SetString("UserName", supervisor.Name);
            TempData["Success"] = "Profile updated successfully!";

            return RedirectToAction(nameof(Profile));
        }
    }
}