using Microsoft.EntityFrameworkCore.Migrations;

namespace Pingtimeout.Web.Data.Migrations
{
    public partial class AddedReservationOverride : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReservationOverride",
                table: "Seats",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReservationOverride",
                table: "Seats");
        }
    }
}
