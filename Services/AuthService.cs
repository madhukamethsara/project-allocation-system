using BlindMatchPAS.Data;
using BlindMatchPAS.Models;
using BlindMatchPAS.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace BlindMatchPAS.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _db;

        public AuthService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<ApplicationUser?> LoginAsync(LoginViewModel vm)
        {
            if (string.IsNullOrWhiteSpace(vm.Email) || string.IsNullOrWhiteSpace(vm.Password))
                return null;

            var normalizedEmail = vm.Email.Trim().ToLower();

            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail);

            if (user == null)
                return null;

            // Individual access control
            if (!user.IsActive)
                return null;

            var validPassword = BCrypt.Net.BCrypt.Verify(vm.Password, user.PasswordHash);
            if (!validPassword)
                return null;

            // Batch-wise login control for students
            if (user.Role == UserRole.Student)
            {
                if (string.IsNullOrWhiteSpace(user.Batch))
                    return null;

                var batchAccess = await _db.BatchAccesses
                    .FirstOrDefaultAsync(b => b.BatchName == user.Batch);

                if (batchAccess == null || !batchAccess.IsLoginEnabled)
                    return null;
            }

            // Supervisor-wise control is already handled by IsActive
            return user;
        }

        public async Task<ApplicationUser?> GetUserByIdAsync(int id)
        {
            return await _db.Users.FindAsync(id);
        }
    }
}