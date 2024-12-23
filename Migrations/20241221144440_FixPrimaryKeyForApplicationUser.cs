using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyEBookLibrary.Migrations
{
    /// <inheritdoc />
    public partial class FixPrimaryKeyForApplicationUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookReviews_AspNetUsers_UserId",
                table: "BookReviews");

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountedPrice",
                table: "Books",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<string>(
                name: "Comment",
                table: "BookReviews",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "UserId1",
                table: "BookReviews",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ApplicationUser",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationUser", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BookReviews_UserId1",
                table: "BookReviews",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_BookReviews_ApplicationUser_UserId",
                table: "BookReviews",
                column: "UserId",
                principalTable: "ApplicationUser",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BookReviews_AspNetUsers_UserId1",
                table: "BookReviews",
                column: "UserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookReviews_ApplicationUser_UserId",
                table: "BookReviews");

            migrationBuilder.DropForeignKey(
                name: "FK_BookReviews_AspNetUsers_UserId1",
                table: "BookReviews");

            migrationBuilder.DropTable(
                name: "ApplicationUser");

            migrationBuilder.DropIndex(
                name: "IX_BookReviews_UserId1",
                table: "BookReviews");

            migrationBuilder.DropColumn(
                name: "DiscountedPrice",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "BookReviews");

            migrationBuilder.AlterColumn<string>(
                name: "Comment",
                table: "BookReviews",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000);

            migrationBuilder.AddForeignKey(
                name: "FK_BookReviews_AspNetUsers_UserId",
                table: "BookReviews",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
