using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RVAegis.Migrations
{
    /// <inheritdoc />
    public partial class RecognitionLogAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RecognitionLogs",
                columns: table => new
                {
                    RecognitionLogId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ImageData = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    Label = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    RecognitionTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CameraIndex = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecognitionLogs", x => x.RecognitionLogId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RecognitionLogs");
        }
    }
}
