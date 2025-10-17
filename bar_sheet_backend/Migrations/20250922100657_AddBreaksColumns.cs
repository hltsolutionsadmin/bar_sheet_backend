using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BarSheetAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddBreaksColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClosingBalance",
                table: "DailyReports");

            migrationBuilder.DropColumn(
                name: "Denomination",
                table: "DailyReports");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "DailyReports");

            migrationBuilder.RenameColumn(
                name: "OpeningBalance",
                table: "DailyReports",
                newName: "BreaksJson");

            migrationBuilder.AddColumn<decimal>(
                name: "TotalBreaksAmount",
                table: "DailyReports",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalBreaksAmount",
                table: "DailyReports");

            migrationBuilder.RenameColumn(
                name: "BreaksJson",
                table: "DailyReports",
                newName: "OpeningBalance");

            migrationBuilder.AddColumn<string>(
                name: "ClosingBalance",
                table: "DailyReports",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Denomination",
                table: "DailyReports",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "DailyReports",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
