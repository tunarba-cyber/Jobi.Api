using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkedIn.Modules.Jobs.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddJobViewsAndAlertMatches : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ViewCount",
                schema: "jobs",
                table: "Jobs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "JobAlertMatches",
                schema: "jobs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AlertId = table.Column<long>(type: "bigint", nullable: false),
                    JobId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobAlertMatches", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JobAlertMatches_AlertId_JobId",
                schema: "jobs",
                table: "JobAlertMatches",
                columns: new[] { "AlertId", "JobId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JobAlertMatches_JobId",
                schema: "jobs",
                table: "JobAlertMatches",
                column: "JobId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JobAlertMatches",
                schema: "jobs");

            migrationBuilder.DropColumn(
                name: "ViewCount",
                schema: "jobs",
                table: "Jobs");
        }
    }
}
