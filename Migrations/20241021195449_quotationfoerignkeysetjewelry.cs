using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JewelEase.Migrations
{
    /// <inheritdoc />
    public partial class quotationfoerignkeysetjewelry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ItemQuotationLineItem_JewelryItemId",
                table: "ItemQuotationLineItem",
                column: "JewelryItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemQuotationLineItem_JewelryItems_JewelryItemId",
                table: "ItemQuotationLineItem",
                column: "JewelryItemId",
                principalTable: "JewelryItems",
                principalColumn: "JewelryItemId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItemQuotationLineItem_JewelryItems_JewelryItemId",
                table: "ItemQuotationLineItem");

            migrationBuilder.DropIndex(
                name: "IX_ItemQuotationLineItem_JewelryItemId",
                table: "ItemQuotationLineItem");
        }
    }
}
