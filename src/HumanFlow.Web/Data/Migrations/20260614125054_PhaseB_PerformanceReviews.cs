using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HumanFlow.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class PhaseB_PerformanceReviews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PerformanceReviews",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PeriodType = table.Column<int>(type: "int", nullable: false),
                    PeriodYear = table.Column<int>(type: "int", nullable: false),
                    PeriodNumber = table.Column<int>(type: "int", nullable: false),
                    ReviewDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ReviewerEmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OverallRating = table.Column<int>(type: "int", nullable: false),
                    StrengthsNotes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ImprovementNotes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    GoalsNotes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    AcknowledgedDate = table.Column<DateOnly>(type: "date", nullable: true),
                    AcknowledgementNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerformanceReviews", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PerformanceReviews_EmployeeId_PeriodYear_PeriodType_PeriodNumber",
                table: "PerformanceReviews",
                columns: new[] { "EmployeeId", "PeriodYear", "PeriodType", "PeriodNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PerformanceReviews_TenantId_EmployeeId",
                table: "PerformanceReviews",
                columns: new[] { "TenantId", "EmployeeId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PerformanceReviews");
        }
    }
}
