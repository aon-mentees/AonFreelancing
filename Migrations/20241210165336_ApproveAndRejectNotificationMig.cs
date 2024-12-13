using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AonFreelancing.Migrations
{
    /// <inheritdoc />
    public partial class ApproveAndRejectNotificationMig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BidApprovalNotification",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    ProjectId = table.Column<long>(type: "bigint", nullable: false),
                    ApproverId = table.Column<long>(type: "bigint", nullable: false),
                    ApproverName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BidId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BidApprovalNotification", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BidApprovalNotification_AspNetUsers_ApproverId",
                        column: x => x.ApproverId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BidApprovalNotification_Bids_BidId",
                        column: x => x.BidId,
                        principalTable: "Bids",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BidApprovalNotification_Notifications_Id",
                        column: x => x.Id,
                        principalTable: "Notifications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BidRejectionNotification",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    ProjectId = table.Column<long>(type: "bigint", nullable: false),
                    RejectorId = table.Column<long>(type: "bigint", nullable: false),
                    RejectorName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BidId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BidRejectionNotification", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BidRejectionNotification_AspNetUsers_RejectorId",
                        column: x => x.RejectorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BidRejectionNotification_Bids_BidId",
                        column: x => x.BidId,
                        principalTable: "Bids",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BidRejectionNotification_Notifications_Id",
                        column: x => x.Id,
                        principalTable: "Notifications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BidApprovalNotification_ApproverId",
                table: "BidApprovalNotification",
                column: "ApproverId");

            migrationBuilder.CreateIndex(
                name: "IX_BidApprovalNotification_BidId",
                table: "BidApprovalNotification",
                column: "BidId");

            migrationBuilder.CreateIndex(
                name: "IX_BidRejectionNotification_BidId",
                table: "BidRejectionNotification",
                column: "BidId");

            migrationBuilder.CreateIndex(
                name: "IX_BidRejectionNotification_RejectorId",
                table: "BidRejectionNotification",
                column: "RejectorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BidApprovalNotification");

            migrationBuilder.DropTable(
                name: "BidRejectionNotification");
        }
    }
}
