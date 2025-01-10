using Microsoft.EntityFrameworkCore.Migrations;

namespace MyEBookLibrary.Migrations
{
    public partial class AddColumnsToUserBooks : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Adding IsReturned column if it does not exist
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

            // Setting IsReturned based on ReturnDate
            migrationBuilder.Sql(@"
                UPDATE UserBooks 
                SET IsReturned = CASE 
                    WHEN ReturnDate IS NOT NULL THEN 1 
                    ELSE 0 
                END
            ");

            // Adding BuyPrice column if it does not exist
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT * FROM sys.columns 
                    WHERE Name = N'BuyPrice'
                    AND Object_ID = Object_ID(N'UserBooks')
                )
                BEGIN
                    ALTER TABLE UserBooks
                    ADD BuyPrice DECIMAL(18, 2) NULL
                END
            ");
        }
protected override void Down(MigrationBuilder migrationBuilder)
{
    // Drop the constraint on IsReturned if it exists
    migrationBuilder.Sql(@"
        DECLARE @ConstraintName nvarchar(128)
        SELECT @ConstraintName = name
        FROM sys.default_constraints
        WHERE parent_object_id = object_id('UserBooks')
            AND type = 'D'
            AND parent_column_id = (
                SELECT column_id
                FROM sys.columns
                WHERE object_id = object_id('UserBooks')
                    AND name = 'IsReturned'
            )
        IF @ConstraintName IS NOT NULL
            EXEC('ALTER TABLE UserBooks DROP CONSTRAINT ' + @ConstraintName)
    ");

    // Drop the IsReturned column if it exists
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

    // Drop the BuyPrice column if it exists
    migrationBuilder.Sql(@"
        IF EXISTS (
            SELECT * FROM sys.columns
            WHERE Name = N'BuyPrice'
            AND Object_ID = Object_ID(N'UserBooks')
        )
        BEGIN
            ALTER TABLE UserBooks
            DROP COLUMN BuyPrice
        END
    ");
}
    }
}