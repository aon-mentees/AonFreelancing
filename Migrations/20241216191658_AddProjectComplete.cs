using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AonFreelancing.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectComplete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_PROJECT_STATUS",
                table: "Projects");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Projects",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "pending",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldDefaultValue: "available");

            migrationBuilder.AddCheckConstraint(
                name: "CK_PROJECT_STATUS",
                table: "Projects",
                sql: "[Status] IN ('pending', 'in-progress', 'completed')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_PROJECT_STATUS",
                table: "Projects");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Projects",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "available",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldDefaultValue: "pending");

            migrationBuilder.AddCheckConstraint(
                name: "CK_PROJECT_STATUS",
                table: "Projects",
                sql: "[Status] IN ('available', 'closed')");
        }
    }
}
