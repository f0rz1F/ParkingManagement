using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ServerApp.Models;

var builder = WebApplication.CreateBuilder(args);

// Підключення SQLite бази даних
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

    // Читання токена з кукі браузера або із заголовка Authorization
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            if (context.Request.Cookies.ContainsKey("jwt_token"))
            {
                context.Token = context.Request.Cookies["jwt_token"];
            }
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            // Перенаправлення браузера на форму входу при відсутності доступу
            if (!context.Request.Headers.Accept.ToString().Contains("application/json"))
            {
                context.HandleResponse();
                context.Response.Redirect("/auth/login");
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// Підтримка контролерів та представлень (MVC)
builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => "Система управління парковкою");

app.MapControllers();

app.Run();