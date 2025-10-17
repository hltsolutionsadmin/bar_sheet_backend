using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BarSheetAPI.Migrations
{
    /// <inheritdoc />
    public partial class SplitDailyReportJsons : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DataJson",
                table: "DailyReports",
                newName: "SalesJson");

            migrationBuilder.AddColumn<string>(
                name: "CBJson",
                table: "DailyReports",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OBJson",
                table: "DailyReports",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReceiptsJson",
                table: "DailyReports",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalReceiptsAmount",
                table: "DailyReports",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalSalesAmount",
                table: "DailyReports",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CBJson",
                table: "DailyReports");

            migrationBuilder.DropColumn(
                name: "OBJson",
                table: "DailyReports");

            migrationBuilder.DropColumn(
                name: "ReceiptsJson",
                table: "DailyReports");

            migrationBuilder.DropColumn(
                name: "TotalReceiptsAmount",
                table: "DailyReports");

            migrationBuilder.DropColumn(
                name: "TotalSalesAmount",
                table: "DailyReports");

            migrationBuilder.RenameColumn(
                name: "SalesJson",
                table: "DailyReports",
                newName: "DataJson");
        }
    }
}
