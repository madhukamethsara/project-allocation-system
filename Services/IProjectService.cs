using BlindMatchPAS.Models;
using BlindMatchPAS.Models.ViewModels;

namespace BlindMatchPAS.Services
{
    public interface IProjectService
    {
        Task<Project> SubmitProjectAsync(ProjectSubmitViewModel vm, int studentId);
        Task<List<Project>> GetStudentProjectsAsync(int studentId);
        Task<Project?> GetProjectByIdAsync(int id);
        Task<bool> UpdateProjectAsync(int id, ProjectSubmitViewModel vm, int studentId);
        Task<bool> WithdrawProjectAsync(int id, int studentId);
        Task<List<BlindProjectViewModel>> GetBlindProjectsForSupervisorAsync(int supervisorId);
        Task<bool> MatchProjectAsync(int projectId, int supervisorId);
        Task<List<MatchOverviewViewModel>> GetAllMatchesAsync();
        Task<List<Project>> GetAllProjectsAsync();

        // NEW: Pin + Feedback + Revision
        Task<bool> PinProjectAsync(int projectId, int supervisorId);
        Task<bool> UnpinProjectAsync(int projectId, int supervisorId);
        Task<List<Project>> GetPinnedProjectsForSupervisorAsync(int supervisorId);
        Task<bool> AddFeedbackAsync(int projectId, int supervisorId, string comment);
        Task<List<ProjectFeedback>> GetFeedbackForProjectAsync(int projectId);
        Task<bool> MarkRevisionNeededAsync(int projectId, int supervisorId);
        Task<bool> ClearRevisionNeededAsync(int projectId, int supervisorId);
    }
}