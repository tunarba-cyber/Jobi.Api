using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkedIn.Modules.Jobs.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFollows : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CompanyFollows",
                schema: "jobs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FollowerUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CompanyId = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("PK_CompanyFollows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanyFollows_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalSchema: "jobs",
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserFollows",
                schema: "jobs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FollowerUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    FollowedUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
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
                    table.PrimaryKey("PK_UserFollows", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CompanyFollows_CompanyId",
                schema: "jobs",
                table: "CompanyFollows",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyFollows_FollowerUserId_CompanyId",
                schema: "jobs",
                table: "CompanyFollows",
                columns: new[] { "FollowerUserId", "CompanyId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_UserFollows_FollowedUserId",
                schema: "jobs",
                table: "UserFollows",
                column: "FollowedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserFollows_FollowerUserId_FollowedUserId",
                schema: "jobs",
                table: "UserFollows",
                columns: new[] { "FollowerUserId", "FollowedUserId" },
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompanyFollows",
                schema: "jobs");

            migrationBuilder.DropTable(
                name: "UserFollows",
                schema: "jobs");
        }
    }
}
