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

        public async Task<Project> SubmitProjectAsync(ProjectSubmitViewModel vm, int studentId)
        {
            var project = new Project
            {
                Title = vm.Title,
                Description = vm.Description,
                TechStack = vm.TechStack,
                ResearchAreaId = vm.ResearchAreaId,
                StudentId = studentId,
                Status = ProjectStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                NeedsRevision = false
            };

            _db.Projects.Add(project);
            await _db.SaveChangesAsync();
            return project;
        }

        public async Task<List<Project>> GetStudentProjectsAsync(int studentId)
        {
            return await _db.Projects
                .Include(p => p.ResearchArea)
                .Include(p => p.Supervisor)
                .Include(p => p.Feedbacks!)
                    .ThenInclude(f => f.Supervisor)
                .Where(p => p.StudentId == studentId && p.Status != ProjectStatus.Withdrawn)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<Project?> GetProjectByIdAsync(int id)
        {
            return await _db.Projects
                .Include(p => p.ResearchArea)
                .Include(p => p.Student)
                .Include(p => p.Supervisor)
                .Include(p => p.Feedbacks!)
                    .ThenInclude(f => f.Supervisor)
                .Include(p => p.PinnedBy)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<bool> UpdateProjectAsync(int id, ProjectSubmitViewModel vm, int studentId)
        {
            var project = await _db.Projects.FindAsync(id);
            if (project == null || project.StudentId != studentId) return false;
            if (project.Status == ProjectStatus.Matched || project.Status == ProjectStatus.Withdrawn) return false;

            project.Title = vm.Title;
            project.Description = vm.Description;
            project.TechStack = vm.TechStack;
            project.ResearchAreaId = vm.ResearchAreaId;


            project.NeedsRevision = false;

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> WithdrawProjectAsync(int id, int studentId)
        {
            var project = await _db.Projects.FindAsync(id);
            if (project == null || project.StudentId != studentId) return false;
            if (project.Status == ProjectStatus.Matched) return false;

            project.Status = ProjectStatus.Withdrawn;
            await _db.SaveChangesAsync();
            return true;
        }


        public async Task<List<BlindProjectViewModel>> GetBlindProjectsForSupervisorAsync(int supervisorId)
        {
            var supervisor = await _db.Users
                .FirstOrDefaultAsync(u => u.Id == supervisorId);

            if (supervisor == null)
                return new List<BlindProjectViewModel>();

            var supervisorAreaId = supervisor.ResearchAreaId;

            var query = _db.Projects
                .Include(p => p.ResearchArea)
                .Include(p => p.Student)
                .Include(p => p.Supervisor)
                .Include(p => p.PinnedBy)
                .Include(p => p.Feedbacks)
                .AsQueryable();

            if (supervisorAreaId != null)
            {
                query = query.Where(p =>
                    (
                        p.ResearchAreaId == supervisorAreaId &&
                        (p.Status == ProjectStatus.Pending || p.Status == ProjectStatus.UnderReview)
                    )
                    ||
                    (
                        p.Status == ProjectStatus.Matched && p.SupervisorId == supervisorId
                    )
                );
            }
            else
            {
                query = query.Where(p =>
                    p.Status == ProjectStatus.Pending ||
                    p.Status == ProjectStatus.UnderReview ||
                    (p.Status == ProjectStatus.Matched && p.SupervisorId == supervisorId)
                );
            }

            var projects = await query
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return projects.Select(p => MapToBlindViewModel(p, supervisorId)).ToList();
        }

        private static BlindProjectViewModel MapToBlindViewModel(Project p, int supervisorId)
        {
            bool isMatchedByThisSupervisor = p.Status == ProjectStatus.Matched
                                             && p.SupervisorId == supervisorId;

            return new BlindProjectViewModel
            {
                Id = p.Id,
                Title = p.Title,
                Description = p.Description,
                TechStack = p.TechStack,
                ResearchAreaName = p.ResearchArea?.Name ?? "Unknown",
                Status = p.Status,
                CreatedAt = p.CreatedAt,
                IsMatched = p.Status == ProjectStatus.Matched,
                RevealedStudentName = isMatchedByThisSupervisor ? p.Student?.Name : null,
                RevealedStudentEmail = isMatchedByThisSupervisor ? p.Student?.Email : null
            };
        }

        public async Task<bool> MatchProjectAsync(int projectId, int supervisorId)
        {
            var project = await _db.Projects.FindAsync(projectId);
            if (project == null) return false;

            if (project.Status != ProjectStatus.Pending && project.Status != ProjectStatus.UnderReview)
                return false;

            project.SupervisorId = supervisorId;
            project.Status = ProjectStatus.Matched;
            project.MatchedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return true;
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

        public async Task<bool> MarkRevisionNeededAsync(int projectId, int supervisorId)
        {
            var project = await _db.Projects.FindAsync(projectId);
            if (project == null) return false;

            project.NeedsRevision = true;

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
        }

        public async Task<List<MatchOverviewViewModel>> GetAllMatchesAsync()
        {
            return await _db.Projects
                .Include(p => p.Student)
                .Include(p => p.Supervisor)
                .Include(p => p.ResearchArea)
                .Where(p => p.Status == ProjectStatus.Matched)
                .OrderByDescending(p => p.MatchedAt)
                .Select(p => new MatchOverviewViewModel
                {
                    ProjectId = p.Id,
                    ProjectTitle = p.Title,
                    StudentName = p.Student!.Name,
                    StudentEmail = p.Student.Email,
                    SupervisorName = p.Supervisor!.Name,
                    SupervisorEmail = p.Supervisor.Email,
                    ResearchArea = p.ResearchArea!.Name,
                    MatchedAt = p.MatchedAt
                })
                .ToListAsync();
        }

        public async Task<List<Project>> GetAllProjectsAsync()
        {
            return await _db.Projects
                .Include(p => p.Student)
                .Include(p => p.Supervisor)
                .Include(p => p.ResearchArea)
                .Include(p => p.Feedbacks!)
                    .ThenInclude(f => f.Supervisor)
                .Include(p => p.PinnedBy)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }
    }
}