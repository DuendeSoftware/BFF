using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserSessionDb.Migrations.UserSessions
{
    public partial class UserSessions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserSessions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicationName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SubjectId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SessionId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Renewed = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Expires = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Ticket = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSessions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserSessions_ApplicationName_Key",
                table: "UserSessions",
                columns: new[] { "ApplicationName", "Key" },
                unique: true,
                filter: "[ApplicationName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UserSessions_ApplicationName_SessionId",
                table: "UserSessions",
                columns: new[] { "ApplicationName", "SessionId" },
                unique: true,
                filter: "[ApplicationName] IS NOT NULL AND [SessionId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UserSessions_ApplicationName_SubjectId_SessionId",
                table: "UserSessions",
                columns: new[] { "ApplicationName", "SubjectId", "SessionId" },
                unique: true,
                filter: "[ApplicationName] IS NOT NULL AND [SessionId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UserSessions_Expires",
                table: "UserSessions",
                column: "Expires");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserSessions");
        }
    }
}
