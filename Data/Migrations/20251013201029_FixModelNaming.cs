using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EvergreenRanch.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixModelNaming : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "UserModuleProgress",
                newName: "UserModuleProgresses");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "UserModuleProgresses",
                newName: "UserModuleProgress");
        }
    }
}
