using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ServerApp.Models;

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
            if (ModelState.IsValid)
            {
                // Перевірка: чи не зайняте вже це паркомісце іншим бронюванням
                var spot = await _context.ParkingSpots.FindAsync(reservation.SpotID);
                if (spot != null && spot.Status == "Occupied" && reservation.Status == "Active")
                {
                    ModelState.AddModelError("SpotID", "Це паркувальне місце наразі вже зайняте");
                    await PopulateDropDownLists(reservation.SpotID, reservation.AttendantID);
                    return View(reservation);
                }

                _context.Add(reservation);

                // Оновлення статусу місця на Occupied, якщо бронювання активне
                if (reservation.Status == "Active" && spot != null)
                {
                    spot.Status = "Occupied";
                    spot.UserID = reservation.AttendantID;
                    _context.Update(spot);
                }

                await _context.SaveChangesAsync();
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

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(reservation);

                    // Якщо бронювання завершене або скасоване, звільняємо місце
                    if (reservation.Status == "Completed" || reservation.Status == "Cancelled")
                    {
                        var spot = await _context.ParkingSpots.FindAsync(reservation.SpotID);
                        if (spot != null && spot.UserID == reservation.AttendantID)
                        {
                            spot.Status = "Free";
                            spot.UserID = null;
                            _context.Update(spot);
                        }
                    }

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.Reservations.AnyAsync(e => e.ID == id))
                    {
                        return NotFound();
                    }
                    throw;
                }
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
                // Якщо видаляється активне бронювання, повертаємо місцю статус вільного
                if (reservation.Status == "Active")
                {
                    var spot = await _context.ParkingSpots.FindAsync(reservation.SpotID);
                    if (spot != null && spot.UserID == reservation.AttendantID)
                    {
                        spot.Status = "Free";
                        spot.UserID = null;
                        _context.Update(spot);
                    }
                }

                _context.Reservations.Remove(reservation);
                await _context.SaveChangesAsync();
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
