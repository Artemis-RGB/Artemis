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
                    Order = table.Column<int>(),
                    Name = table.Column<string>(nullable: true),
                    FolderEntityGuid = table.Column<string>(nullable: true)
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
                "PluginSettings",
                table => new
                {
                    PluginGuid = table.Column<Guid>(),
                    Name = table.Column<string>(),
                    Value = table.Column<string>(nullable: true)
                },
                constraints: table => { table.PrimaryKey("PK_PluginSettings", x => new {x.Name, x.PluginGuid}); });

            migrationBuilder.CreateTable(
                "Settings",
                table => new
                {
                    Name = table.Column<string>(),
                    Value = table.Column<string>(nullable: true)
                },
                constraints: table => { table.PrimaryKey("PK_Settings", x => x.Name); });

            migrationBuilder.CreateTable(
                "Surfaces",
                table => new
                {
                    Guid = table.Column<string>(),
                    Name = table.Column<string>(nullable: true),
                    IsActive = table.Column<bool>()
                },
                constraints: table => { table.PrimaryKey("PK_Surfaces", x => x.Guid); });

            migrationBuilder.CreateTable(
                "Layers",
                table => new
                {
                    Guid = table.Column<string>(),
                    Order = table.Column<int>(),
                    Name = table.Column<string>(nullable: true),
                    FolderEntityGuid = table.Column<string>(nullable: true)
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
                    PluginGuid = table.Column<Guid>(),
                    Name = table.Column<string>(nullable: true),
                    RootFolderId = table.Column<int>(),
                    RootFolderGuid = table.Column<string>(nullable: true)
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
                "DeviceEntity",
                table => new
                {
                    Guid = table.Column<string>(),
                    DeviceHashCode = table.Column<int>(),
                    X = table.Column<double>(),
                    Y = table.Column<double>(),
                    Rotation = table.Column<double>(),
                    ZIndex = table.Column<int>(),
                    SurfaceId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceEntity", x => x.Guid);
                    table.ForeignKey(
                        "FK_DeviceEntity_Surfaces_SurfaceId",
                        x => x.SurfaceId,
                        "Surfaces",
                        "Guid",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                "LayerSettings",
                table => new
                {
                    Guid = table.Column<string>(),
                    Name = table.Column<string>(nullable: true),
                    Value = table.Column<string>(nullable: true),
                    LayerEntityGuid = table.Column<string>(nullable: true)
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
                    LedName = table.Column<string>(nullable: true),
                    LimitedToDevice = table.Column<string>(nullable: true),
                    LayerId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Leds", x => x.Guid);
                    table.ForeignKey(
                        "FK_Leds_Layers_LayerId",
                        x => x.LayerId,
                        "Layers",
                        "Guid",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                "Keypoints",
                table => new
                {
                    Guid = table.Column<string>(),
                    Time = table.Column<int>(),
                    Value = table.Column<string>(nullable: true),
                    LayerSettingEntityGuid = table.Column<string>(nullable: true)
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
                "IX_DeviceEntity_SurfaceId",
                "DeviceEntity",
                "SurfaceId");

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
                "IX_Leds_LayerId",
                "Leds",
                "LayerId");

            migrationBuilder.CreateIndex(
                "IX_Profiles_RootFolderGuid",
                "Profiles",
                "RootFolderGuid");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                "DeviceEntity");

            migrationBuilder.DropTable(
                "Keypoints");

            migrationBuilder.DropTable(
                "Leds");

            migrationBuilder.DropTable(
                "PluginSettings");

            migrationBuilder.DropTable(
                "Profiles");

            migrationBuilder.DropTable(
                "Settings");

            migrationBuilder.DropTable(
                "Surfaces");

            migrationBuilder.DropTable(
                "LayerSettings");

            migrationBuilder.DropTable(
                "Layers");

            migrationBuilder.DropTable(
                "Folders");
        }
    }
}