using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkedIn.Modules.Jobs.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCompany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompanyName",
                schema: "jobs",
                table: "Jobs");

            migrationBuilder.AddColumn<long>(
                name: "CompanyId",
                schema: "jobs",
                table: "Jobs",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "Companies",
                schema: "jobs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OwnerUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    WebsiteUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    LogoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ModifiedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_CompanyId_Status",
                schema: "jobs",
                table: "Jobs",
                columns: new[] { "CompanyId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Companies_OwnerUserId",
                schema: "jobs",
                table: "Companies",
                column: "OwnerUserId",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Companies_Slug",
                schema: "jobs",
                table: "Companies",
                column: "Slug",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.AddForeignKey(
                name: "FK_Jobs_Companies_CompanyId",
                schema: "jobs",
                table: "Jobs",
                column: "CompanyId",
                principalSchema: "jobs",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Jobs_Companies_CompanyId",
                schema: "jobs",
                table: "Jobs");

            migrationBuilder.DropTable(
                name: "Companies",
                schema: "jobs");

            migrationBuilder.DropIndex(
                name: "IX_Jobs_CompanyId_Status",
                schema: "jobs",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                schema: "jobs",
                table: "Jobs");

            migrationBuilder.AddColumn<string>(
                name: "CompanyName",
                schema: "jobs",
                table: "Jobs",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");
        }
    }
}
