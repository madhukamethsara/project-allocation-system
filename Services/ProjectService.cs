using BlindMatchPAS.Data;
using BlindMatchPAS.Models;
using BlindMatchPAS.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace BlindMatchPAS.Services
{
    public class ProjectService : IProjectService
    {
        private readonly ApplicationDbContext _db;

        public ProjectService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<bool> PinProjectAsync(int projectId, int supervisorId)
        {
            var projectExists = await _db.Projects.AnyAsync(p => p.Id == projectId);
            if (!projectExists) return false;

            var exists = await _db.PinnedProjects
                .AnyAsync(p => p.ProjectId == projectId && p.SupervisorId == supervisorId);

            if (exists) return false;

            var pin = new PinnedProject
            {
                ProjectId = projectId,
                SupervisorId = supervisorId,
                PinnedAt = DateTime.UtcNow
            };

            _db.PinnedProjects.Add(pin);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UnpinProjectAsync(int projectId, int supervisorId)
        {
            var pin = await _db.PinnedProjects
                .FirstOrDefaultAsync(p => p.ProjectId == projectId && p.SupervisorId == supervisorId);

            if (pin == null) return false;

            _db.PinnedProjects.Remove(pin);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<List<Project>> GetPinnedProjectsForSupervisorAsync(int supervisorId)
        {
            return await _db.PinnedProjects
                .Where(p => p.SupervisorId == supervisorId)
                .Include(p => p.Project!)
                    .ThenInclude(project => project.ResearchArea)
                .Include(p => p.Project!)
                    .ThenInclude(project => project.Supervisor)
                .Include(p => p.Project!)
                    .ThenInclude(project => project.Student)
                .OrderByDescending(p => p.PinnedAt)
                .Select(p => p.Project!)
                .ToListAsync();
        }

        public async Task<bool> MarkRevisionNeededAsync(int projectId, int supervisorId)
        {
            var project = await _db.Projects.FindAsync(projectId);
            if (project == null) return false;

            project.NeedsRevision = true;

        // --- STUDENT OPERATIONS ---


        public async Task<bool> AddFeedbackAsync(int projectId, int supervisorId, string comment)
        {
            if (string.IsNullOrWhiteSpace(comment)) return false;

            var project = await _db.Projects.FindAsync(projectId);
            if (project == null) return false;

            var feedback = new ProjectFeedback
            {
                ProjectId = projectId,
                SupervisorId = supervisorId,
                Comment = comment.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            _db.ProjectFeedbacks.Add(feedback);

            // Automatically flag for revision
            project.NeedsRevision = true;

            // Optional: if you want feedback stage visible in status
            if (project.Status == ProjectStatus.Pending)
            {
                project.Status = ProjectStatus.UnderReview;
            }

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ClearRevisionNeededAsync(int projectId, int supervisorId)
        {
            var project = await _db.Projects.FindAsync(projectId);
            if (project == null) return false;

            project.NeedsRevision = false;
            await _db.SaveChangesAsync();
            return true;
        public async Task<List<ProjectFeedback>> GetFeedbackForProjectAsync(int projectId)
        {
            return await _db.ProjectFeedbacks
                .Where(f => f.ProjectId == projectId)
                .Include(f => f.Supervisor)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();
        }
    }
}