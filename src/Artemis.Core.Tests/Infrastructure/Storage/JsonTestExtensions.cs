using System.Text.Json;
using System.Text.Json.Nodes;

namespace Artemis.Core.Tests.Infrastructure.Storage;

public static class JsonTestExtensions
{
    private static readonly JsonSerializerOptions Indented = new() {WriteIndented = true};

    /// <summary>
    ///     Checks that the JSON holds the same values as the expected JSON, the order of properties doesn't matter.
    ///     Shows both when they differ.
    /// </summary>
    public static void ShouldBeJson(this JsonNode? actual, string expected)
    {
        JsonNode? expectedNode = JsonNode.Parse(expected);
        if (JsonNode.DeepEquals(actual, expectedNode))
            return;

        throw new ShouldAssertException($"""
                                         The JSON is different from what was expected.
                                         Expected:
                                         {expectedNode?.ToJsonString(Indented)}
                                         Actual:
                                         {actual?.ToJsonString(Indented)}
                                         """);
    }

    /// <summary>
    ///     Finds every node of every node script in the JSON, in old and new profiles alike
    /// </summary>
    public static List<JsonObject> FindNodes(this JsonNode root)
    {
        List<JsonObject> nodes = [];
        Collect(root);
        return nodes;

        void Collect(JsonNode? node)
        {
            switch (node)
            {
                case JsonObject jsonObject:
                    if (jsonObject.ContainsKey("IsExitNode"))
                        nodes.Add(jsonObject);
                    foreach ((string _, JsonNode? value) in jsonObject)
                        Collect(value);
                    break;
                case JsonArray jsonArray:
                    foreach (JsonNode? value in jsonArray)
                        Collect(value);
                    break;
            }
        }
    }

    /// <summary>
    ///     Finds the node with the given ID anywhere in the JSON
    /// </summary>
    public static JsonObject FindNode(this JsonNode root, string id)
    {
        return root.FindNodes().Single(n => n["Id"]?.GetValue<string>() == id);
    }

    /// <summary>
    ///     Gets the text of every <c>$type</c> left anywhere in the JSON
    /// </summary>
    public static List<string> FindTypeNames(this JsonNode root)
    {
        List<string> types = [];
        Collect(root);
        return types;

        void Collect(JsonNode? node)
        {
            switch (node)
            {
                case JsonObject jsonObject:
                    foreach ((string key, JsonNode? value) in jsonObject)
                    {
                        if (key == "$type" && value != null)
                            types.Add(value.ToString());
                        Collect(value);
                    }

                    break;
                case JsonArray jsonArray:
                    foreach (JsonNode? value in jsonArray)
                        Collect(value);
                    break;
            }
        }
    }
}
