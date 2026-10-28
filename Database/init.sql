-- Опис полів таблиць:
-- 1. Attendants (Працівники парковки / Користувачі):
--    - ID: унікальний номер працівника (первинний ключ)
--    - Name: ім'я та прізвище працівника
--    - Email: електронна пошта (використовується як логін для входу)
--    - PasswordHash: пароль у захешованому вигляді для безпеки
--
-- 2. ParkingSpots (Паркувальні місця):
--    - ID: унікальний номер запису паркомісця
--    - Number: номер паркувального місця на парковці (1, 2, 3...)
--    - Status: поточний стан місця ('Free' - вільне, 'Occupied' - зайняте)
--    - UserID: ID працівника, який обслуговує або закріплений за місцем
--
-- 3. Reservations (Бронювання місць):
--    - ID: унікальний номер бронювання
--    - AttendantID: ID працівника, який оформив це бронювання
--    - SpotID: ID паркувального місця, яке забронювали
--    - ReservationDate: дата та час створення броні
--    - Status: стан бронювання ('Active' - активне, 'Completed' - завершене, 'Cancelled' - скасоване)

-- Таблиця працівників парковки (Users)
CREATE TABLE IF NOT EXISTS Attendants (
    ID INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    Email TEXT NOT NULL UNIQUE,
    PasswordHash TEXT NOT NULL
);

-- Таблиця паркувальних місць (Items)
CREATE TABLE IF NOT EXISTS ParkingSpots (
    ID INTEGER PRIMARY KEY AUTOINCREMENT,
    Number INTEGER NOT NULL UNIQUE,
    Status TEXT NOT NULL DEFAULT 'Free' CHECK(Status IN ('Free', 'Occupied')),
    UserID INTEGER,
    FOREIGN KEY (UserID) REFERENCES Attendants(ID)
);

-- Таблиця бронювань (Actions)
CREATE TABLE IF NOT EXISTS Reservations (
    ID INTEGER PRIMARY KEY AUTOINCREMENT,
    AttendantID INTEGER NOT NULL,
    SpotID INTEGER NOT NULL,
    ReservationDate TEXT NOT NULL,
    Status TEXT NOT NULL DEFAULT 'Active' CHECK(Status IN ('Active', 'Completed', 'Cancelled')),
    FOREIGN KEY (AttendantID) REFERENCES Attendants(ID),
    FOREIGN KEY (SpotID) REFERENCES ParkingSpots(ID)
);

-- Індекси для оптимізації запитів (Лабораторна 11)
CREATE UNIQUE INDEX IF NOT EXISTS IX_Attendants_Email ON Attendants(Email);
CREATE INDEX IF NOT EXISTS IX_ParkingSpots_UserID ON ParkingSpots(UserID);
CREATE INDEX IF NOT EXISTS IX_Reservations_AttendantID ON Reservations(AttendantID);
CREATE INDEX IF NOT EXISTS IX_Reservations_SpotID ON Reservations(SpotID);
CREATE INDEX IF NOT EXISTS IX_Reservations_Status ON Reservations(Status);

-- Тестові дані

-- Працівники (пароль: Password123!, захешований через BCrypt)
INSERT INTO Attendants (Name, Email, PasswordHash) VALUES
('Sasha Yaschenko', 'sasha@parking.com', '$2a$11$BM8YzTfkyS/y.TmUYpQDaedXe5i8VV9DXgAP2tokr3X.QAKZ83Xd.'),
('Olena Koval', 'olena@parking.com', '$2a$11$BM8YzTfkyS/y.TmUYpQDaedXe5i8VV9DXgAP2tokr3X.QAKZ83Xd.'),
('Maksym Shevchenko', 'maksym@parking.com', '$2a$11$BM8YzTfkyS/y.TmUYpQDaedXe5i8VV9DXgAP2tokr3X.QAKZ83Xd.');

-- Паркувальні місця
INSERT INTO ParkingSpots (Number, Status, UserID) VALUES
(1, 'Free', NULL),
(2, 'Occupied', 1),
(3, 'Free', NULL),
(4, 'Occupied', 2),
(5, 'Free', NULL),
(6, 'Free', NULL),
(7, 'Free', NULL),
(8, 'Occupied', 3),
(9, 'Free', NULL),
(10, 'Free', NULL);

-- Бронювання
INSERT INTO Reservations (AttendantID, SpotID, ReservationDate, Status) VALUES
(1, 2, '2026-09-01', 'Active'),
(2, 4, '2026-09-02', 'Active'),
(3, 8, '2026-09-02', 'Active');