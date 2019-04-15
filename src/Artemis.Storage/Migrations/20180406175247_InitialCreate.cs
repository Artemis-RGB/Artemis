using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Artemis.Storage.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                "Folders",
                table => new
                {
                    Guid = table.Column<string>(),
                    FolderEntityGuid = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    Order = table.Column<int>()
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Folders", x => x.Guid);
                    table.ForeignKey(
                        "FK_Folders_Folders_FolderEntityGuid",
                        x => x.FolderEntityGuid,
                        "Folders",
                        "Guid",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                "Settings",
                table => new
                {
                    Name = table.Column<string>(),
                    Value = table.Column<string>(nullable: true)
                },
                constraints: table => { table.PrimaryKey("PK_Settings", x => x.Name); });

            migrationBuilder.CreateTable(
                "Layers",
                table => new
                {
                    Guid = table.Column<string>(),
                    FolderEntityGuid = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    Order = table.Column<int>()
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Layers", x => x.Guid);
                    table.ForeignKey(
                        "FK_Layers_Folders_FolderEntityGuid",
                        x => x.FolderEntityGuid,
                        "Folders",
                        "Guid",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                "Profiles",
                table => new
                {
                    Guid = table.Column<string>(),
                    Name = table.Column<string>(nullable: true),
                    PluginGuid = table.Column<Guid>(),
                    RootFolderGuid = table.Column<string>(nullable: true),
                    RootFolderId = table.Column<int>()
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Profiles", x => x.Guid);
                    table.ForeignKey(
                        "FK_Profiles_Folders_RootFolderGuid",
                        x => x.RootFolderGuid,
                        "Folders",
                        "Guid",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                "LayerSettings",
                table => new
                {
                    Guid = table.Column<string>(),
                    LayerEntityGuid = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    Value = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LayerSettings", x => x.Guid);
                    table.ForeignKey(
                        "FK_LayerSettings_Layers_LayerEntityGuid",
                        x => x.LayerEntityGuid,
                        "Layers",
                        "Guid",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                "Leds",
                table => new
                {
                    Guid = table.Column<string>(),
                    LayerGuid = table.Column<string>(nullable: true),
                    LayerId = table.Column<int>(),
                    LedName = table.Column<string>(nullable: true),
                    LimitedToDevice = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Leds", x => x.Guid);
                    table.ForeignKey(
                        "FK_Leds_Layers_LayerGuid",
                        x => x.LayerGuid,
                        "Layers",
                        "Guid",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                "Keypoints",
                table => new
                {
                    Guid = table.Column<string>(),
                    LayerSettingEntityGuid = table.Column<string>(nullable: true),
                    Time = table.Column<int>(),
                    Value = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Keypoints", x => x.Guid);
                    table.ForeignKey(
                        "FK_Keypoints_LayerSettings_LayerSettingEntityGuid",
                        x => x.LayerSettingEntityGuid,
                        "LayerSettings",
                        "Guid",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                "IX_Folders_FolderEntityGuid",
                "Folders",
                "FolderEntityGuid");

            migrationBuilder.CreateIndex(
                "IX_Keypoints_LayerSettingEntityGuid",
                "Keypoints",
                "LayerSettingEntityGuid");

            migrationBuilder.CreateIndex(
                "IX_Layers_FolderEntityGuid",
                "Layers",
                "FolderEntityGuid");

            migrationBuilder.CreateIndex(
                "IX_LayerSettings_LayerEntityGuid",
                "LayerSettings",
                "LayerEntityGuid");

            migrationBuilder.CreateIndex(
                "IX_Leds_LayerGuid",
                "Leds",
                "LayerGuid");

            migrationBuilder.CreateIndex(
                "IX_Profiles_RootFolderGuid",
                "Profiles",
                "RootFolderGuid");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                "Keypoints");

            migrationBuilder.DropTable(
                "Leds");

            migrationBuilder.DropTable(
                "Profiles");

            migrationBuilder.DropTable(
                "Settings");

            migrationBuilder.DropTable(
                "LayerSettings");

            migrationBuilder.DropTable(
                "Layers");

            migrationBuilder.DropTable(
                "Folders");
        }
    }
}