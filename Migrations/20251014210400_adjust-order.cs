using KiCData.Models;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KiCData.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(KiCdbContext))]
    [Migration("20251014210400_adjust-order")]
    public partial class adjustOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "SquareOrderId",
                table: "Order",
                type: "varchar(100)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "SquareOrderId",
                table: "Order",
                type: "varchar(50)");
        }
    }
}
