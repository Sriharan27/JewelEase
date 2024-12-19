using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JewelEase.Migrations
{
    /// <inheritdoc />
    public partial class jewelrytblstatussetting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "JewelryItems",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
