using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Artemis.Storage.Migrations
{
    public partial class SettingsPluginGuid : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PluginGuid",
                table: "Settings",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Settings_Name_PluginGuid",
                table: "Settings",
                columns: new[] { "Name", "PluginGuid" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Settings_Name_PluginGuid",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "PluginGuid",
                table: "Settings");
        }
    }
}
