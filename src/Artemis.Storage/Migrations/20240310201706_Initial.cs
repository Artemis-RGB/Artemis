using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Artemis.Storage.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Devices",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 512, nullable: false),
                    DeviceProvider = table.Column<string>(type: "TEXT", maxLength: 512, nullable: false),
                    X = table.Column<float>(type: "REAL", nullable: false),
                    Y = table.Column<float>(type: "REAL", nullable: false),
                    Rotation = table.Column<float>(type: "REAL", nullable: false),
                    Scale = table.Column<float>(type: "REAL", nullable: false),
                    ZIndex = table.Column<int>(type: "INTEGER", nullable: false),
                    RedScale = table.Column<float>(type: "REAL", nullable: false),
                    GreenScale = table.Column<float>(type: "REAL", nullable: false),
                    BlueScale = table.Column<float>(type: "REAL", nullable: false),
                    IsEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    PhysicalLayout = table.Column<int>(type: "INTEGER", nullable: false),
                    LogicalLayout = table.Column<string>(type: "TEXT", maxLength: 32, nullable: true),
                    LayoutType = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    LayoutParameter = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true),
                    Categories = table.Column<string>(type: "TEXT", nullable: false),
                    InputIdentifiers = table.Column<string>(type: "TEXT", nullable: false),
                    InputMappings = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Devices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Entries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    EntryId = table.Column<long>(type: "INTEGER", nullable: false),
                    EntryType = table.Column<int>(type: "INTEGER", nullable: false),
                    Author = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    ReleaseId = table.Column<long>(type: "INTEGER", nullable: false),
                    ReleaseVersion = table.Column<string>(type: "TEXT", nullable: false),
                    InstalledAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    Metadata = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Entries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Plugins",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PluginGuid = table.Column<Guid>(type: "TEXT", nullable: false),
                    IsEnabled = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Plugins", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PluginSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PluginGuid = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PluginSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProfileCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    IsCollapsed = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsSuspended = table.Column<bool>(type: "INTEGER", nullable: false),
                    Order = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfileCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Releases",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Version = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    InstalledAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Releases", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PluginFeatures",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Type = table.Column<string>(type: "TEXT", nullable: false),
                    IsEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    PluginEntityId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PluginFeatures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PluginFeatures_Plugins_PluginEntityId",
                        column: x => x.PluginEntityId,
                        principalTable: "Plugins",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ProfileContainers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Icon = table.Column<byte[]>(type: "BLOB", nullable: false),
                    ProfileCategoryId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProfileConfiguration = table.Column<string>(type: "TEXT", nullable: false),
                    Profile = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfileContainers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProfileContainers_ProfileCategories_ProfileCategoryId",
                        column: x => x.ProfileCategoryId,
                        principalTable: "ProfileCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Entries_EntryId",
                table: "Entries",
                column: "EntryId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PluginFeatures_PluginEntityId",
                table: "PluginFeatures",
                column: "PluginEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_Plugins_PluginGuid",
                table: "Plugins",
                column: "PluginGuid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PluginSettings_Name_PluginGuid",
                table: "PluginSettings",
                columns: new[] { "Name", "PluginGuid" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PluginSettings_PluginGuid",
                table: "PluginSettings",
                column: "PluginGuid");

            migrationBuilder.CreateIndex(
                name: "IX_ProfileCategories_Name",
                table: "ProfileCategories",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProfileContainers_ProfileCategoryId",
                table: "ProfileContainers",
                column: "ProfileCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Releases_InstalledAt",
                table: "Releases",
                column: "InstalledAt");

            migrationBuilder.CreateIndex(
                name: "IX_Releases_Version",
                table: "Releases",
                column: "Version",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Devices");

            migrationBuilder.DropTable(
                name: "Entries");

            migrationBuilder.DropTable(
                name: "PluginFeatures");

            migrationBuilder.DropTable(
                name: "PluginSettings");

            migrationBuilder.DropTable(
                name: "ProfileContainers");

            migrationBuilder.DropTable(
                name: "Releases");

            migrationBuilder.DropTable(
                name: "Plugins");

            migrationBuilder.DropTable(
                name: "ProfileCategories");
        }
    }
}
