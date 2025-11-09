using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetPassport.Migrations
{
    /// <inheritdoc />
    public partial class Vaccines : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Vaccine_Pets_petId",
                table: "Vaccine");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Vaccine",
                table: "Vaccine");

            migrationBuilder.RenameTable(
                name: "Vaccine",
                newName: "Vaccines");

            migrationBuilder.RenameColumn(
                name: "petId",
                table: "Vaccines",
                newName: "PetId");

            migrationBuilder.RenameIndex(
                name: "IX_Vaccine_petId",
                table: "Vaccines",
                newName: "IX_Vaccines_PetId");

            migrationBuilder.AlterColumn<int>(
                name: "PeriodValue",
                table: "Vaccines",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "PeriodUnit",
                table: "Vaccines",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "NextVacinationDate",
                table: "Vaccines",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Vaccines",
                table: "Vaccines",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Vaccines_Pets_PetId",
                table: "Vaccines",
                column: "PetId",
                principalTable: "Pets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Vaccines_Pets_PetId",
                table: "Vaccines");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Vaccines",
                table: "Vaccines");

            migrationBuilder.RenameTable(
                name: "Vaccines",
                newName: "Vaccine");

            migrationBuilder.RenameColumn(
                name: "PetId",
                table: "Vaccine",
                newName: "petId");

            migrationBuilder.RenameIndex(
                name: "IX_Vaccines_PetId",
                table: "Vaccine",
                newName: "IX_Vaccine_petId");

            migrationBuilder.AlterColumn<int>(
                name: "PeriodValue",
                table: "Vaccine",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "PeriodUnit",
                table: "Vaccine",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "NextVacinationDate",
                table: "Vaccine",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1),
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Vaccine",
                table: "Vaccine",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Vaccine_Pets_petId",
                table: "Vaccine",
                column: "petId",
                principalTable: "Pets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
