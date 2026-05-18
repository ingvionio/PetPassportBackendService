using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetPassport.Migrations
{
    /// <inheritdoc />
    public partial class AddPetPassportFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Allergies",
                table: "Pets",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BloodType",
                table: "Pets",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ChronicConditions",
                table: "Pets",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "Pets",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Gender",
                table: "Pets",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsNeutered",
                table: "Pets",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MicrochipNumber",
                table: "Pets",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Species",
                table: "Pets",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Allergies",
                table: "Pets");

            migrationBuilder.DropColumn(
                name: "BloodType",
                table: "Pets");

            migrationBuilder.DropColumn(
                name: "ChronicConditions",
                table: "Pets");

            migrationBuilder.DropColumn(
                name: "Color",
                table: "Pets");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "Pets");

            migrationBuilder.DropColumn(
                name: "IsNeutered",
                table: "Pets");

            migrationBuilder.DropColumn(
                name: "MicrochipNumber",
                table: "Pets");

            migrationBuilder.DropColumn(
                name: "Species",
                table: "Pets");
        }
    }
}
