using BlindMatchPAS.Data;
using BlindMatchPAS.Models;
using BlindMatchPAS.Models.ViewModels;
using BlindMatchPAS.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlindMatchPAS.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthService _auth;
        private readonly ApplicationDbContext _db;

        public AccountController(IAuthService auth, ApplicationDbContext db)
        {
            _auth = auth;
            _db = db;
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var user = await _auth.LoginAsync(vm);
            if (user == null)
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View(vm);
            }

            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("UserName", user.Name);
            HttpContext.Session.SetString("UserRole", user.Role.ToString());

            return user.Role switch
            {
                UserRole.Student => RedirectToAction("Dashboard", "Student"),
                UserRole.Supervisor => RedirectToAction("Dashboard", "Supervisor"),
                UserRole.Admin => RedirectToAction("Dashboard", "Admin"),
                _ => RedirectToAction("Index", "Home")
            };
        }

        [HttpGet]
        public async Task<IActionResult> Register()
        {
            var vm = new RegisterViewModel
            {
                ResearchAreas = await _db.ResearchAreas
                    .OrderBy(r => r.Name)
                    .ToListAsync()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel vm)
        {
            vm.ResearchAreas = await _db.ResearchAreas
                .OrderBy(r => r.Name)
                .ToListAsync();

            if (vm.Role == UserRole.Admin)
            {
                ModelState.AddModelError(nameof(vm.Role), "Admin registration is not allowed.");
            }

            if (vm.Role == UserRole.Student)
            {
                if (string.IsNullOrWhiteSpace(vm.Batch))
                    ModelState.AddModelError(nameof(vm.Batch), "Batch is required for students.");

                if (string.IsNullOrWhiteSpace(vm.RegistrationNumber))
                    ModelState.AddModelError(nameof(vm.RegistrationNumber), "Registration number is required for students.");
            }

            if (vm.Role == UserRole.Supervisor)
            {
                if (string.IsNullOrWhiteSpace(vm.Department))
                    ModelState.AddModelError(nameof(vm.Department), "Department is required for supervisors.");

                if (vm.ResearchAreaId == null || vm.ResearchAreaId <= 0)
                    ModelState.AddModelError(nameof(vm.ResearchAreaId), "Research area is required for supervisors.");
            }

            if (!ModelState.IsValid)
                return View(vm);

            var normalizedEmail = vm.Email.Trim().ToLower();

            var existsInUsers = await _db.Users
                .AnyAsync(u => u.Email.ToLower() == normalizedEmail);

            var existsInRequests = await _db.RegistrationRequests
                .AnyAsync(r => r.Email.ToLower() == normalizedEmail && r.Status == "Pending");

            if (existsInUsers || existsInRequests)
            {
                ModelState.AddModelError(nameof(vm.Email), "This email is already registered or pending approval.");
                return View(vm);
            }

            var request = new RegistrationRequest
            {
                Name = vm.Name.Trim(),
                Email = normalizedEmail,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(vm.Password),
                Role = vm.Role,
                Batch = vm.Role == UserRole.Student ? vm.Batch?.Trim() : null,
                RegistrationNumber = vm.Role == UserRole.Student ? vm.RegistrationNumber?.Trim() : null,
                Department = vm.Role == UserRole.Supervisor ? vm.Department?.Trim() : null,
                ResearchAreaId = vm.Role == UserRole.Supervisor ? vm.ResearchAreaId : null,
                Status = "Pending",
                RequestedAt = DateTime.Now
            };

            _db.RegistrationRequests.Add(request);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Your registration request was submitted successfully. Please wait for admin approval.";
            return RedirectToAction(nameof(Login));
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction(nameof(Login));
        }
    }
}