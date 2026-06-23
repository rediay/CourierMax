using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CourierMax.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCityDistances : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CityDistances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Origin = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Destination = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DistanceKm = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    DistanceFee = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CityDistances", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CityDistances_Origin_Destination",
                table: "CityDistances",
                columns: new[] { "Origin", "Destination" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CityDistances");
        }
    }
}
