using Newtonsoft.Json.Linq;

namespace Artemis.Core.Services;

internal interface IProfileMigration
{
    int Version { get; }
    void Migrate(JObject profileJson);
}