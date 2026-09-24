using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkedIn.Modules.Jobs.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddApplications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Applications",
                schema: "jobs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobId = table.Column<long>(type: "bigint", nullable: false),
                    CandidateUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CandidateName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CandidateEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ResumeUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CoverLetter = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Applications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Applications_Jobs_JobId",
                        column: x => x.JobId,
                        principalSchema: "jobs",
                        principalTable: "Jobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Applications_CandidateUserId",
                schema: "jobs",
                table: "Applications",
                column: "CandidateUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Applications_JobId_CandidateUserId",
                schema: "jobs",
                table: "Applications",
                columns: new[] { "JobId", "CandidateUserId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Applications_JobId_Status",
                schema: "jobs",
                table: "Applications",
                columns: new[] { "JobId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Applications",
                schema: "jobs");
        }
    }
}
