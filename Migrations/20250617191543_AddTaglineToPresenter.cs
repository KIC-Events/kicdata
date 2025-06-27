using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KiCData.Migrations
{
    /// <inheritdoc />
    public partial class AddTaglineToPresenter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Tagline",
                table: "Presenter",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Tagline",
                table: "Presenter");
        }
    }
}
