using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HumanFlow.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class PhaseD_EmployeeBirthPlace : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BirthCityId",
                table: "Employees",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BirthCountryId",
                table: "Employees",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BirthLocalityId",
                table: "Employees",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_BirthCityId",
                table: "Employees",
                column: "BirthCityId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_BirthCountryId",
                table: "Employees",
                column: "BirthCountryId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_BirthLocalityId",
                table: "Employees",
                column: "BirthLocalityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Cities_BirthCityId",
                table: "Employees",
                column: "BirthCityId",
                principalTable: "Cities",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Countries_BirthCountryId",
                table: "Employees",
                column: "BirthCountryId",
                principalTable: "Countries",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Localities_BirthLocalityId",
                table: "Employees",
                column: "BirthLocalityId",
                principalTable: "Localities",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Cities_BirthCityId",
                table: "Employees");

            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Countries_BirthCountryId",
                table: "Employees");

            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Localities_BirthLocalityId",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Employees_BirthCityId",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Employees_BirthCountryId",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Employees_BirthLocalityId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "BirthCityId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "BirthCountryId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "BirthLocalityId",
                table: "Employees");
        }
    }
}
