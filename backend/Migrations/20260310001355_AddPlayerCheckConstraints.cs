using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace cursor_dotnet_test.Migrations
{
    /// <inheritdoc />
    public partial class AddPlayerCheckConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_Players_PlayerAge_Positive",
                table: "Players",
                sql: "\"PlayerAge\" > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Players_PlayerPosition_NotEmpty",
                table: "Players",
                sql: "length(\"PlayerPosition\") > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Players_TeamId_NotEmpty",
                table: "Players",
                sql: "\"TeamId\" != '00000000-0000-0000-0000-000000000000'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Players_PlayerAge_Positive",
                table: "Players");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Players_PlayerPosition_NotEmpty",
                table: "Players");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Players_TeamId_NotEmpty",
                table: "Players");
        }
    }
}
