using System.Text.Json.Nodes;

namespace Artemis.Storage.Legacy.Migrations;

public interface IProfileMigration
{
    int Version { get; }
    void Migrate(JsonObject configurationJson, JsonObject profileJson);
}