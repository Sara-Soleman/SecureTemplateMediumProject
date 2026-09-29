using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdentityPlatform.Identity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class userSessionAddedToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UserId1",
                table: "UserSessions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserSessions_UserId1",
                table: "UserSessions",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_UserSessions_Users_UserId1",
                table: "UserSessions",
                column: "UserId1",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserSessions_Users_UserId1",
                table: "UserSessions");

            migrationBuilder.DropIndex(
                name: "IX_UserSessions_UserId1",
                table: "UserSessions");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "UserSessions");
        }
    }
}
