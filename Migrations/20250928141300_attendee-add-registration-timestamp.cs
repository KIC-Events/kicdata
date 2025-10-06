using KiCData.Models;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KiCData.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(KiCdbContext))]
    [Migration("20250928141300_attendee-add-registration-timestamp")]
    public partial class AttendeeAddRegistrationTimestamp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                    name: "RegistrationConfirmationEmailTimestamp",
                    table: "Attendee",
                    type: "datetime",
                    nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RegistrationConfirmationEmailTimestamp",
                table: "Attendee");
        }
    }
}