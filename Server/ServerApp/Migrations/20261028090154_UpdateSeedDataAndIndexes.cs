using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServerApp.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSeedDataAndIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Attendants",
                keyColumn: "ID",
                keyValue: 1,
                columns: new[] { "Email", "Name", "PasswordHash" },
                values: new object[] { "sasha@parking.com", "Sasha Yaschenko", "$2a$11$BM8YzTfkyS/y.TmUYpQDaedXe5i8VV9DXgAP2tokr3X.QAKZ83Xd." });

            migrationBuilder.UpdateData(
                table: "Attendants",
                keyColumn: "ID",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$BM8YzTfkyS/y.TmUYpQDaedXe5i8VV9DXgAP2tokr3X.QAKZ83Xd.");

            migrationBuilder.UpdateData(
                table: "Attendants",
                keyColumn: "ID",
                keyValue: 3,
                column: "PasswordHash",
                value: "$2a$11$BM8YzTfkyS/y.TmUYpQDaedXe5i8VV9DXgAP2tokr3X.QAKZ83Xd.");

            migrationBuilder.Sql("DROP INDEX IF EXISTS IX_Reservations_Status;");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_Status",
                table: "Reservations",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Reservations_Status",
                table: "Reservations");

            migrationBuilder.UpdateData(
                table: "Attendants",
                keyColumn: "ID",
                keyValue: 1,
                columns: new[] { "Email", "Name", "PasswordHash" },
                values: new object[] { "ivan@parking.com", "Ivan Petrenko", "AQAAAAIAAYagAAAAELkR+DH3s1xMKYGJqo5MhFP1x2a4Bz5KJ2E6TQKZ8cO0vH1nV+uLz0E6vPxjG0YRA==" });

            migrationBuilder.UpdateData(
                table: "Attendants",
                keyColumn: "ID",
                keyValue: 2,
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAELkR+DH3s1xMKYGJqo5MhFP1x2a4Bz5KJ2E6TQKZ8cO0vH1nV+uLz0E6vPxjG0YRA==");

            migrationBuilder.UpdateData(
                table: "Attendants",
                keyColumn: "ID",
                keyValue: 3,
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAELkR+DH3s1xMKYGJqo5MhFP1x2a4Bz5KJ2E6TQKZ8cO0vH1nV+uLz0E6vPxjG0YRA==");
        }
    }
}
