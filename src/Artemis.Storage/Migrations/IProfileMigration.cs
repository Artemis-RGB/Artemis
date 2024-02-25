using Newtonsoft.Json.Linq;

namespace Artemis.Storage.Migrations;

public interface IProfileMigration
{
    int Version { get; }
    void Migrate(JObject configurationJson, JObject profileJson);
}