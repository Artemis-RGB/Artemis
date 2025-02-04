using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Artemis.Storage.Migrations
{
    /// <inheritdoc />
    public partial class DevicesClearBrokenJson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE Devices SET InputMappings = \"[]\", InputIdentifiers = \"[]\"");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE Devices SET InputMappings = '{\"Capacity\":0}', InputIdentifiers = '{\"Capacity\":0}'");
        }
    }
}
