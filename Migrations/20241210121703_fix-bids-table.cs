using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AonFreelancing.Migrations
{
    /// <inheritdoc />
    public partial class fixbidstable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bids_Clients_ClientId",
                table: "Bids");

            migrationBuilder.DropForeignKey(
                name: "FK_Bids_SystemUsers_SystemUserId",
                table: "Bids");

            migrationBuilder.DropIndex(
                name: "IX_Bids_ClientId",
                table: "Bids");

            migrationBuilder.DropIndex(
                name: "IX_Bids_SystemUserId",
                table: "Bids");

            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "Bids");

            migrationBuilder.DropColumn(
                name: "SystemUserId",
                table: "Bids");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ClientId",
                table: "Bids",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "SystemUserId",
                table: "Bids",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bids_ClientId",
                table: "Bids",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_Bids_SystemUserId",
                table: "Bids",
                column: "SystemUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bids_Clients_ClientId",
                table: "Bids",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Bids_SystemUsers_SystemUserId",
                table: "Bids",
                column: "SystemUserId",
                principalTable: "SystemUsers",
                principalColumn: "Id");
        }
    }
}
