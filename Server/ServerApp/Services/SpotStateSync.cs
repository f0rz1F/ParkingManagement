using Microsoft.EntityFrameworkCore;
using ServerApp.Models;

namespace ServerApp.Services
{
    // Узгодження стану паркомісця з бронюваннями.
    // Єдине джерело правди — таблиця Reservations:
    //   є активне бронювання  -> місце Occupied і закріплене за працівником цього бронювання;
    //   немає активного       -> місце Free і без працівника.
    // Викликається ПІСЛЯ SaveChangesAsync, бо читає вже збережені бронювання з бази.
    public static class SpotStateSync
    {
        public const string SpotFree = "Free";
        public const string SpotOccupied = "Occupied";
        public const string ReservationActive = "Active";
        public const string ReservationCompleted = "Completed";
        public const string ReservationCancelled = "Cancelled";

        public static readonly string[] SpotStatuses = { SpotFree, SpotOccupied };
        public static readonly string[] ReservationStatuses = { ReservationActive, ReservationCompleted, ReservationCancelled };

        // Перерахувати статус та закріплення для вказаних місць
        public static async Task SyncAsync(AppDbContext context, params int[] spotIds)
        {
            foreach (var spotId in spotIds.Distinct())
            {
                var spot = await context.ParkingSpots.FindAsync(spotId);
                if (spot == null)
                {
                    continue;
                }

                var active = await context.Reservations
                    .AsNoTracking()
                    .Where(r => r.SpotID == spotId && r.Status == ReservationActive)
                    .OrderByDescending(r => r.ReservationDate)
                    .FirstOrDefaultAsync();

                spot.Status = active != null ? SpotOccupied : SpotFree;
                spot.UserID = active?.AttendantID;
            }
        }

        // Чи є на місці інше активне бронювання (крім вказаного)
        public static Task<bool> HasOtherActiveReservationAsync(AppDbContext context, int spotId, int? exceptReservationId = null)
        {
            return context.Reservations.AnyAsync(r =>
                r.SpotID == spotId &&
                r.Status == ReservationActive &&
                (exceptReservationId == null || r.ID != exceptReservationId));
        }
    }
}
