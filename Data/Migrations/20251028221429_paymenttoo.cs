using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EvergreenRanch.Data.Migrations
{
    /// <inheritdoc />
    public partial class paymenttoo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "hourlyRate",
                table: "ShiftAttendances",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "hourlyRate",
                table: "ShiftAttendances");
        }
    }
}
