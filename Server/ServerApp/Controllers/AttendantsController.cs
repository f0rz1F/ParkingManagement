using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerApp.Models;

namespace ServerApp.Controllers
{
    // Контролер управління працівниками (Users CRUD)
    [Authorize(Roles = "Admin")]
    public class AttendantsController : Controller
    {
        private readonly AppDbContext _context;

        public AttendantsController(AppDbContext context)
        {
            _context = context;
        }

        // Отримання списку всіх працівників (GET: /Attendants)
        [HttpGet("/Attendants")]
        [HttpGet("/Users")]
        public async Task<IActionResult> Index()
        {
            var attendants = await _context.Attendants
                .Include(a => a.ParkingSpots)
                .Include(a => a.Reservations)
                .ToListAsync();
            return View(attendants);
        }

        // Відображення форми створення нового працівника (GET: /Attendants/Create)
        [HttpGet("/Attendants/Create")]
        [HttpGet("/Users/Create")]
        public IActionResult Create()
        {
            return View();
        }

        // Збереження нового працівника в базі даних (POST: /Attendants/Create)
        [HttpPost("/Attendants/Create")]
        [HttpPost("/Users/Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Email")] Attendant attendant, string? password)
        {
            ModelState.Remove(nameof(Attendant.PasswordHash));

            if (ModelState.IsValid)
            {
                // Перевірка унікальності email
                var emailExists = await _context.Attendants.AnyAsync(a => a.Email == attendant.Email);
                if (emailExists)
                {
                    ModelState.AddModelError("Email", "Працівник із такою поштою вже існує");
                    return View(attendant);
                }

                // Запис хешу пароля через BCrypt
                var rawPassword = string.IsNullOrWhiteSpace(password) ? "Password123!" : password;
                attendant.PasswordHash = BCrypt.Net.BCrypt.HashPassword(rawPassword);

                _context.Add(attendant);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(attendant);
        }

        // Відображення форми редагування працівника (GET: /Attendants/Edit/5)
        [HttpGet("/Attendants/Edit/{id:int}")]
        [HttpGet("/Users/Edit/{id:int}")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var attendant = await _context.Attendants.FindAsync(id);
            if (attendant == null)
            {
                return NotFound();
            }
            return View(attendant);
        }

        // Збереження змін працівника (POST: /Attendants/Edit/5)
        [HttpPost("/Attendants/Edit/{id:int}")]
        [HttpPost("/Users/Edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Name,Email")] Attendant attendant, string? newPassword)
        {
            if (id != attendant.ID)
            {
                return NotFound();
            }

            ModelState.Remove(nameof(Attendant.PasswordHash));

            if (ModelState.IsValid)
            {
                // Перевірка унікальності email серед інших працівників
                var emailExists = await _context.Attendants.AnyAsync(a => a.Email == attendant.Email && a.ID != id);
                if (emailExists)
                {
                    ModelState.AddModelError("Email", "Ця електронна пошта вже використовується іншим працівником");
                    return View(attendant);
                }

                var existing = await _context.Attendants.FindAsync(id);
                if (existing == null)
                {
                    return NotFound();
                }

                existing.Name = attendant.Name;
                existing.Email = attendant.Email;

                // Оновлення пароля за наявності нового значення через BCrypt
                if (!string.IsNullOrWhiteSpace(newPassword))
                {
                    existing.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
                }

                try
                {
                    _context.Update(existing);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.Attendants.AnyAsync(e => e.ID == id))
                    {
                        return NotFound();
                    }
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(attendant);
        }

        // Відображення сторінки підтвердження видалення (GET: /Attendants/Delete/5)
        [HttpGet("/Attendants/Delete/{id:int}")]
        [HttpGet("/Users/Delete/{id:int}")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var attendant = await _context.Attendants
                .Include(a => a.ParkingSpots)
                .Include(a => a.Reservations)
                .FirstOrDefaultAsync(m => m.ID == id);

            if (attendant == null)
            {
                return NotFound();
            }

            return View(attendant);
        }

        // Видалення працівника з бази даних (POST: /Attendants/Delete/5)
        [HttpPost("/Attendants/Delete/{id:int}")]
        [HttpPost("/Users/Delete/{id:int}")]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var attendant = await _context.Attendants.FindAsync(id);
            if (attendant != null)
            {
                _context.Attendants.Remove(attendant);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
