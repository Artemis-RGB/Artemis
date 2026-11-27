using System.Text.Json.Nodes;
using Artemis.Core.Tests.Infrastructure.Storage;
using Artemis.Storage.Migrations.Profile;

namespace Artemis.Core.Tests.Storage.Migrations;

public class M0004NodeStorageTests
{
    private readonly RecordingLogger _log = new();
    private readonly M0004NodeStorage _migration;

    public M0004NodeStorageTests()
    {
        _migration = new M0004NodeStorage(_log.Logger);
    }

    [Fact]
    public void Version_IsFour()
    {
        _migration.Version.ShouldBe(4);
    }

    [Fact]
    public void Migrate_ConvertsTheStorageOfEveryNode()
    {
        (JsonObject configuration, JsonObject profile) = Version3Profile();

        _migration.Migrate(configuration, profile);

        // Only MigrateProfile sets the version
        configuration["Version"] = 4;
        configuration.ShouldBeJson(OldProfileJson.MigratedConfiguration);
        profile.ShouldBeJson(OldProfileJson.MigratedProfile);
        _log.Errors.ShouldBeEmpty();
    }

    [Theory]
    [InlineData(OldProfileJson.FolderConditionNodeId, """{"Path":"Player.IsAlive","DataModelId":"Artemis.Plugins.Modules.Game-12345678"}""")]
    [InlineData(OldProfileJson.LayerBrushNodeId, """[{"Color":"#ffff0000","Position":0.0},{"Color":"#ff0000ff","Position":1.0}]""")]
    [InlineData(OldProfileJson.FolderEffectNodeId, "\"1 + 1\"")]
    [InlineData(OldProfileJson.LayerConditionNodeId, "true")]
    [InlineData(OldProfileJson.LayerTransformNodeId, "50.0")]
    public void Migrate_ConvertsStorageEverywhereInTheProfile(string nodeId, string expectedStorage)
    {
        (JsonObject configuration, JsonObject profile) = Version3Profile();

        _migration.Migrate(configuration, profile);

        profile.FindNode(nodeId)["Storage"]!.GetValue<string>().ShouldBe(expectedStorage);
    }

    [Fact]
    public void Migrate_ConvertsTheStorageOfActivationConditionNodes()
    {
        (JsonObject configuration, JsonObject profile) = Version3Profile();

        _migration.Migrate(configuration, profile);

        configuration.FindNode(OldProfileJson.ActivationDataModelNodeId)["Storage"]!.GetValue<string>()
            .ShouldBe("""{"Path":"IsInGame","DataModelId":"Artemis.Plugins.Modules.Game-12345678"}""");
        configuration.FindNode(OldProfileJson.ActivationExitNodeId)["Storage"]!.GetValue<string>().ShouldBe("");
    }

    [Fact]
    public void Migrate_RunningItTwice_GivesTheSameResult()
    {
        (JsonObject configuration, JsonObject profile) = Version3Profile();

        _migration.Migrate(configuration, profile);
        _migration.Migrate(configuration, profile);

        profile.ShouldBeJson(OldProfileJson.MigratedProfile);
    }

    [Theory]
    // A plain object loses its type name, also in its children
    [InlineData("""{"$type":"Artemis.Storage.Entities.Profile.DataModelPathEntity, Artemis.Storage","Path":"Value"}""", """{"Path":"Value"}""")]
    [InlineData("""{"$type":"A","Child":{"$type":"B","Items":{"$type":"List","$values":[{"$type":"C","X":1}]}}}""", """{"Child":{"Items":[{"X":1}]}}""")]
    // A list becomes its values
    [InlineData("""{"$type":"List","$values":[{"$type":"Item","X":1},2]}""", """[{"X":1},2]""")]
    [InlineData("""{"$type":"List","$values":[]}""", "[]")]
    // Anything that isn't an object stays as it is
    [InlineData("5", "5")]
    [InlineData("\"text\"", "\"text\"")]
    [InlineData("""[{"$type":"Item"}]""", """[{"$type":"Item"}]""")]
    [InlineData("", "")]
    [InlineData(null, null)]
    // Already converted storage
    [InlineData("""{"Path":"Value"}""", """{"Path":"Value"}""")]
    public void MigrateNodeStorageJson_ConvertsOldStorage(string? storage, string? expected)
    {
        M0004NodeStorage.MigrateNodeStorageJson(storage, _log.Logger).ShouldBe(expected);
        _log.Errors.ShouldBeEmpty();
    }

    [Fact]
    public void MigrateNodeStorageJson_UsesTheShortTypeNamesOfConditionsAndAdaptionHints()
    {
        const string storage = """{"$type":"Artemis.Storage.Entities.Profile.Conditions.StaticConditionEntity, Artemis.Storage","PlayMode":1}""";

        M0004NodeStorage.MigrateNodeStorageJson(storage, _log.Logger).ShouldBe("""{"$type":"Static","PlayMode":1}""");
    }

    [Fact]
    public void MigrateNodeStorageJson_WritesTextEscaped()
    {
        // The storage is written again with the default encoder, which escapes characters like + and quotes. Reading it
        // back gives the same values.
        const string storage = """{"$type":"Expression","Text":"1 + 'a'"}""";

        M0004NodeStorage.MigrateNodeStorageJson(storage, _log.Logger).ShouldBe("{\"Text\":\"1 \\u002B \\u0027a\\u0027\"}");
    }

    [Theory]
    [InlineData("{not json")]
    [InlineData("""{"$type":"List","$values":5}""")]
    public void MigrateNodeStorageJson_BrokenStorage_IsLeftAloneAndLogged(string storage)
    {
        M0004NodeStorage.MigrateNodeStorageJson(storage, _log.Logger).ShouldBe(storage);

        _log.Errors.ShouldHaveSingleItem().MessageTemplate.Text.ShouldBe("Failed to migrate node storage JSON");
    }

    [Fact]
    public void Migrate_NodeWithoutStorage_LeavesTheStorageOut()
    {
        (JsonObject configuration, JsonObject profile) = Version3Profile();
        JsonObject node = profile.FindNode(OldProfileJson.LayerTransformNodeId);
        node.Remove("Storage");

        _migration.Migrate(configuration, profile);

        node.ContainsKey("Storage").ShouldBeFalse();
    }

    [Fact]
    public void Migrate_ProfileWithoutNodes_DoesNothing()
    {
        const string configurationJson = """{"Name": "Empty", "ActivationCondition": null}""";
        const string profileJson = """
                                   {
                                     "Folders": [null, {"Name": "Folder", "LayerEffects": [{"PropertyGroup": null}]}],
                                     "Layers": [null, {"Name": "Layer", "DisplayCondition": {"$type": "AlwaysOn"}, "GeneralPropertyGroup": {"Properties": [{"DataBinding": null}]}}]
                                   }
                                   """;
        JsonObject configuration = OldProfileJson.Parse(configurationJson);
        JsonObject profile = OldProfileJson.Parse(profileJson);

        _migration.Migrate(configuration, profile);

        configuration.ShouldBeJson(configurationJson);
        profile.ShouldBeJson(profileJson);
    }

    [Fact]
    public void Migrate_ProfileStillInTheOldFormat_Throws()
    {
        // This migration expects M0003 to have turned the lists into arrays
        Should.Throw<InvalidOperationException>(() => _migration.Migrate(new JsonObject(), OldProfileJson.Parse(OldProfileJson.Version0Profile)));
    }

    private static (JsonObject Configuration, JsonObject Profile) Version3Profile()
    {
        JsonObject configuration = OldProfileJson.Parse(OldProfileJson.Version0Configuration);
        JsonObject profile = OldProfileJson.Parse(OldProfileJson.Version0Profile);
        new M0001NodeProviders().Migrate(configuration, profile);
        new M0002NodeProvidersProfileConfig().Migrate(configuration, profile);
        new M0003SystemTextJson().Migrate(configuration, profile);
        return (configuration, profile);
    }
}
