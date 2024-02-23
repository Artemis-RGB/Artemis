using LiteDB;
using Newtonsoft.Json.Linq;

namespace Artemis.Storage.Migrations;

internal interface IProfileMigration
{
    int Version { get; }
    void Migrate(JObject profileJson);
    void Migrate(BsonDocument profileBson);
}