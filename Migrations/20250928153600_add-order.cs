using KiCData.Models;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KiCData.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(KiCdbContext))]
    [Migration("20250928153600_add-order")]
    public partial class addOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Order",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    SquareOrderId = table.Column<string>(type: "varchar(50)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OrderDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    ItemsTotal = table.Column<int>(type: "int", nullable: true),
                    Discounts = table.Column<int>(type: "int", nullable: true),
                    SubTotal = table.Column<int>(type: "int", nullable: true),
                    Taxes = table.Column<int>(type: "int", nullable: true),
                    GrandTotal = table.Column<int>(type: "int", nullable: true),
                    PaymentsTotal = table.Column<int>(type: "int", nullable: true),
                    RefundsTotal = table.Column<int>(type: "int", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Order", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Order");
        }
    }
}
