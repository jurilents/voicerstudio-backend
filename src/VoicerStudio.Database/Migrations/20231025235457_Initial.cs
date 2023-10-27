using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VoicerStudio.Database.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppUsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TgUserId = table.Column<long>(type: "bigint", nullable: false),
                    TgUsername = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Language = table.Column<string>(type: "varchar(2)", unicode: false, maxLength: 2, nullable: false),
                    Role = table.Column<string>(type: "varchar(16)", unicode: false, maxLength: 16, nullable: false),
                    Status = table.Column<string>(type: "varchar(16)", unicode: false, maxLength: 16, nullable: false),
                    AdminWhoSetStatusId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Updated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppUsers_AppUsers_AdminWhoSetStatusId",
                        column: x => x.AdminWhoSetStatusId,
                        principalTable: "AppUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AppTokens",
                columns: table => new
                {
                    UserToken = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    JwtToken = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Expired = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppTokens", x => x.UserToken);
                    table.ForeignKey(
                        name: "FK_AppTokens_AppUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AppUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppTokens_UserId",
                table: "AppTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AppUsers_AdminWhoSetStatusId",
                table: "AppUsers",
                column: "AdminWhoSetStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_AppUsers_TgUserId",
                table: "AppUsers",
                column: "TgUserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppTokens");

            migrationBuilder.DropTable(
                name: "AppUsers");
        }
    }
}
