using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PetPassport.Migrations
{
    /// <inheritdoc />
    public partial class AddEventTemplates2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EventTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "text", nullable: false),
                    EventType = table.Column<int>(type: "integer", nullable: false),
                    Clinic = table.Column<string>(type: "text", nullable: true),
                    Doctor = table.Column<string>(type: "text", nullable: true),
                    Diagnosis = table.Column<string>(type: "text", nullable: true),
                    Recommendations = table.Column<string>(type: "text", nullable: true),
                    Referrals = table.Column<string>(type: "text", nullable: true),
                    Medicine = table.Column<string>(type: "text", nullable: true),
                    PeriodValue = table.Column<int>(type: "integer", nullable: true),
                    PeriodUnit = table.Column<int>(type: "integer", nullable: true),
                    Remedy = table.Column<string>(type: "text", nullable: true),
                    Parasite = table.Column<string>(type: "text", nullable: true),
                    TreatmentPeriodValue = table.Column<int>(type: "integer", nullable: true),
                    TreatmentPeriodUnit = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventTemplates", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventTemplates");
        }
    }
}
