using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RVAegis.Migrations
{
    /// <inheritdoc />
    public partial class HistoryRecordsCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TypeActions",
                columns: table => new
                {
                    ActionId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ActionDescription = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TypeActions", x => x.ActionId);
                });

            migrationBuilder.CreateTable(
                name: "HistoryRecords",
                columns: table => new
                {
                    HistoryRecordId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DateAction = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TypeActionId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoryRecords", x => x.HistoryRecordId);
                    table.ForeignKey(
                        name: "FK_HistoryRecords_TypeActions_TypeActionId",
                        column: x => x.TypeActionId,
                        principalTable: "TypeActions",
                        principalColumn: "ActionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HistoryRecords_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HistoryRecords_TypeActionId",
                table: "HistoryRecords",
                column: "TypeActionId");

            migrationBuilder.CreateIndex(
                name: "IX_HistoryRecords_UserId",
                table: "HistoryRecords",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HistoryRecords");

            migrationBuilder.DropTable(
                name: "TypeActions");
        }
    }
}
