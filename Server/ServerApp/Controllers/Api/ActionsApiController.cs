using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerApp.Models;
using ServerApp.Models.DTOs;

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

            // Перевірка існування паркомісця
            var spot = await _context.ParkingSpots.FindAsync(dto.SpotID);
            if (spot == null)
            {
                return NotFound(new { message = "Паркувальне місце не знайдено" });
            }

            // Перевірка, чи вільне місце
            if (spot.Status != "Free")
            {
                return BadRequest(new { message = "Це паркувальне місце вже зайняте" });
            }

            // Визначення ID працівника з токена авторизації
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int attendantId;
            if (int.TryParse(userIdString, out var parsedId))
            {
                attendantId = parsedId;
            }
            else
            {
                // Якщо токен адміністратора, прив'язуємо до першого працівника для збереження цілісності
                var firstAttendant = await _context.Attendants.FirstOrDefaultAsync();
                if (firstAttendant == null)
                {
                    return BadRequest(new { message = "В системі відсутні зареєстровані працівники" });
                }
                attendantId = firstAttendant.ID;
            }

            var attendant = await _context.Attendants.FindAsync(attendantId);
            if (attendant == null)
            {
                return BadRequest(new { message = "Користувача не знайдено" });
            }

            // Створення запису бронювання
            var reservation = new Reservation
            {
                SpotID = spot.ID,
                AttendantID = attendant.ID,
                ReservationDate = DateTime.Now,
                Status = "Active"
            };

            // Оновлення статусу місця
            spot.Status = "Occupied";
            spot.UserID = attendant.ID;

            _context.Reservations.Add(reservation);
            _context.ParkingSpots.Update(spot);
            await _context.SaveChangesAsync();

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
                return Forbid();
            }

            // Оновлення статусу активного бронювання на Cancelled
            var activeReservation = await _context.Reservations
                .Where(r => r.SpotID == spotId && r.Status == "Active")
                .OrderByDescending(r => r.ReservationDate)
                .FirstOrDefaultAsync();

            if (activeReservation != null)
            {
                activeReservation.Status = "Cancelled";
                _context.Reservations.Update(activeReservation);
            }

            // Звільнення місця
            spot.Status = "Free";
            spot.UserID = null;
            _context.ParkingSpots.Update(spot);

            await _context.SaveChangesAsync();
            return Ok(new { message = "Бронювання успішно скасовано, місце звільнено" });
        }
    }
}
