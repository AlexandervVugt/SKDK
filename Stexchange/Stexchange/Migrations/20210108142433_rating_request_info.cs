using Microsoft.EntityFrameworkCore.Migrations;

namespace Stexchange.Migrations
{
    public partial class rating_request_info : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "plant_name",
                table: "RatingRequests",
                type: "varchar(30)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "request_quality",
                table: "RatingRequests",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "plant_name",
                table: "RatingRequests");

            migrationBuilder.DropColumn(
                name: "request_quality",
                table: "RatingRequests");
        }
    }
}
