using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyEBookLibrary.Migrations
{
    /// <inheritdoc />
    public partial class RevertAddBorrowsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Borrows_AspNetUsers_UserId1",
                table: "Borrows");

            migrationBuilder.DropIndex(
                name: "IX_Borrows_UserId1",
                table: "Borrows");

            migrationBuilder.DropColumn(
                name: "Subtotal",
                table: "CartItem");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "Borrows");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "Borrows",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Borrows_UserId",
                table: "Borrows",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Borrows_AspNetUsers_UserId",
                table: "Borrows",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Borrows_AspNetUsers_UserId",
                table: "Borrows");

            migrationBuilder.DropIndex(
                name: "IX_Borrows_UserId",
                table: "Borrows");

            migrationBuilder.AddColumn<decimal>(
                name: "Subtotal",
                table: "CartItem",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Borrows",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "UserId1",
                table: "Borrows",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Borrows_UserId1",
                table: "Borrows",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Borrows_AspNetUsers_UserId1",
                table: "Borrows",
                column: "UserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
