using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerApp.Models;
using ServerApp.Models.DTOs;
using ServerApp.Services;

namespace ServerApp.Controllers.Api
{
    // REST API контролер для бронювань (Actions)
    [ApiController]
    [Route("api/actions")]
    [Route("api/reservations")]
    [Authorize] // Захист доступу через JWT
    public class ActionsApiController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ActionsApiController(AppDbContext context)
        {
            _context = context;
        }

        // Отримання списку бронювань поточного користувача (GET: /api/actions)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReservationDto>>> GetReservations()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isUser = int.TryParse(userIdString, out var userId);
            var isAdmin = User.IsInRole("Admin");

            var query = _context.Reservations
                .AsNoTracking()
                .Include(r => r.ParkingSpot)
                .Include(r => r.Attendant)
                .AsQueryable();

            // Якщо запит від звичайного користувача, показуємо лише його бронювання
            if (isUser && !isAdmin)
            {
                query = query.Where(r => r.AttendantID == userId);
            }

            var list = await query
                .OrderByDescending(r => r.ReservationDate)
                .Select(r => new ReservationDto
                {
                    ID = r.ID,
                    SpotID = r.SpotID,
                    SpotNumber = r.ParkingSpot != null ? r.ParkingSpot.Number : 0,
                    AttendantID = r.AttendantID,
                    AttendantName = r.Attendant != null ? r.Attendant.Name : string.Empty,
                    ReservationDate = r.ReservationDate,
                    Status = r.Status
                })
                .ToListAsync();

            return Ok(list);
        }

        // Створення нового бронювання паркомісця (POST: /api/actions)
        [HttpPost]
        public async Task<ActionResult<ReservationDto>> CreateReservation([FromBody] CreateReservationDto dto)
        {
            if (dto.SpotID <= 0)
            {
                return BadRequest(new { message = "Некоректний ідентифікатор паркомісця" });
            }

            // Бронювати можуть лише працівники (у токені адміністратора немає ID працівника)
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out var attendantId) || attendantId <= 0)
            {
                return BadRequest(new { message = "Бронювати місця через API можуть лише працівники" });
            }

            var attendant = await _context.Attendants.FindAsync(attendantId);
            if (attendant == null)
            {
                return BadRequest(new { message = "Користувача не знайдено" });
            }

            // Перевірка існування паркомісця
            var spot = await _context.ParkingSpots.AsNoTracking().FirstOrDefaultAsync(s => s.ID == dto.SpotID);
            if (spot == null)
            {
                return NotFound(new { message = "Паркувальне місце не знайдено" });
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();

            // Атомарне захоплення місця: оновлення спрацює лише якщо місце ще вільне.
            // Якщо двоє натиснуть "Бронювати" одночасно, другий отримає 0 оновлених рядків.
            var captured = await _context.ParkingSpots
                .Where(s => s.ID == dto.SpotID && s.Status == SpotStateSync.SpotFree)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(s => s.Status, SpotStateSync.SpotOccupied)
                    .SetProperty(s => s.UserID, (int?)attendant.ID));

            if (captured == 0)
            {
                return BadRequest(new { message = "Це паркувальне місце вже зайняте" });
            }

            // Створення запису бронювання
            var reservation = new Reservation
            {
                SpotID = spot.ID,
                AttendantID = attendant.ID,
                ReservationDate = DateTime.Now,
                Status = SpotStateSync.ReservationActive
            };

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            var resultDto = new ReservationDto
            {
                ID = reservation.ID,
                SpotID = spot.ID,
                SpotNumber = spot.Number,
                AttendantID = attendant.ID,
                AttendantName = attendant.Name,
                ReservationDate = reservation.ReservationDate,
                Status = reservation.Status
            };

            return CreatedAtAction(nameof(GetReservations), new { id = reservation.ID }, resultDto);
        }

        // Скасування активного бронювання та звільнення місця (DELETE: /api/actions/{spotId})
        [HttpDelete("{spotId:int}")]
        [HttpPost("cancel/{spotId:int}")]
        public async Task<IActionResult> CancelReservation(int spotId)
        {
            var spot = await _context.ParkingSpots.FindAsync(spotId);
            if (spot == null)
            {
                return NotFound(new { message = "Паркувальне місце не знайдено" });
            }

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(userIdString, out var currentUserId);
            var isAdmin = User.IsInRole("Admin");

            // Перевіряємо, чи має право користувач звільнити місце
            if (!isAdmin && spot.UserID != currentUserId)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "Ви можете скасувати лише власне бронювання" });
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();

            // Скасування всіх активних бронювань цього місця
            var activeReservations = await _context.Reservations
                .Where(r => r.SpotID == spotId && r.Status == SpotStateSync.ReservationActive)
                .ToListAsync();

            foreach (var reservation in activeReservations)
            {
                reservation.Status = SpotStateSync.ReservationCancelled;
            }
            await _context.SaveChangesAsync();

            // Звільнення місця (перерахунок з бронювань)
            await SpotStateSync.SyncAsync(_context, spotId);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new { message = "Бронювання успішно скасовано, місце звільнено" });
        }
    }
}
