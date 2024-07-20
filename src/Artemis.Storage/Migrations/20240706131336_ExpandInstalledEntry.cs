using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Artemis.Storage.Migrations
{
    /// <inheritdoc />
    public partial class ExpandInstalledEntry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AutoUpdate",
                table: "Entries",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Categories",
                table: "Entries",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Entries",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<long>(
                name: "Downloads",
                table: "Entries",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "LatestReleaseId",
                table: "Entries",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Summary",
                table: "Entries",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            // Enable auto-update on all entries that are not profiles
            migrationBuilder.Sql("UPDATE Entries SET AutoUpdate = 1 WHERE EntryType != 2");
            
            // Enable auto-update on all entries of profiles that are fresh imports
            migrationBuilder.Sql("""
                UPDATE Entries
                SET AutoUpdate = 1
                WHERE EntryType = 2
                AND EXISTS (
                    SELECT 1
                    FROM ProfileContainers
                    WHERE json_extract(ProfileContainers.Profile, '$.Id') = json_extract(Entries.Metadata, '$.ProfileId')
                    AND json_extract(ProfileContainers.Profile, '$.IsFreshImport') = 1
                );
            """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AutoUpdate",
                table: "Entries");

            migrationBuilder.DropColumn(
                name: "Categories",
                table: "Entries");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Entries");

            migrationBuilder.DropColumn(
                name: "Downloads",
                table: "Entries");

            migrationBuilder.DropColumn(
                name: "LatestReleaseId",
                table: "Entries");

            migrationBuilder.DropColumn(
                name: "Summary",
                table: "Entries");
        }
    }
}
