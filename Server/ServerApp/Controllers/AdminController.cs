using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerApp.Models;

namespace ServerApp.Controllers
{
    // Контролер панелі адміністратора
    [Authorize(Roles = "Admin")] // Доступ лише для користувачів з роллю Admin
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        // Головна сторінка панелі адміністратора (GET: /admin)
        [HttpGet("/admin")]
        [HttpGet("/")]
        public async Task<IActionResult> Index()
        {
            // Отримання статистики для панелі керування
            ViewBag.AdminEmail = User.Identity?.Name ?? "Адміністратор";
            ViewBag.AttendantsCount = await _context.Attendants.CountAsync();
            ViewBag.SpotsCount = await _context.ParkingSpots.CountAsync();
            ViewBag.FreeSpotsCount = await _context.ParkingSpots.CountAsync(s => s.Status == "Free");
            ViewBag.ReservationsCount = await _context.Reservations.CountAsync();
            ViewBag.ActiveReservationsCount = await _context.Reservations.CountAsync(r => r.Status == "Active");

            return View();
        }
    }
}