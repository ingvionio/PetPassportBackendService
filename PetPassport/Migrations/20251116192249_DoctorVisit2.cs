using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetPassport.Migrations
{
    /// <inheritdoc />
    public partial class DoctorVisit2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "EventType",
                table: "Events",
                type: "character varying(13)",
                maxLength: 13,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(8)",
                oldMaxLength: 8);

            migrationBuilder.AddColumn<string>(
                name: "Clinic",
                table: "Events",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Diagnosis",
                table: "Events",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Doctor",
                table: "Events",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Recommendations",
                table: "Events",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Referrals",
                table: "Events",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Clinic",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "Diagnosis",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "Doctor",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "Recommendations",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "Referrals",
                table: "Events");

            migrationBuilder.AlterColumn<string>(
                name: "EventType",
                table: "Events",
                type: "character varying(8)",
                maxLength: 8,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(13)",
                oldMaxLength: 13);
        }
    }
}
