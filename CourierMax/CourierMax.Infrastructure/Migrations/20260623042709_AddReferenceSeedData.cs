using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CourierMax.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReferenceSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "CityDistances",
                columns: new[] { "Id", "Destination", "DistanceFee", "DistanceKm", "Origin" },
                values: new object[,]
                {
                    { 1, "Medellín", 12000m, 480m, "Bogotá" },
                    { 2, "Cali", 9000m, 360m, "Bogotá" },
                    { 3, "Barranquilla", 20000m, 950m, "Bogotá" },
                    { 4, "Cali", 8000m, 310m, "Medellín" },
                    { 5, "Barranquilla", 15000m, 650m, "Medellín" },
                    { 6, "Barranquilla", 18000m, 900m, "Cali" }
                });

            migrationBuilder.InsertData(
                table: "Drivers",
                columns: new[] { "Id", "CreatedAt", "Email", "IsActive", "Name", "Phone", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 6, 10, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "Juan Pérez", null, null },
                    { 2, new DateTime(2026, 6, 10, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "María López", null, null },
                    { 3, new DateTime(2026, 6, 10, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "Carlos Ruiz", null, null }
                });

            migrationBuilder.InsertData(
                table: "Vehicles",
                columns: new[] { "Id", "CreatedAt", "CurrentVolumeM3", "CurrentWeightKg", "DriverId", "MaxVolumeM3", "MaxWeightKg", "Plate", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 6, 10, 0, 0, 0, 0, DateTimeKind.Utc), 0m, 0m, 1, 10m, 500m, "ABC-123", null },
                    { 2, new DateTime(2026, 6, 10, 0, 0, 0, 0, DateTimeKind.Utc), 0m, 0m, 2, 6m, 300m, "DEF-456", null },
                    { 3, new DateTime(2026, 6, 10, 0, 0, 0, 0, DateTimeKind.Utc), 0m, 0m, 3, 15m, 800m, "GHI-789", null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "CityDistances",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "CityDistances",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "CityDistances",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "CityDistances",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "CityDistances",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "CityDistances",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Drivers",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Drivers",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Drivers",
                keyColumn: "Id",
                keyValue: 3);
        }
    }
}
