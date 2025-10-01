using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BarSheetAPI.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedDailyReportstable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DataJson",
                table: "DailyReports",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsPublished",
                table: "DailyReports",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "OverallTotalAmount",
                table: "DailyReports",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "PublishedAt",
                table: "DailyReports",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataJson",
                table: "DailyReports");

            migrationBuilder.DropColumn(
                name: "IsPublished",
                table: "DailyReports");

            migrationBuilder.DropColumn(
                name: "OverallTotalAmount",
                table: "DailyReports");

            migrationBuilder.DropColumn(
                name: "PublishedAt",
                table: "DailyReports");
        }
    }
}
