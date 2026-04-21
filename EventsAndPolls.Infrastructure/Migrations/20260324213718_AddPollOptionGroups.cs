using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventsAndPolls.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPollOptionGroups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GroupId",
                table: "PollOptions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PollOptionGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PollId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PollOptionGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PollOptionGroups_Polls_PollId",
                        column: x => x.PollId,
                        principalTable: "Polls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PollOptions_GroupId",
                table: "PollOptions",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_PollOptionGroups_PollId",
                table: "PollOptionGroups",
                column: "PollId");

            migrationBuilder.AddForeignKey(
                name: "FK_PollOptions_PollOptionGroups_GroupId",
                table: "PollOptions",
                column: "GroupId",
                principalTable: "PollOptionGroups",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PollOptions_PollOptionGroups_GroupId",
                table: "PollOptions");

            migrationBuilder.DropTable(
                name: "PollOptionGroups");

            migrationBuilder.DropIndex(
                name: "IX_PollOptions_GroupId",
                table: "PollOptions");

            migrationBuilder.DropColumn(
                name: "GroupId",
                table: "PollOptions");
        }
    }
}
