using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using ServerApp.Models;

namespace ServerApp.Controllers
{
    // Контролер аутентифікації адміністратора
    public class AuthController : Controller
    {
        private readonly IConfiguration _configuration;

        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // Відображення сторінки входу (GET: /auth/login)
        [HttpGet("/auth/login")]
        public IActionResult Login()
        {
            return View();
        }

        // Аутентифікація адміністратора та видача токена (POST: /auth/login)
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
            var tokenString = GenerateJwtToken(model.Email, "Admin");

            // Збереження токена в кукі jwt_token
            Response.Cookies.Append("jwt_token", tokenString, new CookieOptions
            {
                HttpOnly = true, // Захист від XSS (доступ лише через HTTP)
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

        // Вихід із системи та видалення токена (GET: /auth/logout)
        [HttpGet("/auth/logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("jwt_token");
            return RedirectToAction("Login");
        }

        // Генерація JWT-токена з цифровим підписом
        private string GenerateJwtToken(string email, string role)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "ParkingManagementSuperSecretKey2026!Key");

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                // Claims токена: email та роль
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, email),
                    new Claim(ClaimTypes.Email, email),
                    new Claim(ClaimTypes.Role, role)
                }),
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