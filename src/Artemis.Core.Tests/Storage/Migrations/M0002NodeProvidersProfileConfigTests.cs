using System.Text.Json.Nodes;
using Artemis.Core.Tests.Infrastructure.Storage;
using Artemis.Storage.Migrations.Profile;

namespace Artemis.Core.Tests.Storage.Migrations;

public class M0002NodeProvidersProfileConfigTests
{
    private readonly M0002NodeProvidersProfileConfig _migration = new();

    [Fact]
    public void Version_IsTwo()
    {
        _migration.Version.ShouldBe(2);
    }

    [Fact]
    public void Migrate_MovesActivationConditionNodesToTheGeneralNodesPlugin()
    {
        JsonObject configuration = OldProfileJson.Parse(OldProfileJson.Version0Configuration);

        _migration.Migrate(configuration, OldProfileJson.Parse(OldProfileJson.Version0Profile));

        JsonObject dataModelNode = configuration.FindNode(OldProfileJson.ActivationDataModelNodeId);
        dataModelNode["Type"]!.GetValue<string>().ShouldBe("Artemis.Plugins.Nodes.General.Nodes.DataModel.DataModelNode");
        dataModelNode["ProviderId"]!.GetValue<string>().ShouldBe(OldProfileJson.GeneralNodesProviderId);

        JsonObject exitNode = configuration.FindNode(OldProfileJson.ActivationExitNodeId);
        exitNode["Type"]!.GetValue<string>().ShouldBe("Artemis.Core.Internal.ExitNode`1");
        exitNode["ProviderId"]!.GetValue<string>().ShouldBe(OldProfileJson.GeneralNodesProviderId);
    }

    [Fact]
    public void Migrate_ChangesNothingButTheNodes()
    {
        JsonObject configuration = OldProfileJson.Parse(OldProfileJson.Version0Configuration);

        _migration.Migrate(configuration, new JsonObject());

        foreach (JsonObject node in configuration.FindNodes())
        {
            node.Remove("ProviderId");
            node["Type"] = node["Type"]!.GetValue<string>().Replace("Artemis.Plugins.Nodes.General.Nodes", "Artemis.VisualScripting.Nodes");
        }

        configuration.ShouldBeJson(OldProfileJson.Version0Configuration);
    }

    [Fact]
    public void Migrate_LeavesTheProfileAlone()
    {
        JsonObject profile = OldProfileJson.Parse(OldProfileJson.Version0Profile);

        _migration.Migrate(OldProfileJson.Parse(OldProfileJson.Version0Configuration), profile);

        profile.ShouldBeJson(OldProfileJson.Version0Profile);
    }

    [Fact]
    public void Migrate_RunningItTwice_GivesTheSameResult()
    {
        JsonObject once = OldProfileJson.Parse(OldProfileJson.Version0Configuration);
        _migration.Migrate(once, new JsonObject());
        JsonObject twice = OldProfileJson.Parse(OldProfileJson.Version0Configuration);
        _migration.Migrate(twice, new JsonObject());
        _migration.Migrate(twice, new JsonObject());

        twice.ShouldBeJson(once.ToJsonString());
    }

    [Theory]
    [InlineData("""{"Name": "No condition"}""")]
    [InlineData("""{"Name": "Empty condition", "ActivationCondition": null}""")]
    [InlineData("""{"Name": "Condition without nodes", "ActivationCondition": {"Name": "Script"}}""")]
    [InlineData("""{"Name": "Empty node list", "ActivationCondition": {"Nodes": {"$type": "List", "$values": [null]}}}""")]
    public void Migrate_ConfigurationWithoutNodes_DoesNothing(string json)
    {
        JsonObject configuration = OldProfileJson.Parse(json);

        _migration.Migrate(configuration, new JsonObject());

        configuration.ShouldBeJson(json);
    }

    [Fact]
    public void Migrate_ConfigurationAlreadyInTheNewFormat_Throws()
    {
        // Like M0001 it only understands the old {"$values": []} lists
        JsonObject configuration = OldProfileJson.Parse(OldProfileJson.MigratedConfiguration);

        Should.Throw<InvalidOperationException>(() => _migration.Migrate(configuration, new JsonObject()));
    }
}
