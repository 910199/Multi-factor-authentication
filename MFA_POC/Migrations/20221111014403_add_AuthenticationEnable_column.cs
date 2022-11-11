using Microsoft.EntityFrameworkCore.Migrations;

namespace MFA_POC.Migrations
{
    public partial class add_AuthenticationEnable_column : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AuthenticationEnable",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AuthenticationEnable",
                table: "Users");
        }
    }
}
