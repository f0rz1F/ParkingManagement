# ParkingManagement

Система для управління Attendants, ParkingSpots, Reservations з MVC-адмін-панеллю та MAUI-застосунком.

Предметна область: система управління парковкою (варіант 4).
Таблиці:
- Attendants (Users) - працівники парковки
- ParkingSpots (Items) - паркувальні місця
- Reservations (Actions) - бронювання місць

## Структура проекту

- Server/ServerApp - серверна частина (ASP.NET Core, .NET 8)
- Client/ClientApp - клієнтська частина (.NET MAUI, .NET 8)
  - ViewModels - папка для моделей представлення (MVVM)

## Інструкція по запуску

Вимоги:
- Visual Studio 2022
- .NET 8 SDK
- Workloads: ASP.NET and web development, .NET MAUI

Запуск серверної частини:
1. Відкрити консоль у папці проекту або запустити через dotnet:
   dotnet run --project Server/ServerApp/ServerApp.csproj
2. Сервер запуститься і поверне базову сторінку на вказаному порту.

Запуск клієнтської частини:
1. Відкрити Client/ClientApp/ClientApp.csproj у Visual Studio 2022
2. Обрати цільовий пристрій (Android Emulator або Windows Machine)
3. Запустити проект через F5
