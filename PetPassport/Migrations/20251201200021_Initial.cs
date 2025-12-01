using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PetPassport.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Owners",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TelegramId = table.Column<long>(type: "bigint", nullable: false),
                    TelegramNick = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Owners", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Pets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Breed = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    WeightKg = table.Column<decimal>(type: "numeric(6,2)", nullable: true),
                    BirthDate = table.Column<DateOnly>(type: "date", nullable: true),
                    OwnerId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pets_Owners_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Owners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "text", nullable: false),
                    EventDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PetId = table.Column<int>(type: "integer", nullable: false),
                    ReminderEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    ReminderValue = table.Column<int>(type: "integer", nullable: true),
                    ReminderUnit = table.Column<int>(type: "integer", nullable: true),
                    EventType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ReminderDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsReminderSent = table.Column<bool>(type: "boolean", nullable: false),
                    Clinic = table.Column<string>(type: "text", nullable: true),
                    Doctor = table.Column<string>(type: "text", nullable: true),
                    Diagnosis = table.Column<string>(type: "text", nullable: true),
                    Recommendations = table.Column<string>(type: "text", nullable: true),
                    Referrals = table.Column<string>(type: "text", nullable: true),
                    Medicine = table.Column<string>(type: "text", nullable: true),
                    PeriodValue = table.Column<int>(type: "integer", nullable: true),
                    PeriodUnit = table.Column<int>(type: "integer", nullable: true),
                    NextVaccinationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Remedy = table.Column<string>(type: "text", nullable: true),
                    Parasite = table.Column<string>(type: "text", nullable: true),
                    TreatmentEvent_PeriodValue = table.Column<int>(type: "integer", nullable: true),
                    TreatmentEvent_PeriodUnit = table.Column<int>(type: "integer", nullable: true),
                    NextTreatmentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Events_Pets_PetId",
                        column: x => x.PetId,
                        principalTable: "Pets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PetPhotos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Url = table.Column<string>(type: "text", nullable: false),
                    TelegramFileId = table.Column<string>(type: "text", nullable: true),
                    PetId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PetPhotos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PetPhotos_Pets_PetId",
                        column: x => x.PetId,
                        principalTable: "Pets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Events_PetId",
                table: "Events",
                column: "PetId");

            migrationBuilder.CreateIndex(
                name: "IX_Owners_TelegramId",
                table: "Owners",
                column: "TelegramId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PetPhotos_PetId",
                table: "PetPhotos",
                column: "PetId");

            migrationBuilder.CreateIndex(
                name: "IX_Pets_OwnerId",
                table: "Pets",
                column: "OwnerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Events");

            migrationBuilder.DropTable(
                name: "PetPhotos");

            migrationBuilder.DropTable(
                name: "Pets");

            migrationBuilder.DropTable(
                name: "Owners");
        }
    }
}
