using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetPassport.Migrations
{
    /// <inheritdoc />
    public partial class Treatment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "NextTreatmentDate",
                table: "Events",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Parasite",
                table: "Events",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Remedy",
                table: "Events",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TreatmentEvent_PeriodUnit",
                table: "Events",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TreatmentEvent_PeriodValue",
                table: "Events",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NextTreatmentDate",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "Parasite",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "Remedy",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "TreatmentEvent_PeriodUnit",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "TreatmentEvent_PeriodValue",
                table: "Events");
        }
    }
}
