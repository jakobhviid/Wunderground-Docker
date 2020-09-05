using Microsoft.EntityFrameworkCore.Migrations;

namespace Server.Migrations
{
    public partial class Forecast : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GeoCode",
                table: "Subscriptions",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "Subscriptions",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GeoCode",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "Subscriptions");
        }
    }
}
