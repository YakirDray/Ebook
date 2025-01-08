using Microsoft.EntityFrameworkCore.Migrations;

namespace MyEBookLibrary.Migrations
{
    public partial class AddColumnsToUserBooks : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // בדיקה אם העמודה כבר קיימת
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT * FROM sys.columns 
                    WHERE Name = N'IsReturned'
                    AND Object_ID = Object_ID(N'UserBooks')
                )
                BEGIN
                    ALTER TABLE UserBooks
                    ADD IsReturned bit NOT NULL DEFAULT(0)
                END
            ");

            // עדכון ערכי IsReturned על סמך ReturnDate
            migrationBuilder.Sql(@"
                UPDATE UserBooks 
                SET IsReturned = CASE 
                    WHEN ReturnDate IS NOT NULL THEN 1 
                    ELSE 0 
                END
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT * FROM sys.columns 
                    WHERE Name = N'IsReturned'
                    AND Object_ID = Object_ID(N'UserBooks')
                )
                BEGIN
                    ALTER TABLE UserBooks
                    DROP COLUMN IsReturned
                END
            ");
        }
    }
}