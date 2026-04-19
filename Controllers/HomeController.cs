using Microsoft.AspNetCore.Mvc;

namespace BlindMatchPAS.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            var role = HttpContext.Session.GetString("UserRole");
            return role switch
            {
                "Student" => RedirectToAction("Dashboard", "Student"),
                "Supervisor" => RedirectToAction("Dashboard", "Supervisor"),
                "Admin" => RedirectToAction("Dashboard", "Admin"),
                _ => View()
            };
        }
    }
}
