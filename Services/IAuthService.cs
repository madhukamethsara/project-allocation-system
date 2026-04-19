using BlindMatchPAS.Models;
using BlindMatchPAS.Models.ViewModels;

namespace BlindMatchPAS.Services
{
    public interface IAuthService
    {
        Task<ApplicationUser?> LoginAsync(LoginViewModel vm);
        Task<ApplicationUser?> GetUserByIdAsync(int id);
    }
}