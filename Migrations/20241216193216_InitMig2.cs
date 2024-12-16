using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AonFreelancing.Migrations
{
    /// <inheritdoc />
    public partial class InitMig2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "QualificationName",
                table: "Freelancers",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_FREELANCER_QUALIFICATION_NAME",
                table: "Freelancers",
                sql: "[QualificationName] IN ('uiux', 'frontend', 'mobile', 'backend', 'fullstack')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_FREELANCER_QUALIFICATION_NAME",
                table: "Freelancers");

            migrationBuilder.AlterColumn<string>(
                name: "QualificationName",
                table: "Freelancers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
