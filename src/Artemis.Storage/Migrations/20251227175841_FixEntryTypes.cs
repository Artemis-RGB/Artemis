using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Artemis.Storage.Migrations
{
    /// <inheritdoc />
    public partial class FixEntryTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // These were scrambled in Artemis 1.2025.1223.2 due to a bad GraphQL introspection copy-paste
            migrationBuilder.Sql("UPDATE Entries SET EntryType = 0 WHERE Metadata LIKE '%PluginId%';");
            migrationBuilder.Sql("UPDATE Entries SET EntryType = 1 WHERE Metadata LIKE '%ProfileId%';");
            migrationBuilder.Sql("UPDATE Entries SET EntryType = 2 WHERE Metadata = '{}';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
