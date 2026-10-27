using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ServerApp.Models;
using ServerApp.Models.DTOs;

namespace ServerApp.Controllers
{
    // Контролер аутентифікації адміністратора та API-користувачів
    public class AuthController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;

        public AuthController(IConfiguration configuration, AppDbContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        // Відображення сторінки входу (GET: /auth/login)
        [HttpGet("/auth/login")]
        public IActionResult Login()
        {
            return View();
        }

        // Аутентифікація адміністратора через веб-форму (POST: /auth/login)
        [HttpPost("/auth/login")]
        public IActionResult Login([FromForm] LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var adminEmail = _configuration["Admin:Email"] ?? "admin@parking.com";
            var adminPassword = _configuration["Admin:Password"] ?? "AdminPassword123!";

            // Перевірка облікових даних адміністратора
            if (model.Email != adminEmail || model.Password != adminPassword)
            {
                ModelState.AddModelError(string.Empty, "Невірний email або пароль адміністратора");
                return View(model);
            }

            // Генерація JWT-токена з роллю Admin
            var tokenString = GenerateJwtToken(model.Email, "Admin", 0, "Адміністратор");

            // Збереження токена в кукі jwt_token
            Response.Cookies.Append("jwt_token", tokenString, new CookieOptions
            {
                HttpOnly = true, // Захист від XSS
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddHours(8)
            });

            // Повернення токена у форматі JSON для API-запитів
            if (Request.Headers.Accept.ToString().Contains("application/json"))
            {
                return Ok(new { token = tokenString, email = model.Email, role = "Admin" });
            }

            // Перенаправлення до панелі адміністратора
            return RedirectToAction("Index", "Admin");
        }

        // Аутентифікація користувачів (Users / Attendants) через REST API (POST: /api/auth/login)
        [HttpPost("/api/auth/login")]
        public async Task<IActionResult> ApiLogin([FromBody] UserLoginDto model)
        {
            if (string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Password))
            {
                return BadRequest(new { message = "Email та пароль обов'язкові для заповнення" });
            }

            var adminEmail = _configuration["Admin:Email"] ?? "admin@parking.com";
            var adminPassword = _configuration["Admin:Password"] ?? "AdminPassword123!";

            // 1. Перевірка адміністратора
            if (model.Email.Equals(adminEmail, StringComparison.OrdinalIgnoreCase) && model.Password == adminPassword)
            {
                var adminToken = GenerateJwtToken(adminEmail, "Admin", 0, "Адміністратор");
                return Ok(new AuthResponseDto
                {
                    Token = adminToken,
                    UserID = 0,
                    Name = "Адміністратор",
                    Email = adminEmail,
                    Role = "Admin"
                });
            }

            // 2. Перевірка працівника (користувача) у базі даних
            var user = await _context.Attendants.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (user == null || !VerifyPassword(model.Password, user.PasswordHash))
            {
                return Unauthorized(new { message = "Невірний email або пароль" });
            }

            var userToken = GenerateJwtToken(user.Email, "User", user.ID, user.Name);
            return Ok(new AuthResponseDto
            {
                Token = userToken,
                UserID = user.ID,
                Name = user.Name,
                Email = user.Email,
                Role = "User"
            });
        }

        // Вихід із системи та видалення токена (GET: /auth/logout)
        [HttpGet("/auth/logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("jwt_token");
            return RedirectToAction("Login");
        }

        // Перевірка відповідності пароля його BCrypt хешу
        private static bool VerifyPassword(string enteredPassword, string storedHash)
        {
            if (string.IsNullOrEmpty(storedHash))
            {
                return false;
            }

            try
            {
                return BCrypt.Net.BCrypt.Verify(enteredPassword, storedHash);
            }
            catch
            {
                return false;
            }
        }

        // Генерація JWT-токена з цифровим підписом
        private string GenerateJwtToken(string email, string role, int? userId = null, string? name = null)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "ParkingManagementSuperSecretKey2026!Key");

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, name ?? email),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, role)
            };

            if (userId.HasValue)
            {
                claims.Add(new Claim(ClaimTypes.NameIdentifier, userId.Value.ToString()));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(8), // Термін дії токена
                Issuer = _configuration["Jwt:Issuer"] ?? "ParkingServer",
                Audience = _configuration["Jwt:Audience"] ?? "ParkingClient",
                // Підпис токена симетричним ключем
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
