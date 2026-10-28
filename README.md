# ParkingManagement — Система управління паркувальними місцями

Навчальний проект курсу з розробки програмного забезпечення на платформі .NET 8 (Варіант 4).

Проект складається з серверної частини на **ASP.NET Core** (MVC адмін-панель + REST API) та клієнтського мультиплатформного застосунку на **.NET MAUI** з патерном **MVVM**.

---

## 📋 Предметна область (Варіант 4)

Система автоматизації управління паркувальними місцями та обліку бронювань:
* **Attendants (Users)** — працівники парковки (`ID`, `Name`, `Email`, `PasswordHash`).
* **ParkingSpots (Items)** — паркувальні місця (`ID`, `Number`, `Status` [`Free`/`Occupied`], `UserID`).
* **Reservations (Actions)** — бронювання місць (`ID`, `AttendantID`, `SpotID`, `ReservationDate`, `Status` [`Active`/`Cancelled`]).

### Сценарій використання:
1. **Адміністратор** через веб-інтерфейс (MVC адмін-панель) здійснює повне керування обліковими записами працівників, паркомісцями та журналом бронювань (CRUD).
2. **Працівник (Sasha Yaschenko)** через клієнтський застосунок (.NET MAUI) авторизується за допомогою JWT, переглядає стан паркомісць у реальному часі, бронює вільні місця та за потреби скасовує власні бронювання.

---

## 🏛️ Архітектура проекту

* **Сервер (`Server/ServerApp`):**
  * **ASP.NET Core (.NET 8)** з підтримкою MVC та REST API.
  * **Entity Framework Core (SQLite)** — об'єктно-реляційне відображення, міграції та оптимізовані індекси.
  * **Безпека:** надійне хешування паролів за стандартом **BCrypt**, автентифікація через **JWT (JSON Web Token)** із ролями `Admin` та `User`.
  * **Документація:** інтерактивний **Swagger UI** (`/swagger`).
* **Клієнт (`Client/ClientApp`):**
  * **.NET MAUI** (підтримка Windows та Android).
  * Архітектурний патерн **MVVM** на базі бібліотеки `CommunityToolkit.Mvvm`.
  * Взаємодія з сервером через клієнт `HttpClient` у сервісі `ApiService`.
  * Локальне збереження токена сесії в сховищі `Preferences`.
  * Кешування даних у пам'яті клієнта для оптимізації переходів.
* **База даних (`Database/` та `Server/ServerApp/parking.db`):**
  * Реляційна СУБД SQLite з налаштованими зовнішніми ключами (Foreign Keys) та індексами для прискорення вибірок.
* **Документація та тести (`Docs/`):**
  * `Docs/swagger.json` — експорт OpenAPI-специфікації REST API.
  * `Docs/init.sql` — повний DDL-скрипт бази даних з індексами та тестовими даними.
  * `Docs/ParkingManagement.postman_collection.json` — колекція готових запитів для тестування в Postman.
  * `Docs/TESTING.md` — детальний звіт результатів тестування зі зведеною таблицею.
  * `Docs/Screenshots/` — скріншоти тестування веб-панелі, Swagger UI та MAUI клієнта.

---

## 🔑 Облікові дані для тестування

| Роль | Інтерфейс | Email / Логін | Пароль |
|---|---|---|---|
| **Адміністратор** | Веб-адмінка (`/auth/login`) | `admin@parking.com` | `AdminPassword123!` |
| **Працівник** | MAUI клієнт та API | `sasha@parking.com` | `Password123!` |
| **Працівник 2** | API тестування | `olena@parking.com` | `Password123!` |
| **Працівник 3** | API тестування | `maksym@parking.com` | `Password123!` |

---

## 🌐 Специфікація REST API

| Метод | Маршрут | Опис | Авторизація |
|---|---|---|:---:|
| `POST` | `/api/auth/login` | Аутентифікація працівника та видача JWT-токена | Публічний |
| `GET` | `/api/items` (або `/api/spots`) | Отримання списку всіх паркомісць та їх статусів | Bearer JWT |
| `GET` | `/api/items/{id}` | Отримання детальної інформації про одне місце | Bearer JWT |
| `GET` | `/api/actions` | Отримання списку бронювань поточного користувача | Bearer JWT |
| `POST` | `/api/actions` | Створення нового бронювання вільного паркомісця | Bearer JWT |
| `DELETE`| `/api/actions/{spotId}` | Скасування бронювання та звільнення паркомісця | Bearer JWT |
| `GET` | `/swagger` | Інтерактивна веб-документація Swagger UI | Публічний |

---

## 🚀 Інструкція із запуску

### Системні вимоги:
* **Visual Studio** з підтримкою **.NET 8 SDK** (версія 8.0.x) та робочими навантаженнями:
  * *ASP.NET and web development*
  * *.NET Multi-platform App UI development*
* Інструмент Entity Framework Core CLI (`dotnet ef`).

### 1. Перше налаштування та запуск серверної частини:
1. Перейдіть до каталогу сервера та створіть файл конфігурації:
   ```powershell
   cd Server/ServerApp
   copy appsettings.example.json appsettings.json
   ```
2. Застосуйте міграції EF Core для автоматичного створення бази даних SQLite (`parking.db`) та заповнення тестовими даними:
   ```powershell
   dotnet ef database update
   ```
3. Запустіть сервер:
   * Відкрийте `Server/ServerApp/ServerApp.sln` у **Visual Studio** та натисніть **F5** (або виконайте у терміналі: `dotnet run`).
   * Веб-інтерфейс адміністратора: 👉 `http://localhost:5088/auth/login`
   * Інтерактивна документація Swagger UI: 👉 `http://localhost:5088/swagger`

### 2. Запуск клієнтського MAUI-додатку:
1. Відкрийте `Client/ClientApp/ClientApp.sln` у **Visual Studio**.
2. Оберіть цільову платформу (**Windows Machine** або емулятор **Android**) та натисніть **F5**.
3. Для емулятора Android клієнт автоматично використовує адресу `http://10.0.2.2:5088` з увімкненим дозволом `usesCleartextTraffic`.

### 3. Тестування системи:
* **Тестування API та безпеки:**
  * Через Swagger UI: `http://localhost:5088/swagger`
  * Через Postman: імпорт файлу [`Docs/ParkingManagement.postman_collection.json`](Docs/ParkingManagement.postman_collection.json)
* **Керування сутностями:** через веб-адмінку `http://localhost:5088/auth/login`.
* **Клієнтський інтерфейс:** через додаток .NET MAUI — вхід під `sasha@parking.com`, перегляд місць, бронювання та скасування в 1 клік.
* Детальний звіт та скріншоти тестів зібрані у файлі [`Docs/TESTING.md`](Docs/TESTING.md).
