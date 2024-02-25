using Newtonsoft.Json.Linq;

namespace Artemis.Storage.Migrations.Profile;

/// <summary>
/// Migrates nodes to be provider-based on profile configurations as well..
/// This requires giving them a ProviderId and updating the their namespaces to match the namespace of the new plugin. 
/// </summary>
internal class M0002NodeProvidersProfileConfig : IProfileMigration
{
    /// <inheritdoc />
    public int Version => 2;

    /// <inheritdoc />
    public void Migrate(JObject configurationJson, JObject profileJson)
    {
        MigrateNodeScript(configurationJson["ActivationCondition"]);
    }
    
    private void MigrateNodeScript(JToken? nodeScript)
    {
        if (nodeScript == null || !nodeScript.HasValues)
            return;

        JArray? nodes = (JArray?) nodeScript["Nodes"]?["$values"];
        if (nodes == null)
            return;

        foreach (JToken node in nodes)
        {
            node["Type"] = node["Type"]?.Value<string>()?.Replace("Artemis.VisualScripting.Nodes", "Artemis.Plugins.Nodes.General.Nodes");
            node["ProviderId"] = "Artemis.Plugins.Nodes.General.GeneralNodesProvider-d9e1ee78";
        }
    }
}