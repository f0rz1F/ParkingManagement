using Microsoft.EntityFrameworkCore;

namespace ServerApp.Models
{
    // Контекст бази даних SQLite
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Таблиці бази даних
        public DbSet<Attendant> Attendants { get; set; }     // Працівники
        public DbSet<ParkingSpot> ParkingSpots { get; set; } // Паркувальні місця
        public DbSet<Reservation> Reservations { get; set; } // Бронювання

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Унікальні індекси
            modelBuilder.Entity<Attendant>()
                .HasIndex(a => a.Email)
                .IsUnique();

            modelBuilder.Entity<ParkingSpot>()
                .HasIndex(p => p.Number)
                .IsUnique();

            // Зв'язки між сутностями:
            // ParkingSpot -> Attendant (UserID)
            modelBuilder.Entity<ParkingSpot>()
                .HasOne(p => p.Attendant)
                .WithMany(a => a.ParkingSpots)
                .HasForeignKey(p => p.UserID)
                .OnDelete(DeleteBehavior.SetNull);

            // Reservation -> Attendant (AttendantID)
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Attendant)
                .WithMany(a => a.Reservations)
                .HasForeignKey(r => r.AttendantID)
                .OnDelete(DeleteBehavior.Cascade);

            // Reservation -> ParkingSpot (SpotID)
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.ParkingSpot)
                .WithMany(p => p.Reservations)
                .HasForeignKey(r => r.SpotID)
                .OnDelete(DeleteBehavior.Cascade);

            // Початкові тестові дані (Seed data)
            modelBuilder.Entity<Attendant>().HasData(
                new Attendant { ID = 1, Name = "Ivan Petrenko", Email = "ivan@parking.com", PasswordHash = "AQAAAAIAAYagAAAAELkR+DH3s1xMKYGJqo5MhFP1x2a4Bz5KJ2E6TQKZ8cO0vH1nV+uLz0E6vPxjG0YRA==" },
                new Attendant { ID = 2, Name = "Olena Koval", Email = "olena@parking.com", PasswordHash = "AQAAAAIAAYagAAAAELkR+DH3s1xMKYGJqo5MhFP1x2a4Bz5KJ2E6TQKZ8cO0vH1nV+uLz0E6vPxjG0YRA==" },
                new Attendant { ID = 3, Name = "Maksym Shevchenko", Email = "maksym@parking.com", PasswordHash = "AQAAAAIAAYagAAAAELkR+DH3s1xMKYGJqo5MhFP1x2a4Bz5KJ2E6TQKZ8cO0vH1nV+uLz0E6vPxjG0YRA==" }
            );

            modelBuilder.Entity<ParkingSpot>().HasData(
                new ParkingSpot { ID = 1, Number = 1, Status = "Free", UserID = null },
                new ParkingSpot { ID = 2, Number = 2, Status = "Occupied", UserID = 1 },
                new ParkingSpot { ID = 3, Number = 3, Status = "Free", UserID = null },
                new ParkingSpot { ID = 4, Number = 4, Status = "Occupied", UserID = 2 },
                new ParkingSpot { ID = 5, Number = 5, Status = "Free", UserID = null },
                new ParkingSpot { ID = 6, Number = 6, Status = "Free", UserID = null },
                new ParkingSpot { ID = 7, Number = 7, Status = "Free", UserID = null },
                new ParkingSpot { ID = 8, Number = 8, Status = "Occupied", UserID = 3 },
                new ParkingSpot { ID = 9, Number = 9, Status = "Free", UserID = null },
                new ParkingSpot { ID = 10, Number = 10, Status = "Free", UserID = null }
            );

            modelBuilder.Entity<Reservation>().HasData(
                new Reservation { ID = 1, AttendantID = 1, SpotID = 2, ReservationDate = new DateTime(2026, 9, 1), Status = "Active" },
                new Reservation { ID = 2, AttendantID = 2, SpotID = 4, ReservationDate = new DateTime(2026, 9, 2), Status = "Active" },
                new Reservation { ID = 3, AttendantID = 3, SpotID = 8, ReservationDate = new DateTime(2026, 9, 2), Status = "Active" }
            );
        }
    }
}
