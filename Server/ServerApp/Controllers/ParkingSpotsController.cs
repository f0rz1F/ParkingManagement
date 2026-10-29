using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ServerApp.Models;
using ServerApp.Services;

namespace ServerApp.Controllers
{
    // Контролер управління паркувальними місцями (Items CRUD)
    [Authorize(Roles = "Admin")]
    public class ParkingSpotsController : Controller
    {
        private readonly AppDbContext _context;

        public ParkingSpotsController(AppDbContext context)
        {
            _context = context;
        }

        // Отримання списку всіх паркомісць (GET: /ParkingSpots)
        [HttpGet("/ParkingSpots")]
        [HttpGet("/Items")]
        public async Task<IActionResult> Index()
        {
            var spots = await _context.ParkingSpots
                .AsNoTracking()
                .Include(p => p.Attendant)
                .OrderBy(p => p.Number)
                .ToListAsync();
            return View(spots);
        }

        // Відображення форми створення нового паркомісця (GET: /ParkingSpots/Create)
        [HttpGet("/ParkingSpots/Create")]
        [HttpGet("/Items/Create")]
        public async Task<IActionResult> Create()
        {
            await PopulateAttendantsDropDownList();
            return View();
        }

        // Збереження нового паркомісця в базі даних (POST: /ParkingSpots/Create)
        [HttpPost("/ParkingSpots/Create")]
        [HttpPost("/Items/Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Number,Status,UserID")] ParkingSpot spot)
        {
            ValidateStatus(spot);

            if (ModelState.IsValid)
            {
                // Перевірка унікальності номера місця
                var numberExists = await _context.ParkingSpots.AnyAsync(p => p.Number == spot.Number);
                if (numberExists)
                {
                    ModelState.AddModelError("Number", "Паркомісце з таким номером уже існує");
                    await PopulateAttendantsDropDownList(spot.UserID);
                    return View(spot);
                }

                await using var transaction = await _context.Database.BeginTransactionAsync();

                var requestedStatus = spot.Status;
                var requestedUserId = spot.UserID;
                spot.Status = SpotStateSync.SpotFree;
                spot.UserID = null;
                _context.Add(spot);
                await _context.SaveChangesAsync();

                // Зайняте місце = активне бронювання на обраного працівника
                await ApplyRequestedStateAsync(spot.ID, requestedStatus, requestedUserId);
                await transaction.CommitAsync();

                return RedirectToAction(nameof(Index));
            }
            await PopulateAttendantsDropDownList(spot.UserID);
            return View(spot);
        }

        // Відображення форми редагування паркомісця (GET: /ParkingSpots/Edit/5)
        [HttpGet("/ParkingSpots/Edit/{id:int}")]
        [HttpGet("/Items/Edit/{id:int}")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var spot = await _context.ParkingSpots.FindAsync(id);
            if (spot == null)
            {
                return NotFound();
            }

            await PopulateAttendantsDropDownList(spot.UserID);
            return View(spot);
        }

        // Збереження змін паркомісця (POST: /ParkingSpots/Edit/5)
        [HttpPost("/ParkingSpots/Edit/{id:int}")]
        [HttpPost("/Items/Edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Number,Status,UserID")] ParkingSpot spot)
        {
            if (id != spot.ID)
            {
                return NotFound();
            }

            ValidateStatus(spot);

            if (ModelState.IsValid)
            {
                // Перевірка унікальності номера місця серед інших записів
                var numberExists = await _context.ParkingSpots.AnyAsync(p => p.Number == spot.Number && p.ID != id);
                if (numberExists)
                {
                    ModelState.AddModelError("Number", "Паркомісце з таким номером уже існує");
                    await PopulateAttendantsDropDownList(spot.UserID);
                    return View(spot);
                }

                var existing = await _context.ParkingSpots.FindAsync(id);
                if (existing == null)
                {
                    return NotFound();
                }

                await using var transaction = await _context.Database.BeginTransactionAsync();

                existing.Number = spot.Number;
                await _context.SaveChangesAsync();

                // Статус і працівник змінюються лише через бронювання, щоб дані не розходились
                await ApplyRequestedStateAsync(id, spot.Status, spot.UserID);
                await transaction.CommitAsync();

                return RedirectToAction(nameof(Index));
            }
            await PopulateAttendantsDropDownList(spot.UserID);
            return View(spot);
        }

        // Перевірка значень статусу та працівника з форми
        private void ValidateStatus(ParkingSpot spot)
        {
            if (!SpotStateSync.SpotStatuses.Contains(spot.Status))
            {
                ModelState.AddModelError("Status", "Некоректний статус місця");
            }
            else if (spot.Status == SpotStateSync.SpotOccupied && spot.UserID == null)
            {
                ModelState.AddModelError("UserID", "Для зайнятого місця оберіть працівника");
            }
        }

        // Приведення бронювань місця до обраного в адмінці стану:
        //   Free     -> усі активні бронювання місця скасовуються;
        //   Occupied -> якщо місце вже за цим працівником, нічого не змінюється,
        //               інакше старі активні бронювання скасовуються і створюється нове.
        private async Task ApplyRequestedStateAsync(int spotId, string status, int? userId)
        {
            var activeReservations = await _context.Reservations
                .Where(r => r.SpotID == spotId && r.Status == SpotStateSync.ReservationActive)
                .ToListAsync();

            var keepCurrent = status == SpotStateSync.SpotOccupied &&
                              activeReservations.Count == 1 &&
                              activeReservations[0].AttendantID == userId;

            if (!keepCurrent)
            {
                foreach (var reservation in activeReservations)
                {
                    reservation.Status = SpotStateSync.ReservationCancelled;
                }

                if (status == SpotStateSync.SpotOccupied && userId != null)
                {
                    _context.Reservations.Add(new Reservation
                    {
                        SpotID = spotId,
                        AttendantID = userId.Value,
                        ReservationDate = DateTime.Now,
                        Status = SpotStateSync.ReservationActive
                    });
                }

                await _context.SaveChangesAsync();
            }

            await SpotStateSync.SyncAsync(_context, spotId);
            await _context.SaveChangesAsync();
        }

        // Відображення сторінки підтвердження видалення (GET: /ParkingSpots/Delete/5)
        [HttpGet("/ParkingSpots/Delete/{id:int}")]
        [HttpGet("/Items/Delete/{id:int}")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var spot = await _context.ParkingSpots
                .Include(p => p.Attendant)
                .Include(p => p.Reservations)
                .FirstOrDefaultAsync(m => m.ID == id);

            if (spot == null)
            {
                return NotFound();
            }

            return View(spot);
        }

        // Видалення паркомісця з бази даних (POST: /ParkingSpots/Delete/5)
        [HttpPost("/ParkingSpots/Delete/{id:int}")]
        [HttpPost("/Items/Delete/{id:int}")]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var spot = await _context.ParkingSpots.FindAsync(id);
            if (spot != null)
            {
                _context.ParkingSpots.Remove(spot);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // Формування випадаючого списку працівників
        private async Task PopulateAttendantsDropDownList(object? selectedAttendant = null)
        {
            var attendants = await _context.Attendants.OrderBy(a => a.Name).ToListAsync();
            ViewBag.UserID = new SelectList(attendants, "ID", "Name", selectedAttendant);
        }
    }
}
