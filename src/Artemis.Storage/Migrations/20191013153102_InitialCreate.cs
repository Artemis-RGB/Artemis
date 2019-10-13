using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Artemis.Storage.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Folders",
                columns: table => new
                {
                    Guid = table.Column<string>(nullable: false),
                    Order = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    FolderEntityGuid = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Folders", x => x.Guid);
                    table.ForeignKey(
                        name: "FK_Folders_Folders_FolderEntityGuid",
                        column: x => x.FolderEntityGuid,
                        principalTable: "Folders",
                        principalColumn: "Guid",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PluginSettings",
                columns: table => new
                {
                    PluginGuid = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Value = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PluginSettings", x => new { x.Name, x.PluginGuid });
                });

            migrationBuilder.CreateTable(
                name: "Settings",
                columns: table => new
                {
                    Name = table.Column<string>(nullable: false),
                    Value = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settings", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "Surfaces",
                columns: table => new
                {
                    Guid = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    IsActive = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Surfaces", x => x.Guid);
                });

            migrationBuilder.CreateTable(
                name: "Layers",
                columns: table => new
                {
                    Guid = table.Column<string>(nullable: false),
                    Order = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    FolderEntityGuid = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Layers", x => x.Guid);
                    table.ForeignKey(
                        name: "FK_Layers_Folders_FolderEntityGuid",
                        column: x => x.FolderEntityGuid,
                        principalTable: "Folders",
                        principalColumn: "Guid",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Profiles",
                columns: table => new
                {
                    Guid = table.Column<string>(nullable: false),
                    PluginGuid = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    RootFolderId = table.Column<int>(nullable: false),
                    RootFolderGuid = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Profiles", x => x.Guid);
                    table.ForeignKey(
                        name: "FK_Profiles_Folders_RootFolderGuid",
                        column: x => x.RootFolderGuid,
                        principalTable: "Folders",
                        principalColumn: "Guid",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SurfacePositionEntity",
                columns: table => new
                {
                    Guid = table.Column<string>(nullable: false),
                    DeviceId = table.Column<int>(nullable: false),
                    DeviceName = table.Column<string>(nullable: true),
                    DeviceModel = table.Column<string>(nullable: true),
                    DeviceManufacturer = table.Column<string>(nullable: true),
                    X = table.Column<double>(nullable: false),
                    Y = table.Column<double>(nullable: false),
                    Rotation = table.Column<double>(nullable: false),
                    ZIndex = table.Column<int>(nullable: false),
                    SurfaceId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SurfacePositionEntity", x => x.Guid);
                    table.ForeignKey(
                        name: "FK_SurfacePositionEntity_Surfaces_SurfaceId",
                        column: x => x.SurfaceId,
                        principalTable: "Surfaces",
                        principalColumn: "Guid",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LayerSettings",
                columns: table => new
                {
                    Guid = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Value = table.Column<string>(nullable: true),
                    LayerEntityGuid = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LayerSettings", x => x.Guid);
                    table.ForeignKey(
                        name: "FK_LayerSettings_Layers_LayerEntityGuid",
                        column: x => x.LayerEntityGuid,
                        principalTable: "Layers",
                        principalColumn: "Guid",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Leds",
                columns: table => new
                {
                    Guid = table.Column<string>(nullable: false),
                    LedName = table.Column<string>(nullable: true),
                    LimitedToDevice = table.Column<string>(nullable: true),
                    LayerId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Leds", x => x.Guid);
                    table.ForeignKey(
                        name: "FK_Leds_Layers_LayerId",
                        column: x => x.LayerId,
                        principalTable: "Layers",
                        principalColumn: "Guid",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Keypoints",
                columns: table => new
                {
                    Guid = table.Column<string>(nullable: false),
                    Time = table.Column<int>(nullable: false),
                    Value = table.Column<string>(nullable: true),
                    LayerSettingEntityGuid = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Keypoints", x => x.Guid);
                    table.ForeignKey(
                        name: "FK_Keypoints_LayerSettings_LayerSettingEntityGuid",
                        column: x => x.LayerSettingEntityGuid,
                        principalTable: "LayerSettings",
                        principalColumn: "Guid",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Folders_FolderEntityGuid",
                table: "Folders",
                column: "FolderEntityGuid");

            migrationBuilder.CreateIndex(
                name: "IX_Keypoints_LayerSettingEntityGuid",
                table: "Keypoints",
                column: "LayerSettingEntityGuid");

            migrationBuilder.CreateIndex(
                name: "IX_Layers_FolderEntityGuid",
                table: "Layers",
                column: "FolderEntityGuid");

            migrationBuilder.CreateIndex(
                name: "IX_LayerSettings_LayerEntityGuid",
                table: "LayerSettings",
                column: "LayerEntityGuid");

            migrationBuilder.CreateIndex(
                name: "IX_Leds_LayerId",
                table: "Leds",
                column: "LayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Profiles_RootFolderGuid",
                table: "Profiles",
                column: "RootFolderGuid");

            migrationBuilder.CreateIndex(
                name: "IX_SurfacePositionEntity_SurfaceId",
                table: "SurfacePositionEntity",
                column: "SurfaceId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Keypoints");

            migrationBuilder.DropTable(
                name: "Leds");

            migrationBuilder.DropTable(
                name: "PluginSettings");

            migrationBuilder.DropTable(
                name: "Profiles");

            migrationBuilder.DropTable(
                name: "Settings");

            migrationBuilder.DropTable(
                name: "SurfacePositionEntity");

            migrationBuilder.DropTable(
                name: "LayerSettings");

            migrationBuilder.DropTable(
                name: "Surfaces");

            migrationBuilder.DropTable(
                name: "Layers");

            migrationBuilder.DropTable(
                name: "Folders");
        }
    }
}
