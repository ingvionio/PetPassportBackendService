using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetPassport.Migrations
{
    /// <inheritdoc />
    public partial class Mab : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "TelegramId",
                table: "Owners",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<string>(
                name: "login",
                table: "Owners",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "password",
                table: "Owners",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "login",
                table: "Owners");

            migrationBuilder.DropColumn(
                name: "password",
                table: "Owners");

            migrationBuilder.AlterColumn<long>(
                name: "TelegramId",
                table: "Owners",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);
        }
    }
}
