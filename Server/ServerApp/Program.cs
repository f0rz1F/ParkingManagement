using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ServerApp.Models;

var builder = WebApplication.CreateBuilder(args);

// Підключення бази даних SQLite через AppDbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Налаштування JWT-аутентифікації
var jwtKey = builder.Configuration["Jwt:Key"] ?? "ParkingManagementSuperSecretKey2026!Key";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "ParkingServer";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "ParkingClient";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    // Параметри валідації токена
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };

    // Обробка подій аутентифікації
    options.Events = new JwtBearerEvents
    {
        // Отримання токена з кукі jwt_token
        OnMessageReceived = context =>
        {
            if (context.Request.Cookies.ContainsKey("jwt_token"))
            {
                context.Token = context.Request.Cookies["jwt_token"];
            }
            return Task.CompletedTask;
        },
        // Перенаправлення на сторінку входу при відсутності доступу
        OnChallenge = context =>
        {
            if (!context.Request.Headers.Accept.ToString().Contains("application/json"))
            {
                context.HandleResponse();
                context.Response.Redirect("/auth/login");
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization(); // Авторизація на основі ролей

// Підтримка MVC контролерів та представлень
builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles(); // Обробка статичних файлів
app.UseRouting();

app.UseAuthentication(); // Аутентифікація користувача
app.UseAuthorization();  // Авторизація та перевірка прав доступу

// Налаштування маршрутизації MVC контролерів за замовчуванням
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Admin}/{action=Index}/{id?}");

app.MapControllers(); // Маршрутизація атрибутних контролерів

app.Run();