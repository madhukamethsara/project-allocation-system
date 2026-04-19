using System;
using BlindMatchPAS.Data;
using BlindMatchPAS.Models;
using BlindMatchPAS.Models.ViewModels;
using BlindMatchPAS.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlindMatchPAS.Controllers
{
    public class AdminController : Controller
    {
        private readonly IProjectService _projects;
        private readonly IResearchAreaService _areas;
        private readonly IAuthService _auth;
        private readonly ApplicationDbContext _db;

        public AdminController(
            IProjectService projects,
            IResearchAreaService areas,
            IAuthService auth,
            ApplicationDbContext db)
        {
            _projects = projects;
            _areas = areas;
            _auth = auth;
            _db = db;
        }

        private bool IsAdmin =>
            HttpContext.Session.GetString("UserRole")?.Equals("Admin", StringComparison.OrdinalIgnoreCase) == true;

        public async Task<IActionResult> Dashboard()
        {
            if (!IsAdmin) return RedirectToAction("Login", "Account");

            ViewBag.TotalProjects = (await _projects.GetAllProjectsAsync()).Count;
            ViewBag.TotalMatches = (await _projects.GetAllMatchesAsync()).Count;
            ViewBag.TotalUsers = await _db.Users.CountAsync();
            ViewBag.TotalStudents = await _db.Users.CountAsync(u => u.Role == UserRole.Student);
            ViewBag.TotalSupervisors = await _db.Users.CountAsync(u => u.Role == UserRole.Supervisor);

            return View();
        }

        public async Task<IActionResult> AllProjects()
        {
            if (!IsAdmin) return RedirectToAction("Login", "Account");

            var list = await _projects.GetAllProjectsAsync();
            return View(list);
        }

        public async Task<IActionResult> AllMatches()
        {
            if (!IsAdmin) return RedirectToAction("Login", "Account");

            var matches = await _projects.GetAllMatchesAsync();
            return View(matches);
        }

        public async Task<IActionResult> ResearchAreas()
        {
            if (!IsAdmin) return RedirectToAction("Login", "Account");

            var areas = await _areas.GetAllAsync();
            return View(areas);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddResearchArea(string name)
        {
            if (!IsAdmin) return RedirectToAction("Login", "Account");

            if (!string.IsNullOrWhiteSpace(name))
            {
                await _areas.CreateAsync(name.Trim());
                TempData["Success"] = "Research area added.";
            }
            else
            {
                TempData["Error"] = "Research area name cannot be empty.";
            }

            return RedirectToAction(nameof(ResearchAreas));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteResearchArea(int id)
        {
            if (!IsAdmin) return RedirectToAction("Login", "Account");

            await _areas.DeleteAsync(id);
            TempData["Success"] = "Research area removed.";
            return RedirectToAction(nameof(ResearchAreas));
        }

        // ---------------------------------------------------
        // ONE COMBINED PAGE
        // ---------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> UserManagement(string? search, string? batch, string? role)
        {
            if (!IsAdmin) return RedirectToAction("Login", "Account");

            var query = _db.Users
                .Include(u => u.ResearchArea)
                .AsQueryable();

            // Filter by name or email
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchText = search.Trim().ToLower();

                query = query.Where(u =>
                    u.Name.ToLower().Contains(searchText) ||
                    u.Email.ToLower().Contains(searchText));
            }

            // Filter by batch
            if (!string.IsNullOrWhiteSpace(batch))
            {
                query = query.Where(u => u.Batch == batch);
            }

            // Filter by role
            if (!string.IsNullOrWhiteSpace(role) &&
                Enum.TryParse<UserRole>(role, true, out var parsedRole))
            {
                query = query.Where(u => u.Role == parsedRole);
            }

            var users = await query
                .OrderBy(u => u.Role)
                .ThenBy(u => u.Name)
                .ToListAsync();

            var researchAreas = await _areas.GetAllAsync();
            var batches = await _db.BatchAccesses
                .OrderBy(b => b.BatchName)
                .Select(b => b.BatchName)
                .ToListAsync();

            ViewBag.Search = search;
            ViewBag.Batch = batch;
            ViewBag.Role = role;

            var vm = new AdminUserManagementPageViewModel
            {
                NewStudent = new AdminCreateUserViewModel
                {
                    Role = UserRole.Student,
                    IsActive = true,
                    ResearchAreas = researchAreas
                },
                NewSupervisor = new AdminCreateUserViewModel
                {
                    Role = UserRole.Supervisor,
                    IsActive = true,
                    ResearchAreas = researchAreas
                },
                Users = users,
                ResearchAreas = researchAreas,
                Batches = batches
            };

            return View(vm);
        }
        // ---------------------------------------------------
        // ADD STUDENT
        // ---------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddStudent(AdminCreateUserViewModel vm)
        {
            if (!IsAdmin) return RedirectToAction("Login", "Account");

            vm.Role = UserRole.Student;

            if (string.IsNullOrWhiteSpace(vm.Batch))
                ModelState.AddModelError(nameof(vm.Batch), "Batch is required for students.");

            if (string.IsNullOrWhiteSpace(vm.RegistrationNumber))
                ModelState.AddModelError(nameof(vm.RegistrationNumber), "Registration number is required for students.");

            if (!ModelState.IsValid)
            {
                var pageVm = await BuildUserManagementPageViewModelAsync();
                pageVm.NewStudent = vm;
                return View("UserManagement", pageVm);
            }

            var normalizedEmail = vm.Email.Trim().ToLower();

            bool emailExists = await _db.Users.AnyAsync(u => u.Email.ToLower() == normalizedEmail);
            if (emailExists)
            {
                ModelState.AddModelError(nameof(vm.Email), "This email is already in use.");

                var pageVm = await BuildUserManagementPageViewModelAsync();
                pageVm.NewStudent = vm;
                return View("UserManagement", pageVm);
            }

            var student = new ApplicationUser
            {
                Name = vm.Name.Trim(),
                Email = normalizedEmail,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(vm.Password),
                Role = UserRole.Student,
                Batch = vm.Batch?.Trim(),
                RegistrationNumber = vm.RegistrationNumber?.Trim(),
                Department = null,
                ResearchAreaId = null,
                IsActive = vm.IsActive
            };

            _db.Users.Add(student);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Student added successfully.";
            return RedirectToAction(nameof(UserManagement));
        }

        // ---------------------------------------------------
        // ADD SUPERVISOR
        // ---------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSupervisor(AdminCreateUserViewModel vm)
        {
            if (!IsAdmin) return RedirectToAction("Login", "Account");

            vm.Role = UserRole.Supervisor;

            if (string.IsNullOrWhiteSpace(vm.Department))
                ModelState.AddModelError(nameof(vm.Department), "Department is required for supervisors.");

            if (vm.ResearchAreaId == null || vm.ResearchAreaId <= 0)
                ModelState.AddModelError(nameof(vm.ResearchAreaId), "Research area is required for supervisors.");

            if (!ModelState.IsValid)
            {
                var pageVm = await BuildUserManagementPageViewModelAsync();
                pageVm.NewSupervisor = vm;
                return View("UserManagement", pageVm);
            }

            var normalizedEmail = vm.Email.Trim().ToLower();

            bool emailExists = await _db.Users.AnyAsync(u => u.Email.ToLower() == normalizedEmail);
            if (emailExists)
            {
                ModelState.AddModelError(nameof(vm.Email), "This email is already in use.");

                var pageVm = await BuildUserManagementPageViewModelAsync();
                pageVm.NewSupervisor = vm;
                return View("UserManagement", pageVm);
            }

            var supervisor = new ApplicationUser
            {
                Name = vm.Name.Trim(),
                Email = normalizedEmail,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(vm.Password),
                Role = UserRole.Supervisor,
                Batch = null,
                RegistrationNumber = null,
                Department = vm.Department?.Trim(),
                ResearchAreaId = vm.ResearchAreaId,
                IsActive = vm.IsActive
            };

            _db.Users.Add(supervisor);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Supervisor added successfully.";
            return RedirectToAction(nameof(UserManagement));
        }

        // ---------------------------------------------------
        // TOGGLE USER STATUS
        // ---------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleUserStatus(int id)
        {
            if (!IsAdmin) return RedirectToAction("Login", "Account");

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction(nameof(UserManagement));
            }

            user.IsActive = !user.IsActive;
            await _db.SaveChangesAsync();

            TempData["Success"] = user.IsActive
                ? $"{user.Name} is now active."
                : $"{user.Name} has been disabled.";

            return RedirectToAction(nameof(UserManagement));
        }
        [HttpGet]
        public async Task<IActionResult> PendingRegistrations()
        {
            if (!IsAdmin) return RedirectToAction("Login", "Account");

            var requests = await _db.RegistrationRequests
                .Where(r => r.Status == "Pending")
                .OrderByDescending(r => r.RequestedAt)
                .ToListAsync();

            return View(requests);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveRequest(int id)
        {
            if (!IsAdmin) return RedirectToAction("Login", "Account");

            var request = await _db.RegistrationRequests.FirstOrDefaultAsync(r => r.Id == id);
            if (request == null)
            {
                TempData["Error"] = "Request not found.";
                return RedirectToAction(nameof(PendingRegistrations));
            }

            var exists = await _db.Users.AnyAsync(u => u.Email.ToLower() == request.Email.ToLower());
            if (exists)
            {
                TempData["Error"] = "A user with this email already exists.";
                return RedirectToAction(nameof(PendingRegistrations));
            }

            var user = new BlindMatchPAS.Models.ApplicationUser
            {
                Name = request.Name,
                Email = request.Email,
                PasswordHash = request.PasswordHash,
                Role = request.Role,
                Batch = request.Batch,
                RegistrationNumber = request.RegistrationNumber,
                Department = request.Department,
                ResearchAreaId = request.ResearchAreaId,
                IsActive = true
            };

            _db.Users.Add(user);
            request.Status = "Approved";

            await _db.SaveChangesAsync();

            TempData["Success"] = $"{request.Name} has been approved.";
            return RedirectToAction(nameof(PendingRegistrations));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectRequest(int id)
        {
            if (!IsAdmin) return RedirectToAction("Login", "Account");

            var request = await _db.RegistrationRequests.FirstOrDefaultAsync(r => r.Id == id);
            if (request == null)
            {
                TempData["Error"] = "Request not found.";
                return RedirectToAction(nameof(PendingRegistrations));
            }

            request.Status = "Rejected";
            await _db.SaveChangesAsync();

            TempData["Success"] = $"{request.Name} has been rejected.";
            return RedirectToAction(nameof(PendingRegistrations));
        }
        
     
        [HttpGet]
        public async Task<IActionResult> BatchAccess()
        {
            if (!IsAdmin) return RedirectToAction("Login", "Account");

            var batches = await _db.BatchAccesses
                .OrderBy(b => b.BatchName)
                .ToListAsync();

            return View(batches);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleBatchAccess(int id)
        {
            if (!IsAdmin) return RedirectToAction("Login", "Account");

            var batch = await _db.BatchAccesses.FirstOrDefaultAsync(b => b.Id == id);
            if (batch == null)
            {
                TempData["Error"] = "Batch not found.";
                return RedirectToAction(nameof(BatchAccess));
            }

            batch.IsLoginEnabled = !batch.IsLoginEnabled;
            await _db.SaveChangesAsync();

            TempData["Success"] = batch.IsLoginEnabled
                ? $"Login enabled for batch {batch.BatchName}."
                : $"Login disabled for batch {batch.BatchName}.";

            return RedirectToAction(nameof(BatchAccess));
        }

        private async Task<AdminUserManagementPageViewModel> BuildUserManagementPageViewModelAsync()
        {
            var users = await _db.Users
                .Include(u => u.ResearchArea)
                .OrderBy(u => u.Role)
                .ThenBy(u => u.Name)
                .ToListAsync();

            var researchAreas = await _areas.GetAllAsync();

            var batches = await _db.BatchAccesses
                .OrderBy(b => b.BatchName)
                .Select(b => b.BatchName)
                .ToListAsync();

            return new AdminUserManagementPageViewModel
            {
                NewStudent = new AdminCreateUserViewModel
                {
                    Role = UserRole.Student,
                    IsActive = true,
                    ResearchAreas = researchAreas
                },
                NewSupervisor = new AdminCreateUserViewModel
                {
                    Role = UserRole.Supervisor,
                    IsActive = true,
                    ResearchAreas = researchAreas
                },
                Users = users,
                ResearchAreas = researchAreas,
                Batches = batches
            };
        }
    }
}