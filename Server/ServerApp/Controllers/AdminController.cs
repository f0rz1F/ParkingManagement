using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ServerApp.Controllers
{
    // Захищений контролер адмін-панелі
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        [HttpGet("/admin")]
        public IActionResult Index()
        {
            ViewBag.AdminEmail = User.Identity?.Name ?? "Адміністратор";
            return View();
        }
    }
}