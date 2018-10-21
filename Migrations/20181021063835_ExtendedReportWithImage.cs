using Microsoft.EntityFrameworkCore.Migrations;

namespace NASATest2018.Migrations
{
    public partial class ExtendedReportWithImage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImagePath",
                table: "Reports",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImagePath",
                table: "Reports");
        }
    }
}
