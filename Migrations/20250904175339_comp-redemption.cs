using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KiCData.Migrations
{
    /// <inheritdoc />
    public partial class compredemption : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RemainingRedemptions",
                table: "TicketComp",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EventId",
                table: "Sponsor",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Sponsor_EventId",
                table: "Sponsor",
                column: "EventId");

            migrationBuilder.AddForeignKey(
                name: "FK_Sponsor_Event_EventId",
                table: "Sponsor",
                column: "EventId",
                principalTable: "Event",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sponsor_Event_EventId",
                table: "Sponsor");

            migrationBuilder.DropIndex(
                name: "IX_Sponsor_EventId",
                table: "Sponsor");

            migrationBuilder.DropColumn(
                name: "RemainingRedemptions",
                table: "TicketComp");

            migrationBuilder.DropColumn(
                name: "EventId",
                table: "Sponsor");
        }
    }
}
