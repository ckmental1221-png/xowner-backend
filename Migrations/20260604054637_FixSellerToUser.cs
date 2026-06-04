using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Xowner.Com.Migrations
{
    /// <inheritdoc />
    public partial class FixSellerToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Sellers_SellerId",
                table: "Products");

            migrationBuilder.AddColumn<int>(
                name: "SellerId1",
                table: "Products",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Products_SellerId1",
                table: "Products",
                column: "SellerId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Sellers_SellerId1",
                table: "Products",
                column: "SellerId1",
                principalTable: "Sellers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Users_SellerId",
                table: "Products",
                column: "SellerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Sellers_SellerId1",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Users_SellerId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_SellerId1",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "SellerId1",
                table: "Products");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Sellers_SellerId",
                table: "Products",
                column: "SellerId",
                principalTable: "Sellers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
