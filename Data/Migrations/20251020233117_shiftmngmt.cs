using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EvergreenRanch.Data.Migrations
{
    /// <inheritdoc />
    public partial class shiftmngmt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsOvertime",
                table: "Shifts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "PayRate",
                table: "Shifts",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Shifts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ShiftAttendances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ShiftId = table.Column<int>(type: "int", nullable: false),
                    WorkerId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClockInTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ClockOutTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShiftAttendances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShiftAttendances_Shifts_ShiftId",
                        column: x => x.ShiftId,
                        principalTable: "Shifts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ShiftAttendances_ShiftId",
                table: "ShiftAttendances",
                column: "ShiftId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShiftAttendances");

            migrationBuilder.DropColumn(
                name: "IsOvertime",
                table: "Shifts");

            migrationBuilder.DropColumn(
                name: "PayRate",
                table: "Shifts");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Shifts");
        }
    }
}
