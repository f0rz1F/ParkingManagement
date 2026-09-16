-- Система управлiння парковкою
-- База даних: SQLite

-- Таблиця працiвникiв парковки (Users)
CREATE TABLE IF NOT EXISTS Attendants (
    ID INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    Email TEXT NOT NULL UNIQUE,
    PasswordHash TEXT NOT NULL
);

-- Таблиця паркувальних мiсць (Items)
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

-- Тестовi данi

-- Працiвники (пароль: Password123!)
INSERT INTO Attendants (Name, Email, PasswordHash) VALUES
('Ivan Petrenko', 'ivan@parking.com', 'AQAAAAIAAYagAAAAELkR+DH3s1xMKYGJqo5MhFP1x2a4Bz5KJ2E6TQKZ8cO0vH1nV+uLz0E6vPxjG0YRA=='),
('Olena Koval', 'olena@parking.com', 'AQAAAAIAAYagAAAAELkR+DH3s1xMKYGJqo5MhFP1x2a4Bz5KJ2E6TQKZ8cO0vH1nV+uLz0E6vPxjG0YRA=='),
('Maksym Shevchenko', 'maksym@parking.com', 'AQAAAAIAAYagAAAAELkR+DH3s1xMKYGJqo5MhFP1x2a4Bz5KJ2E6TQKZ8cO0vH1nV+uLz0E6vPxjG0YRA==');

-- Паркувальнi мiсця
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
