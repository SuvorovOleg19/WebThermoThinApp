using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebThermoThinApp.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomMaterialFieldsToVariant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "MaterialConductivity",
                table: "Variants",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "MaterialDensity",
                table: "Variants",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "MaterialHeatCapacity",
                table: "Variants",
                type: "REAL",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaterialConductivity",
                table: "Variants");

            migrationBuilder.DropColumn(
                name: "MaterialDensity",
                table: "Variants");

            migrationBuilder.DropColumn(
                name: "MaterialHeatCapacity",
                table: "Variants");
        }
    }
}
