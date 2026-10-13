ParkingManagement
Система для управління Attendants, ParkingSpots, Reservations з MVC-адмін-панеллю та MAUI-застосунком.

Предметна область: система управління парковкою (варіант 4).
Таблиці:
- Attendants (Users) - працівники парковки
- ParkingSpots (Items) - паркувальні місця
- Reservations (Actions) - бронювання місць

Структура проекту

- Database - схема бази даних init.sql
- Server/ServerApp - серверна частина (ASP.NET Core, .NET 8, EF Core SQLite)
- Client/ClientApp - клієнтська частина (.NET MAUI, .NET 8)
  - ViewModels - папка для моделей представлення (MVVM)

Інструкція по запуску

Вимоги:
- Visual Studio
- .NET 8 SDK
- Workloads: ASP.NET and web development, .NET MAUI

Запуск серверної частини:
1. Скопіювати файл налаштувань:
   Скопіювати Server/ServerApp/appsettings.example.json як Server/ServerApp/appsettings.json
2. Застосувати міграції для створення бази даних SQLite:
   dotnet ef database update --project Server/ServerApp/ServerApp.csproj
   (створиться файл бази parking.db з початковими тестовими даними)
3. Запустити сервер:
   dotnet run --project Server/ServerApp/ServerApp.csproj
   Після запуску доступна сторінка входу (/auth/login) та повноцінна MVC адмін-панель (/admin) для керування працівниками, місцями та бронюваннями (CRUD).
   Облікові дані адміністратора: admin@parking.com / AdminPassword123!

Запуск клієнтської частини:
1. Відкрити Client/ClientApp/ClientApp.csproj у Visual Studio
2. Обрати цільовий пристрій (Android Emulator або Windows Machine)
3. Запустити проект через F5
