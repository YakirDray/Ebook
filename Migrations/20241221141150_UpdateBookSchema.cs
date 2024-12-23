using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyEBookLibrary.Migrations
{
    /// <inheritdoc />
    public partial class UpdateBookSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DiscountEndDate",
                table: "Books",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "OriginalPrice",
                table: "Books",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiscountEndDate",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "OriginalPrice",
                table: "Books");
        }
    }
}
