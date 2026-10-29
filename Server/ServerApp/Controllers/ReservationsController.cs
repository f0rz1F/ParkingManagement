using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ServerApp.Models;
using ServerApp.Services;

namespace ServerApp.Controllers
{
    // Контролер управління бронюваннями (Actions CRUD)
    [Authorize(Roles = "Admin")]
    public class ReservationsController : Controller
    {
        private readonly AppDbContext _context;

        public ReservationsController(AppDbContext context)
        {
            _context = context;
        }

        // Отримання списку всіх бронювань (GET: /Reservations)
        [HttpGet("/Reservations")]
        [HttpGet("/Actions")]
        public async Task<IActionResult> Index()
        {
            var reservations = await _context.Reservations
                .AsNoTracking()
                .Include(r => r.Attendant)
                .Include(r => r.ParkingSpot)
                .OrderByDescending(r => r.ReservationDate)
                .ToListAsync();
            return View(reservations);
        }

        // Відображення форми створення нового бронювання (GET: /Reservations/Create)
        [HttpGet("/Reservations/Create")]
        [HttpGet("/Actions/Create")]
        public async Task<IActionResult> Create()
        {
            await PopulateDropDownLists();
            var model = new Reservation
            {
                ReservationDate = DateTime.Now,
                Status = "Active"
            };
            return View(model);
        }

        // Збереження нового бронювання в базі даних (POST: /Reservations/Create)
        [HttpPost("/Reservations/Create")]
        [HttpPost("/Actions/Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SpotID,AttendantID,ReservationDate,Status")] Reservation reservation)
        {
            if (!SpotStateSync.ReservationStatuses.Contains(reservation.Status))
            {
                ModelState.AddModelError("Status", "Некоректний статус бронювання");
            }

            if (ModelState.IsValid)
            {
                // Перевірка: чи немає на цьому місці іншого активного бронювання
                if (reservation.Status == SpotStateSync.ReservationActive &&
                    await SpotStateSync.HasOtherActiveReservationAsync(_context, reservation.SpotID))
                {
                    ModelState.AddModelError("SpotID", "Це паркувальне місце наразі вже зайняте");
                    await PopulateDropDownLists(reservation.SpotID, reservation.AttendantID);
                    return View(reservation);
                }

                await using var transaction = await _context.Database.BeginTransactionAsync();

                _context.Add(reservation);
                await _context.SaveChangesAsync();

                // Статус місця перераховується з бронювань
                await SpotStateSync.SyncAsync(_context, reservation.SpotID);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return RedirectToAction(nameof(Index));
            }
            await PopulateDropDownLists(reservation.SpotID, reservation.AttendantID);
            return View(reservation);
        }

        // Відображення форми редагування бронювання (GET: /Reservations/Edit/5)
        [HttpGet("/Reservations/Edit/{id:int}")]
        [HttpGet("/Actions/Edit/{id:int}")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null)
            {
                return NotFound();
            }

            await PopulateDropDownLists(reservation.SpotID, reservation.AttendantID);
            return View(reservation);
        }

        // Збереження змін бронювання (POST: /Reservations/Edit/5)
        [HttpPost("/Reservations/Edit/{id:int}")]
        [HttpPost("/Actions/Edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,SpotID,AttendantID,ReservationDate,Status")] Reservation reservation)
        {
            if (id != reservation.ID)
            {
                return NotFound();
            }

            if (!SpotStateSync.ReservationStatuses.Contains(reservation.Status))
            {
                ModelState.AddModelError("Status", "Некоректний статус бронювання");
            }

            if (ModelState.IsValid)
            {
                var existing = await _context.Reservations.FindAsync(id);
                if (existing == null)
                {
                    return NotFound();
                }

                // Активне бронювання не може перетинатися з іншим активним на тому ж місці
                if (reservation.Status == SpotStateSync.ReservationActive &&
                    await SpotStateSync.HasOtherActiveReservationAsync(_context, reservation.SpotID, id))
                {
                    ModelState.AddModelError("SpotID", "На цьому місці вже є інше активне бронювання");
                    await PopulateDropDownLists(reservation.SpotID, reservation.AttendantID);
                    return View(reservation);
                }

                var oldSpotId = existing.SpotID;

                await using var transaction = await _context.Database.BeginTransactionAsync();

                existing.SpotID = reservation.SpotID;
                existing.AttendantID = reservation.AttendantID;
                existing.ReservationDate = reservation.ReservationDate;
                existing.Status = reservation.Status;
                await _context.SaveChangesAsync();

                // Перерахунок і старого, і нового місця (якщо місце змінили)
                await SpotStateSync.SyncAsync(_context, oldSpotId, existing.SpotID);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return RedirectToAction(nameof(Index));
            }
            await PopulateDropDownLists(reservation.SpotID, reservation.AttendantID);
            return View(reservation);
        }

        // Відображення сторінки підтвердження видалення (GET: /Reservations/Delete/5)
        [HttpGet("/Reservations/Delete/{id:int}")]
        [HttpGet("/Actions/Delete/{id:int}")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reservation = await _context.Reservations
                .Include(r => r.Attendant)
                .Include(r => r.ParkingSpot)
                .FirstOrDefaultAsync(m => m.ID == id);

            if (reservation == null)
            {
                return NotFound();
            }

            return View(reservation);
        }

        // Видалення бронювання з бази даних (POST: /Reservations/Delete/5)
        [HttpPost("/Reservations/Delete/{id:int}")]
        [HttpPost("/Actions/Delete/{id:int}")]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation != null)
            {
                var spotId = reservation.SpotID;

                await using var transaction = await _context.Database.BeginTransactionAsync();

                _context.Reservations.Remove(reservation);
                await _context.SaveChangesAsync();

                // Після видалення місце звільняється, якщо на ньому не лишилось активних бронювань
                await SpotStateSync.SyncAsync(_context, spotId);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // Формування випадаючих списків для паркомісць та працівників
        private async Task PopulateDropDownLists(object? selectedSpot = null, object? selectedAttendant = null)
        {
            var spots = await _context.ParkingSpots.OrderBy(p => p.Number).ToListAsync();
            var attendants = await _context.Attendants.OrderBy(a => a.Name).ToListAsync();

            ViewBag.SpotID = new SelectList(spots.Select(s => new {
                ID = s.ID,
                Display = $"Місце № {s.Number} ({s.Status})"
            }), "ID", "Display", selectedSpot);

            ViewBag.AttendantID = new SelectList(attendants, "ID", "Name", selectedAttendant);
        }
    }
}
