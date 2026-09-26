using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkedIn.Modules.Jobs.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSavedItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SavedCandidates",
                schema: "jobs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CandidateProfileId = table.Column<long>(type: "bigint", nullable: false),
                    EmployerUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
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
                    table.PrimaryKey("PK_SavedCandidates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SavedCandidates_CandidateProfiles_CandidateProfileId",
                        column: x => x.CandidateProfileId,
                        principalSchema: "jobs",
                        principalTable: "CandidateProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SavedJobs",
                schema: "jobs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobId = table.Column<long>(type: "bigint", nullable: false),
                    CandidateUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
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
                    table.PrimaryKey("PK_SavedJobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SavedJobs_Jobs_JobId",
                        column: x => x.JobId,
                        principalSchema: "jobs",
                        principalTable: "Jobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SavedCandidates_CandidateProfileId_EmployerUserId",
                schema: "jobs",
                table: "SavedCandidates",
                columns: new[] { "CandidateProfileId", "EmployerUserId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_SavedCandidates_EmployerUserId",
                schema: "jobs",
                table: "SavedCandidates",
                column: "EmployerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SavedJobs_CandidateUserId",
                schema: "jobs",
                table: "SavedJobs",
                column: "CandidateUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SavedJobs_JobId_CandidateUserId",
                schema: "jobs",
                table: "SavedJobs",
                columns: new[] { "JobId", "CandidateUserId" },
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SavedCandidates",
                schema: "jobs");

            migrationBuilder.DropTable(
                name: "SavedJobs",
                schema: "jobs");
        }
    }
}
