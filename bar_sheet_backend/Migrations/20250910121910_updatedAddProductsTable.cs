using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BarSheetAPI.Migrations
{
    /// <inheritdoc />
    public partial class updatedAddProductsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "VariantsJson",
                table: "Products",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VariantsJson",
                table: "Products");
        }
    }
}
