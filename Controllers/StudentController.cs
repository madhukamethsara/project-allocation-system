using BlindMatchPAS.Models;
using BlindMatchPAS.Models.ViewModels;
using BlindMatchPAS.Services;
using BlindMatchPAS.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlindMatchPAS.Controllers
{
    public class StudentController : Controller
    {
        private readonly IProjectService _projects;
        private readonly IResearchAreaService _areas;
        private readonly ApplicationDbContext _db;

        public StudentController(
            IProjectService projects,
            IResearchAreaService areas,
            ApplicationDbContext db)
        {
            _projects = projects;
            _areas = areas;
            _db = db;
        }

        private int? CurrentUserId => HttpContext.Session.GetInt32("UserId");
        private bool IsStudent => HttpContext.Session.GetString("UserRole") == "Student";


        public async Task<IActionResult> Dashboard()
        {
            if (!IsStudent) return RedirectToAction("Login", "Account");

            var list = await _projects.GetStudentProjectsAsync(CurrentUserId!.Value);
            return View(list);
        }


        [HttpGet]
        public async Task<IActionResult> Submit()
        {
            if (!IsStudent) return RedirectToAction("Login", "Account");

            var vm = new ProjectSubmitViewModel
            {
                ResearchAreas = await _areas.GetAllAsync()
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Submit(ProjectSubmitViewModel vm)
        {
            if (!IsStudent) return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
            {
                vm.ResearchAreas = await _areas.GetAllAsync();
                return View(vm);
            }

            await _projects.SubmitProjectAsync(vm, CurrentUserId!.Value);

            TempData["Success"] = "Project submitted successfully!";
            return RedirectToAction("Dashboard");
        }


        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            if (!IsStudent) return RedirectToAction("Login", "Account");

            var userId = CurrentUserId;
            if (userId == null) return RedirectToAction("Login", "Account");

            var student = await _db.Users
                .FirstOrDefaultAsync(u => u.Id == userId && u.Role == UserRole.Student);

            if (student == null) return RedirectToAction("Login", "Account");

            var vm = new StudentProfileEditViewModel
            {
                Name = student.Name,
                Email = student.Email,
                Batch = student.Batch ?? "",
                RegistrationNumber = student.RegistrationNumber ?? ""
            };

            return View(vm);
        }
        [HttpGet]
        public async Task<IActionResult> ProjectDetails(int id)
        {
            if (!IsStudent) return RedirectToAction("Login", "Account");

            var project = await _db.Projects
                .Include(p => p.ResearchArea)
                .Include(p => p.Supervisor)
                .Include(p => p.Feedbacks)
                    .ThenInclude(f => f.Supervisor)
                .FirstOrDefaultAsync(p => p.Id == id && p.StudentId == CurrentUserId);

            if (project == null) return RedirectToAction("Dashboard");

            return View(project);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(StudentProfileEditViewModel vm)
        {
            if (!IsStudent) return RedirectToAction("Login", "Account");

            var userId = CurrentUserId;
            if (userId == null) return RedirectToAction("Login", "Account");

            var student = await _db.Users
                .FirstOrDefaultAsync(u => u.Id == userId && u.Role == UserRole.Student);

            if (student == null) return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
                return View(vm);

            // Prevent duplicate emails
            var emailExists = await _db.Users
                .AnyAsync(u => u.Email.ToLower() == vm.Email.ToLower() && u.Id != student.Id);

            if (emailExists)
            {
                ModelState.AddModelError("Email", "Email already in use.");
                return View(vm);
            }

            // Update fields
            student.Name = vm.Name.Trim();
            student.Email = vm.Email.Trim().ToLower();
            student.Batch = vm.Batch.Trim();
            student.RegistrationNumber = vm.RegistrationNumber.Trim();

            await _db.SaveChangesAsync();

            // Update session name
            HttpContext.Session.SetString("UserName", student.Name);

            TempData["Success"] = "Profile updated successfully!";

            return RedirectToAction(nameof(Profile));
        }
    }
}