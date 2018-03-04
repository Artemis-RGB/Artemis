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
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FolderEntityId = table.Column<int>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    Order = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Folders", x => x.Id);
                    table.ForeignKey(
                        "FK_Folders_Folders_FolderEntityId",
                        x => x.FolderEntityId,
                        "Folders",
                        "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                "Settings",
                table => new
                {
                    Name = table.Column<string>(nullable: false),
                    Value = table.Column<string>(nullable: true)
                },
                constraints: table => { table.PrimaryKey("PK_Settings", x => x.Name); });

            migrationBuilder.CreateTable(
                "Layers",
                table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FolderEntityId = table.Column<int>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    Order = table.Column<int>(nullable: false),
                    Type = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Layers", x => x.Id);
                    table.ForeignKey(
                        "FK_Layers_Folders_FolderEntityId",
                        x => x.FolderEntityId,
                        "Folders",
                        "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                "Profiles",
                table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Module = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    RootFolderId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Profiles", x => x.Id);
                    table.ForeignKey(
                        "FK_Profiles_Folders_RootFolderId",
                        x => x.RootFolderId,
                        "Folders",
                        "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                "LayerSettings",
                table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LayerEntityId = table.Column<int>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    Value = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LayerSettings", x => x.Id);
                    table.ForeignKey(
                        "FK_LayerSettings_Layers_LayerEntityId",
                        x => x.LayerEntityId,
                        "Layers",
                        "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                "Leds",
                table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LayerId = table.Column<int>(nullable: false),
                    LedName = table.Column<string>(nullable: true),
                    LimitedToDevice = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Leds", x => x.Id);
                    table.ForeignKey(
                        "FK_Leds_Layers_LayerId",
                        x => x.LayerId,
                        "Layers",
                        "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                "Keypoints",
                table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LayerSettingEntityId = table.Column<int>(nullable: true),
                    Time = table.Column<int>(nullable: false),
                    Value = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Keypoints", x => x.Id);
                    table.ForeignKey(
                        "FK_Keypoints_LayerSettings_LayerSettingEntityId",
                        x => x.LayerSettingEntityId,
                        "LayerSettings",
                        "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                "IX_Folders_FolderEntityId",
                "Folders",
                "FolderEntityId");

            migrationBuilder.CreateIndex(
                "IX_Keypoints_LayerSettingEntityId",
                "Keypoints",
                "LayerSettingEntityId");

            migrationBuilder.CreateIndex(
                "IX_Layers_FolderEntityId",
                "Layers",
                "FolderEntityId");

            migrationBuilder.CreateIndex(
                "IX_LayerSettings_LayerEntityId",
                "LayerSettings",
                "LayerEntityId");

            migrationBuilder.CreateIndex(
                "IX_Leds_LayerId",
                "Leds",
                "LayerId");

            migrationBuilder.CreateIndex(
                "IX_Profiles_RootFolderId",
                "Profiles",
                "RootFolderId");
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