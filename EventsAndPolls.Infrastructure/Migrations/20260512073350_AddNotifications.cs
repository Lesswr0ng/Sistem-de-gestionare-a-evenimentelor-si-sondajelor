using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventsAndPolls.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PollOptionGroups_Polls_PollId",
                table: "PollOptionGroups");

            migrationBuilder.DropForeignKey(
                name: "FK_PollOptions_PollOptionGroups_GroupId",
                table: "PollOptions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PollOptionGroups",
                table: "PollOptionGroups");

            migrationBuilder.RenameTable(
                name: "PollOptionGroups",
                newName: "PollOptionGroup");

            migrationBuilder.RenameIndex(
                name: "IX_PollOptionGroups_PollId",
                table: "PollOptionGroup",
                newName: "IX_PollOptionGroup_PollId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PollOptionGroup",
                table: "PollOptionGroup",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Body = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Channel = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    RelatedPollId = table.Column<int>(type: "int", nullable: true),
                    RelatedEventId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_CreatedAt",
                table: "Notifications",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId_IsRead",
                table: "Notifications",
                columns: new[] { "UserId", "IsRead" });

            migrationBuilder.AddForeignKey(
                name: "FK_PollOptionGroup_Polls_PollId",
                table: "PollOptionGroup",
                column: "PollId",
                principalTable: "Polls",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PollOptions_PollOptionGroup_GroupId",
                table: "PollOptions",
                column: "GroupId",
                principalTable: "PollOptionGroup",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PollOptionGroup_Polls_PollId",
                table: "PollOptionGroup");

            migrationBuilder.DropForeignKey(
                name: "FK_PollOptions_PollOptionGroup_GroupId",
                table: "PollOptions");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PollOptionGroup",
                table: "PollOptionGroup");

            migrationBuilder.RenameTable(
                name: "PollOptionGroup",
                newName: "PollOptionGroups");

            migrationBuilder.RenameIndex(
                name: "IX_PollOptionGroup_PollId",
                table: "PollOptionGroups",
                newName: "IX_PollOptionGroups_PollId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PollOptionGroups",
                table: "PollOptionGroups",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PollOptionGroups_Polls_PollId",
                table: "PollOptionGroups",
                column: "PollId",
                principalTable: "Polls",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PollOptions_PollOptionGroups_GroupId",
                table: "PollOptions",
                column: "GroupId",
                principalTable: "PollOptionGroups",
                principalColumn: "Id");
        }
    }
}
