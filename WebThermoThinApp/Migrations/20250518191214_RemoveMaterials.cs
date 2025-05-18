using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebThermoThinApp.Migrations
{
    /// <inheritdoc />
    public partial class RemoveMaterials : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Materials");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Materials",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Density = table.Column<double>(type: "REAL", nullable: false),
                    Emissivity = table.Column<double>(type: "REAL", nullable: false),
                    HeatCapacity = table.Column<double>(type: "REAL", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    ThermalConductivity = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Materials", x => x.Id);
                });
        }
    }
}
