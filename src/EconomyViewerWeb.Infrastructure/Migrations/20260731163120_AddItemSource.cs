using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EconomyViewerWeb.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddItemSource : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Source",
                table: "Items",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Source",
                table: "Items");
        }
    }
}
