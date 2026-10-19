using System.Text.Json;
using System.Text.Json.Nodes;
using Artemis.Core.Tests.Infrastructure.Storage;
using Artemis.Storage.Entities.Profile;
using Artemis.Storage.Entities.Profile.AdaptionHints;
using Artemis.Storage.Entities.Profile.Conditions;
using Artemis.Storage.Migrations.Profile;

namespace Artemis.Core.Tests.Storage.Migrations;

public class M0003SystemTextJsonTests
{
    private readonly M0003SystemTextJson _migration = new();

    [Fact]
    public void Version_IsThree()
    {
        _migration.Version.ShouldBe(3);
    }

    [Fact]
    public void Migrate_ConvertsEverythingButNodeStorage()
    {
        (JsonObject configuration, JsonObject profile) = Version2Profile();

        _migration.Migrate(configuration, profile);

        // Node storage is a JSON string inside the JSON and is left for M0004, copy what M0004 makes of it
        CopyNodeStorage(OldProfileJson.MigratedConfiguration, configuration);
        CopyNodeStorage(OldProfileJson.MigratedProfile, profile);
        // Only MigrateProfile sets the version
        configuration["Version"] = 4;
        configuration.ShouldBeJson(OldProfileJson.MigratedConfiguration);
        profile.ShouldBeJson(OldProfileJson.MigratedProfile);
    }

    [Fact]
    public void Migrate_LeavesNodeStorageAlone()
    {
        (JsonObject configuration, JsonObject profile) = Version2Profile();

        _migration.Migrate(configuration, profile);

        configuration.FindNode(OldProfileJson.ActivationDataModelNodeId)["Storage"]!.GetValue<string>()
            .ShouldStartWith("{\"$type\":\"Artemis.Storage.Entities.Profile.DataModelPathEntity, Artemis.Storage\"");
        profile.FindNode(OldProfileJson.LayerBrushNodeId)["Storage"]!.GetValue<string>()
            .ShouldStartWith("{\"$type\":\"Artemis.Core.ColorGradient, Artemis.Core\",\"$values\":[");
    }

    [Fact]
    public void Migrate_ProfileCanBeReadAfterwards()
    {
        (JsonObject configuration, JsonObject profile) = Version2Profile();

        _migration.Migrate(configuration, profile);

        ProfileEntity entity = profile.Deserialize<ProfileEntity>(Constants.JsonConvertSettings)!;
        entity.Folders.ShouldHaveSingleItem().DisplayCondition.ShouldBeOfType<StaticConditionEntity>();
        LayerEntity layer = entity.Layers.ShouldHaveSingleItem();
        layer.DisplayCondition.ShouldBeOfType<EventConditionEntity>();
        layer.AdaptionHints.Count.ShouldBe(2);
        layer.AdaptionHints[0].ShouldBeOfType<CategoryAdaptionHintEntity>();
        layer.AdaptionHints[1].ShouldBeOfType<KeyboardSectionAdaptionHintEntity>();
        layer.Leds.Count.ShouldBe(2);

        ProfileConfigurationEntity configurationEntity = configuration.Deserialize<ProfileConfigurationEntity>(Constants.JsonConvertSettings)!;
        configurationEntity.ActivationCondition!.Nodes.Count.ShouldBe(2);
        configurationEntity.EnableHotkey!.Key.ShouldBe(44);
    }

    [Theory]
    [InlineData("Artemis.Storage.Entities.Profile.AdaptionHints.CategoryAdaptionHintEntity, Artemis.Storage", "Category")]
    [InlineData("Artemis.Storage.Entities.Profile.AdaptionHints.DeviceAdaptionHintEntity, Artemis.Storage", "Device")]
    [InlineData("Artemis.Storage.Entities.Profile.AdaptionHints.KeyboardSectionAdaptionHintEntity, Artemis.Storage", "KeyboardSection")]
    [InlineData("Artemis.Storage.Entities.Profile.AdaptionHints.SingleLedAdaptionHintEntity, Artemis.Storage", "SingleLed")]
    [InlineData("Artemis.Storage.Entities.Profile.Conditions.AlwaysOnConditionEntity, Artemis.Storage", "AlwaysOn")]
    [InlineData("Artemis.Storage.Entities.Profile.Conditions.EventConditionEntity, Artemis.Storage", "Event")]
    [InlineData("Artemis.Storage.Entities.Profile.Conditions.PlayOnceConditionEntity, Artemis.Storage", "PlayOnce")]
    [InlineData("Artemis.Storage.Entities.Profile.Conditions.StaticConditionEntity, Artemis.Storage", "Static")]
    public void Migrate_GivesAdaptionHintsAndConditionsTheirShortTypeName(string oldType, string newType)
    {
        JsonObject profile = new() {["Element"] = new JsonObject {["$type"] = oldType, ["Value"] = 1}};

        _migration.Migrate(new JsonObject(), profile);

        profile.ShouldBeJson($$$"""{"Element": {"$type": "{{{newType}}}", "Value": 1}}""");
    }

    [Theory]
    [InlineData("Artemis.Storage.Entities.Profile.LayerEntity, Artemis.Storage")]
    [InlineData("Artemis.Storage.Entities.Profile.Conditions.SomeNewConditionEntity, Artemis.Storage")]
    [InlineData("Static")]
    public void Migrate_RemovesAnyOtherTypeName(string type)
    {
        JsonObject profile = new() {["$type"] = type, ["Element"] = new JsonObject {["$type"] = type, ["Value"] = 1}};

        _migration.Migrate(new JsonObject(), profile);

        profile.ShouldBeJson("""{"Element": {"Value": 1}}""");
    }

    [Fact]
    public void Migrate_ReplacesListsByTheirValues_WhateverTheirType()
    {
        JsonObject profile = OldProfileJson.Parse("""
                                                  {
                                                    "Numbers": {"$type": "System.Int32[], System.Private.CoreLib", "$values": [1, 2]},
                                                    "Elements": {"$type": "Something else entirely", "$values": [{"$type": "Element", "Children": {"$type": "List", "$values": []}}]}
                                                  }
                                                  """);

        _migration.Migrate(new JsonObject(), profile);

        profile.ShouldBeJson("""{"Numbers": [1, 2], "Elements": [{"Children": []}]}""");
    }

    [Fact]
    public void Migrate_ListDirectlyInsideAList_KeepsItsValuesWrapper()
    {
        JsonObject profile = OldProfileJson.Parse("""
                                                  {"Rows": {"$type": "List", "$values": [{"$type": "List", "$values": [1, 2]}]}}
                                                  """);

        _migration.Migrate(new JsonObject(), profile);

        // Pinned as-is: items of a list are only searched for lists in their properties, a list that is itself an item
        // keeps its "$values" wrapper. No profile entity has a list of lists, so old profiles aren't affected.
        profile.ShouldBeJson("""{"Rows": [{"$values": [1, 2]}]}""");
    }

    [Fact]
    public void Migrate_LeavesObjectsInPlainArraysAlone()
    {
        // JSON.NET wrapped every list when it wrote type names, so old profiles have no plain arrays
        const string json = """{"Elements": [{"$type": "Element", "Children": {"$type": "List", "$values": []}}]}""";
        JsonObject profile = OldProfileJson.Parse(json);

        _migration.Migrate(new JsonObject(), profile);

        profile.ShouldBeJson(json);
    }

    [Fact]
    public void Migrate_ValuesThatAreNull_AreTreatedAsAnObject()
    {
        JsonObject profile = OldProfileJson.Parse("""{"Elements": {"$type": "List", "$values": null}}""");

        _migration.Migrate(new JsonObject(), profile);

        profile.ShouldBeJson("""{"Elements": {"$values": null}}""");
    }

    [Fact]
    public void Migrate_ValuesThatAreNotAList_Throws()
    {
        JsonObject profile = OldProfileJson.Parse("""{"Elements": {"$type": "List", "$values": 5}}""");

        Should.Throw<InvalidOperationException>(() => _migration.Migrate(new JsonObject(), profile));
    }

    [Fact]
    public void Migrate_RunningItOnAMigratedProfile_ChangesNothing()
    {
        JsonObject configuration = OldProfileJson.Parse(OldProfileJson.MigratedConfiguration);
        JsonObject profile = OldProfileJson.Parse(OldProfileJson.MigratedProfile);

        _migration.Migrate(configuration, profile);

        // The short type names would be removed (see Migrate_RemovesAnyOtherTypeName), but in a migrated profile they
        // are all inside plain arrays, which this migration doesn't look into
        configuration.ShouldBeJson(OldProfileJson.MigratedConfiguration);
        profile.ShouldBeJson(OldProfileJson.MigratedProfile);
        profile.FindTypeNames().ShouldBe(["Static", "Category", "KeyboardSection", "Event"]);
    }

    private (JsonObject Configuration, JsonObject Profile) Version2Profile()
    {
        JsonObject configuration = OldProfileJson.Parse(OldProfileJson.Version0Configuration);
        JsonObject profile = OldProfileJson.Parse(OldProfileJson.Version0Profile);
        new M0001NodeProviders().Migrate(configuration, profile);
        new M0002NodeProvidersProfileConfig().Migrate(configuration, profile);
        return (configuration, profile);
    }

    private static void CopyNodeStorage(string fromJson, JsonNode to)
    {
        JsonNode from = JsonNode.Parse(fromJson)!;
        foreach (JsonObject node in to.FindNodes())
            node["Storage"] = from.FindNode(node["Id"]!.GetValue<string>())["Storage"]!.DeepClone();
    }
}
