using System.Linq;
using System.Text.Json.Nodes;

namespace Artemis.Storage.Migrations.Profile;

/// <summary>
/// Migrates profiles to be deserializable by System.Text.Json, removing type information from the JSON arrays and most objects.
/// </summary>
internal class M0003SystemTextJson : IProfileMigration
{
    /// <inheritdoc />
    public int Version => 3;

    /// <inheritdoc />
    public void Migrate(JsonObject configurationJson, JsonObject profileJson)
    {
        ConvertToSystemTextJson(configurationJson);
        ConvertToSystemTextJson(profileJson);
    }

    private void ConvertToSystemTextJson(JsonObject jsonObject)
    {
        FilterType(jsonObject);
        
        // Recursively convert all JSON arrays from {$type: "xyz", $values: []} to []
        foreach ((string? key, JsonNode? value) in jsonObject.ToDictionary())
        {
            if (value is not JsonObject obj)
                continue;

            // if there is a $type and a $values, replace the entire node with $values regardless of the value of $type
            if (obj["$type"] != null && obj["$values"] != null)
            {
                JsonArray? values = obj["$values"]?.AsArray();
                if (values != null)
                {
                    obj.Remove("$values");
                    jsonObject[key] = values;
                    foreach (JsonNode? jsonNode in values.ToList())
                    {
                        if (jsonNode is JsonObject childObject)
                            ConvertToSystemTextJson(childObject);
                    }
                }

                obj.Remove("$type");
            }
            else
            {
                ConvertToSystemTextJson(obj);
            }
        }
    }

    private void FilterType(JsonObject jsonObject)
    {
        // Replace or remove $type depending on whether there's a matching JsonDerivedType
        // This could be done with reflection but that would mean this migration automatically gains new behaviour over time.
        JsonNode? type = jsonObject["$type"];
        if (type != null)
        {
            // Adaption Hints
            if (type.GetValue<string>() == "Artemis.Storage.Entities.Profile.AdaptionHints.CategoryAdaptionHintEntity, Artemis.Storage")
                jsonObject["$type"] = "Category";
            else if (type.GetValue<string>() == "Artemis.Storage.Entities.Profile.AdaptionHints.DeviceAdaptionHintEntity, Artemis.Storage")
                jsonObject["$type"] = "Device";
            else if (type.GetValue<string>() == "Artemis.Storage.Entities.Profile.AdaptionHints.KeyboardSectionAdaptionHintEntity, Artemis.Storage")
                jsonObject["$type"] = "KeyboardSection";
            else if (type.GetValue<string>() == "Artemis.Storage.Entities.Profile.AdaptionHints.SingleLedAdaptionHintEntity, Artemis.Storage")
                jsonObject["$type"] = "SingleLed";
            // Conditions
            if (type.GetValue<string>() == "Artemis.Storage.Entities.Profile.Conditions.AlwaysOnConditionEntity, Artemis.Storage")
                jsonObject["$type"] = "AlwaysOn";
            else if (type.GetValue<string>() == "Artemis.Storage.Entities.Profile.Conditions.EventConditionEntity, Artemis.Storage")
                jsonObject["$type"] = "Event";
            else if (type.GetValue<string>() == "Artemis.Storage.Entities.Profile.Conditions.PlayOnceConditionEntity, Artemis.Storage")
                jsonObject["$type"] = "PlayOnce";
            else if (type.GetValue<string>() == "Artemis.Storage.Entities.Profile.Conditions.StaticConditionEntity, Artemis.Storage")
                jsonObject["$type"] = "Static";
            else
                jsonObject.Remove("$type");
        }
    }
}