using System.Text.Json.Nodes;
using Artemis.Core.Tests.Infrastructure.Storage;
using Artemis.Storage.Migrations.Profile;

namespace Artemis.Core.Tests.Storage.Migrations;

public class M0001NodeProvidersTests
{
    private readonly M0001NodeProviders _migration = new();

    [Fact]
    public void Version_IsOne()
    {
        _migration.Version.ShouldBe(1);
    }

    [Theory]
    [InlineData(OldProfileJson.FolderConditionNodeId, "Artemis.Plugins.Nodes.General.Nodes.DataModel.DataModelNode")]
    [InlineData(OldProfileJson.FolderEffectNodeId, "Artemis.Plugins.Nodes.General.Nodes.Mathematics.MathExpressionNode")]
    [InlineData(OldProfileJson.LayerConditionNodeId, "Artemis.Plugins.Nodes.General.Nodes.Static.StaticBooleanValueNode")]
    [InlineData(OldProfileJson.LayerTransformNodeId, "Artemis.Plugins.Nodes.General.Nodes.Static.StaticNumericValueNode")]
    [InlineData(OldProfileJson.LayerBrushNodeId, "Artemis.Plugins.Nodes.General.Nodes.Color.ColorGradientNode")]
    public void Migrate_MovesNodesEverywhereInTheProfileToTheGeneralNodesPlugin(string nodeId, string expectedType)
    {
        JsonObject profile = OldProfileJson.Parse(OldProfileJson.Version0Profile);

        _migration.Migrate(OldProfileJson.Parse(OldProfileJson.Version0Configuration), profile);

        JsonObject node = profile.FindNode(nodeId);
        node["Type"]!.GetValue<string>().ShouldBe(expectedType);
        node["ProviderId"]!.GetValue<string>().ShouldBe(OldProfileJson.GeneralNodesProviderId);
    }

    [Fact]
    public void Migrate_ChangesNothingButTheNodes()
    {
        JsonObject profile = OldProfileJson.Parse(OldProfileJson.Version0Profile);

        _migration.Migrate(OldProfileJson.Parse(OldProfileJson.Version0Configuration), profile);

        // Undo the two node changes, what remains should be the original
        foreach (JsonObject node in profile.FindNodes())
        {
            node.Remove("ProviderId");
            node["Type"] = node["Type"]!.GetValue<string>().Replace("Artemis.Plugins.Nodes.General.Nodes", "Artemis.VisualScripting.Nodes");
        }

        profile.ShouldBeJson(OldProfileJson.Version0Profile);
    }

    [Fact]
    public void Migrate_LeavesTheConfigurationAlone()
    {
        // The activation condition was forgotten in this migration, M0002 migrates it
        JsonObject configuration = OldProfileJson.Parse(OldProfileJson.Version0Configuration);

        _migration.Migrate(configuration, OldProfileJson.Parse(OldProfileJson.Version0Profile));

        configuration.ShouldBeJson(OldProfileJson.Version0Configuration);
    }

    [Fact]
    public void Migrate_GivesNodesOfOtherNamespacesTheProviderButKeepsTheirType()
    {
        JsonObject profile = OldProfileJson.Parse(OldProfileJson.Version0Profile);
        JsonObject node = profile.FindNode(OldProfileJson.LayerTransformNodeId);
        node["Type"] = "Artemis.Core.Internal.DataBindingExitNode`1";

        _migration.Migrate(new JsonObject(), profile);

        // At version 0 all nodes came from Artemis itself, so every node is given the general nodes provider
        node["Type"]!.GetValue<string>().ShouldBe("Artemis.Core.Internal.DataBindingExitNode`1");
        node["ProviderId"]!.GetValue<string>().ShouldBe(OldProfileJson.GeneralNodesProviderId);
    }

    [Fact]
    public void Migrate_RunningItTwice_GivesTheSameResult()
    {
        JsonObject once = OldProfileJson.Parse(OldProfileJson.Version0Profile);
        _migration.Migrate(new JsonObject(), once);
        JsonObject twice = OldProfileJson.Parse(OldProfileJson.Version0Profile);
        _migration.Migrate(new JsonObject(), twice);
        _migration.Migrate(new JsonObject(), twice);

        twice.ShouldBeJson(once.ToJsonString());
    }

    [Fact]
    public void Migrate_ProfileWithoutFoldersOrLayers_DoesNothing()
    {
        JsonObject profile = new() {["Name"] = "Empty"};

        _migration.Migrate(new JsonObject(), profile);

        profile.ShouldBeJson("""{"Name": "Empty"}""");
    }

    [Fact]
    public void Migrate_LayerWithoutPropertiesOrConditions_DoesNothing()
    {
        const string json = """
                            {
                              "Folders": {"$type": "List", "$values": [null, {"Name": "Folder"}]},
                              "Layers": {"$type": "List", "$values": [null, {"Name": "Layer", "DisplayCondition": {"$type": "AlwaysOn"}, "LayerBrush": null}]}
                            }
                            """;
        JsonObject profile = OldProfileJson.Parse(json);

        _migration.Migrate(new JsonObject(), profile);

        profile.ShouldBeJson(json);
    }

    [Fact]
    public void Migrate_NodeWithoutType_LeavesTheTypeOut()
    {
        JsonObject profile = OldProfileJson.Parse(OldProfileJson.Version0Profile);
        JsonObject node = profile.FindNode(OldProfileJson.LayerTransformNodeId);
        node.Remove("Type");

        _migration.Migrate(new JsonObject(), profile);

        node.ContainsKey("Type").ShouldBeFalse();
        node["ProviderId"]!.GetValue<string>().ShouldBe(OldProfileJson.GeneralNodesProviderId);
    }

    [Fact]
    public void Migrate_ProfileAlreadyInTheNewFormat_Throws()
    {
        // Lists in the new format are plain arrays, this migration only understands the old {"$values": []} lists.
        // MigrateProfile only runs it on profiles below version 1 so this shouldn't happen.
        JsonObject profile = OldProfileJson.Parse(OldProfileJson.MigratedProfile);

        Should.Throw<InvalidOperationException>(() => _migration.Migrate(new JsonObject(), profile));
    }
}
