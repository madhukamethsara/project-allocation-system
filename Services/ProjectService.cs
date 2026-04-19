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