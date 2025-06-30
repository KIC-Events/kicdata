using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KiCData.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePresenterStructure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Member_Presenter_PresenterId",
                table: "Member");

            migrationBuilder.DropForeignKey(
                name: "FK_Presentation_Presenter_PresenterId",
                table: "Presentation");

            migrationBuilder.DropIndex(
                name: "IX_Presentation_PresenterId",
                table: "Presentation");

            migrationBuilder.DropIndex(
                name: "IX_Member_PresenterId",
                table: "Member");

            migrationBuilder.DropColumn(
                name: "PresenterId",
                table: "Presentation");

            migrationBuilder.DropColumn(
                name: "PresenterId",
                table: "Member");

            migrationBuilder.AddColumn<int>(
                name: "MemberId",
                table: "Presenter",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PresentationPresenter",
                columns: table => new
                {
                    PresentationsId = table.Column<int>(type: "int", nullable: false),
                    PresentersId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PresentationPresenter", x => new { x.PresentationsId, x.PresentersId });
                    table.ForeignKey(
                        name: "FK_PresentationPresenter_Presentation_PresentationsId",
                        column: x => x.PresentationsId,
                        principalTable: "Presentation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PresentationPresenter_Presenter_PresentersId",
                        column: x => x.PresentersId,
                        principalTable: "Presenter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PresenterSocial",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Platform = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Handle = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PresenterId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PresenterSocial", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PresenterSocial_Presenter_PresenterId",
                        column: x => x.PresenterId,
                        principalTable: "Presenter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Presenter_MemberId",
                table: "Presenter",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_PresentationPresenter_PresentersId",
                table: "PresentationPresenter",
                column: "PresentersId");

            migrationBuilder.CreateIndex(
                name: "IX_PresenterSocial_PresenterId",
                table: "PresenterSocial",
                column: "PresenterId");

            migrationBuilder.AddForeignKey(
                name: "FK_Presenter_Member_MemberId",
                table: "Presenter",
                column: "MemberId",
                principalTable: "Member",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Presenter_Member_MemberId",
                table: "Presenter");

            migrationBuilder.DropTable(
                name: "PresentationPresenter");

            migrationBuilder.DropTable(
                name: "PresenterSocial");

            migrationBuilder.DropIndex(
                name: "IX_Presenter_MemberId",
                table: "Presenter");

            migrationBuilder.DropColumn(
                name: "MemberId",
                table: "Presenter");

            migrationBuilder.AddColumn<int>(
                name: "PresenterId",
                table: "Presentation",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PresenterId",
                table: "Member",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Presentation_PresenterId",
                table: "Presentation",
                column: "PresenterId");

            migrationBuilder.CreateIndex(
                name: "IX_Member_PresenterId",
                table: "Member",
                column: "PresenterId");

            migrationBuilder.AddForeignKey(
                name: "FK_Member_Presenter_PresenterId",
                table: "Member",
                column: "PresenterId",
                principalTable: "Presenter",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Presentation_Presenter_PresenterId",
                table: "Presentation",
                column: "PresenterId",
                principalTable: "Presenter",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
