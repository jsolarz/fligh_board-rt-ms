using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlightBoard.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialFlightSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Flights",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FlightNumber = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    Airline = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false),
                    Origin = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false),
                    Destination = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false),
                    ScheduledDeparture = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ActualDeparture = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ScheduledArrival = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ActualArrival = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Gate = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    Terminal = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    AircraftType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Remarks = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    DelayMinutes = table.Column<int>(type: "INTEGER", nullable: false),
                    Type = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Flights", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Flights_Airline",
                table: "Flights",
                column: "Airline");

            migrationBuilder.CreateIndex(
                name: "IX_Flights_Destination",
                table: "Flights",
                column: "Destination");

            migrationBuilder.CreateIndex(
                name: "IX_Flights_Destination_Type",
                table: "Flights",
                columns: new[] { "Destination", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_Flights_FlightNumber",
                table: "Flights",
                column: "FlightNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Flights_FlightNumber_ScheduledDeparture",
                table: "Flights",
                columns: new[] { "FlightNumber", "ScheduledDeparture" });

            migrationBuilder.CreateIndex(
                name: "IX_Flights_IsDeleted",
                table: "Flights",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Flights_Origin",
                table: "Flights",
                column: "Origin");

            migrationBuilder.CreateIndex(
                name: "IX_Flights_Origin_Type",
                table: "Flights",
                columns: new[] { "Origin", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_Flights_ScheduledArrival",
                table: "Flights",
                column: "ScheduledArrival");

            migrationBuilder.CreateIndex(
                name: "IX_Flights_ScheduledDeparture",
                table: "Flights",
                column: "ScheduledDeparture");

            migrationBuilder.CreateIndex(
                name: "IX_Flights_Status",
                table: "Flights",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Flights_Type",
                table: "Flights",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_Flights_Type_Status",
                table: "Flights",
                columns: new[] { "Type", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Flights");
        }
    }
}
