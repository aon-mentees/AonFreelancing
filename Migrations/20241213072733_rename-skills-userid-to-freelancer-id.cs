using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AonFreelancing.Migrations
{
    /// <inheritdoc />
    public partial class renameskillsuseridtofreelancerid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Skills_Freelancers_UserId",
                table: "Skills");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Skills",
                newName: "FreelancerId");

            migrationBuilder.RenameIndex(
                name: "IX_Skills_UserId",
                table: "Skills",
                newName: "IX_Skills_FreelancerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Skills_Freelancers_FreelancerId",
                table: "Skills",
                column: "FreelancerId",
                principalTable: "Freelancers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Skills_Freelancers_FreelancerId",
                table: "Skills");

            migrationBuilder.RenameColumn(
                name: "FreelancerId",
                table: "Skills",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Skills_FreelancerId",
                table: "Skills",
                newName: "IX_Skills_UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Skills_Freelancers_UserId",
                table: "Skills",
                column: "UserId",
                principalTable: "Freelancers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
