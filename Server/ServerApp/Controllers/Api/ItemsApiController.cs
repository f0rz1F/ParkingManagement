using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerApp.Models;
using ServerApp.Models.DTOs;

namespace ServerApp.Controllers.Api
{
    // REST API контролер для паркувальних місць (Items)
    [ApiController]
    [Route("api/items")]
    [Route("api/spots")]
    [Authorize] // Захист доступу через JWT
    public class ItemsApiController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ItemsApiController(AppDbContext context)
        {
            _context = context;
        }

        // Отримання списку всіх паркомісць (GET: /api/items або /api/spots)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SpotDto>>> GetSpots([FromQuery] bool? freeOnly)
        {
            var query = _context.ParkingSpots
                .AsNoTracking()
                .Include(s => s.Attendant)
                .AsQueryable();

            if (freeOnly == true)
            {
                query = query.Where(s => s.Status == "Free");
            }

            var spots = await query
                .OrderBy(s => s.Number)
                .Select(s => new SpotDto
                {
                    ID = s.ID,
                    Number = s.Number,
                    Status = s.Status,
                    UserID = s.UserID,
                    AttendantName = s.Attendant != null ? s.Attendant.Name : null
                })
                .ToListAsync();

            return Ok(spots);
        }

        // Отримання інформації про окреме паркомісце за ID (GET: /api/items/5)
        [HttpGet("{id:int}")]
        public async Task<ActionResult<SpotDto>> GetSpotById(int id)
        {
            var spot = await _context.ParkingSpots
                .Include(s => s.Attendant)
                .FirstOrDefaultAsync(s => s.ID == id);

            if (spot == null)
            {
                return NotFound(new { message = "Паркомісце не знайдено" });
            }

            var dto = new SpotDto
            {
                ID = spot.ID,
                Number = spot.Number,
                Status = spot.Status,
                UserID = spot.UserID,
                AttendantName = spot.Attendant != null ? spot.Attendant.Name : null
            };

            return Ok(dto);
        }
    }
}
