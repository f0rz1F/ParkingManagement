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
        // Перенаправлення на сторінку входу при відсутності доступу (крім API запитів)
        OnChallenge = context =>
        {
            if (!context.Request.Path.StartsWithSegments("/api") && !context.Request.Headers.Accept.ToString().Contains("application/json"))
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

// Налаштування Swagger документації API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "ParkingManagement API",
        Version = "v1",
        Description = "REST API для взаємодії з мобільним застосунком .NET MAUI"
    });

    // Підтримка передачі JWT Bearer токена в Swagger UI
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Введіть токен у форматі: Bearer {ваш_токен}"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Налаштування CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles(); // Обробка статичних файлів
app.UseRouting();

app.UseCors(); // Дозвіл крос-доменних запитів від клієнта

app.UseSwagger(); // Генерація специфікації OpenAPI (Swagger)
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ParkingManagement API v1");
    c.RoutePrefix = "swagger";
});

app.UseAuthentication(); // Аутентифікація користувача
app.UseAuthorization();  // Авторизація та перевірка прав доступу

// Налаштування маршрутизації MVC контролерів за замовчуванням
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Admin}/{action=Index}/{id?}");

app.MapControllers(); // Маршрутизація атрибутних контролерів

app.Run();