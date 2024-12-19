﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JewelEase.Migrations
{
    /// <inheritdoc />
    public partial class jewelitemupdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TryOnUrl",
                table: "JewelryItems",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TryOnUrl",
                table: "JewelryItems");
        }
    }
}
