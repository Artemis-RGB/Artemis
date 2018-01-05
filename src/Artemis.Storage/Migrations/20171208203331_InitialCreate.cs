using Microsoft.EntityFrameworkCore.Migrations;

namespace Artemis.Storage.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                "Profiles",
                table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table => { table.PrimaryKey("PK_Profiles", x => x.Id); });

            migrationBuilder.CreateTable(
                "Settings",
                table => new
                {
                    Name = table.Column<string>(nullable: false),
                    Value = table.Column<string>(nullable: true)
                },
                constraints: table => { table.PrimaryKey("PK_Settings", x => x.Name); });

            migrationBuilder.CreateTable(
                "Layer",
                table => new
                {
                    ProfileId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Layer", x => new {x.ProfileId, x.Name});
                    table.ForeignKey(
                        "FK_Layer_Profiles_ProfileId",
                        x => x.ProfileId,
                        "Profiles",
                        "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                "Layer");

            migrationBuilder.DropTable(
                "Settings");

            migrationBuilder.DropTable(
                "Profiles");
        }
    }
}