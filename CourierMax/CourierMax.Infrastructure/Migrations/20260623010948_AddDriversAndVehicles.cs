using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CourierMax.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDriversAndVehicles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Drivers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Drivers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Vehicles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Plate = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DriverId = table.Column<int>(type: "int", nullable: true),
                    MaxWeightKg = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    MaxVolumeM3 = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    CurrentWeightKg = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    CurrentVolumeM3 = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vehicles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Vehicles_Drivers_DriverId",
                        column: x => x.DriverId,
                        principalTable: "Drivers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_DriverId",
                table: "Shipments",
                column: "DriverId");

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_VehicleId",
                table: "Shipments",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_DriverId",
                table: "Vehicles",
                column: "DriverId",
                unique: true,
                filter: "[DriverId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_Plate",
                table: "Vehicles",
                column: "Plate",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Shipments_Drivers_DriverId",
                table: "Shipments",
                column: "DriverId",
                principalTable: "Drivers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Shipments_Vehicles_VehicleId",
                table: "Shipments",
                column: "VehicleId",
                principalTable: "Vehicles",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Shipments_Drivers_DriverId",
                table: "Shipments");

            migrationBuilder.DropForeignKey(
                name: "FK_Shipments_Vehicles_VehicleId",
                table: "Shipments");

            migrationBuilder.DropTable(
                name: "Vehicles");

            migrationBuilder.DropTable(
                name: "Drivers");

            migrationBuilder.DropIndex(
                name: "IX_Shipments_DriverId",
                table: "Shipments");

            migrationBuilder.DropIndex(
                name: "IX_Shipments_VehicleId",
                table: "Shipments");
        }
    }
}
