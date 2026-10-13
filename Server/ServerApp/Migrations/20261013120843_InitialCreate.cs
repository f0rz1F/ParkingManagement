using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ServerApp.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Attendants",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attendants", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "ParkingSpots",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Number = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    UserID = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParkingSpots", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ParkingSpots_Attendants_UserID",
                        column: x => x.UserID,
                        principalTable: "Attendants",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Reservations",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AttendantID = table.Column<int>(type: "INTEGER", nullable: false),
                    SpotID = table.Column<int>(type: "INTEGER", nullable: false),
                    ReservationDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservations", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Reservations_Attendants_AttendantID",
                        column: x => x.AttendantID,
                        principalTable: "Attendants",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reservations_ParkingSpots_SpotID",
                        column: x => x.SpotID,
                        principalTable: "ParkingSpots",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Attendants",
                columns: new[] { "ID", "Email", "Name", "PasswordHash" },
                values: new object[,]
                {
                    { 1, "ivan@parking.com", "Ivan Petrenko", "AQAAAAIAAYagAAAAELkR+DH3s1xMKYGJqo5MhFP1x2a4Bz5KJ2E6TQKZ8cO0vH1nV+uLz0E6vPxjG0YRA==" },
                    { 2, "olena@parking.com", "Olena Koval", "AQAAAAIAAYagAAAAELkR+DH3s1xMKYGJqo5MhFP1x2a4Bz5KJ2E6TQKZ8cO0vH1nV+uLz0E6vPxjG0YRA==" },
                    { 3, "maksym@parking.com", "Maksym Shevchenko", "AQAAAAIAAYagAAAAELkR+DH3s1xMKYGJqo5MhFP1x2a4Bz5KJ2E6TQKZ8cO0vH1nV+uLz0E6vPxjG0YRA==" }
                });

            migrationBuilder.InsertData(
                table: "ParkingSpots",
                columns: new[] { "ID", "Number", "Status", "UserID" },
                values: new object[,]
                {
                    { 1, 1, "Free", null },
                    { 3, 3, "Free", null },
                    { 5, 5, "Free", null },
                    { 6, 6, "Free", null },
                    { 7, 7, "Free", null },
                    { 9, 9, "Free", null },
                    { 10, 10, "Free", null },
                    { 2, 2, "Occupied", 1 },
                    { 4, 4, "Occupied", 2 },
                    { 8, 8, "Occupied", 3 }
                });

            migrationBuilder.InsertData(
                table: "Reservations",
                columns: new[] { "ID", "AttendantID", "ReservationDate", "SpotID", "Status" },
                values: new object[,]
                {
                    { 1, 1, new DateTime(2026, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 2, "Active" },
                    { 2, 2, new DateTime(2026, 9, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), 4, "Active" },
                    { 3, 3, new DateTime(2026, 9, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), 8, "Active" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Attendants_Email",
                table: "Attendants",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ParkingSpots_Number",
                table: "ParkingSpots",
                column: "Number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ParkingSpots_UserID",
                table: "ParkingSpots",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_AttendantID",
                table: "Reservations",
                column: "AttendantID");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_SpotID",
                table: "Reservations",
                column: "SpotID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Reservations");

            migrationBuilder.DropTable(
                name: "ParkingSpots");

            migrationBuilder.DropTable(
                name: "Attendants");
        }
    }
}
