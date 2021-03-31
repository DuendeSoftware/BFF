using System;
using Microsoft.EntityFrameworkCore.Migrations;

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
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Renewed = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Expires = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Ticket = table.Column<string>(type: "TEXT", nullable: false),
                    Key = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    SubjectId = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    SessionId = table.Column<string>(type: "TEXT", nullable: true),
                    Scheme = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSessions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserSessions_Key",
                table: "UserSessions",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserSessions_SessionId",
                table: "UserSessions",
                column: "SessionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserSessions_SubjectId_SessionId",
                table: "UserSessions",
                columns: new[] { "SubjectId", "SessionId" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserSessions");
        }
    }
}
