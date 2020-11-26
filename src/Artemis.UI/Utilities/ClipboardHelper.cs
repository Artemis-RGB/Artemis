using System.Windows;
using Newtonsoft.Json;

namespace Artemis.UI.Utilities
{
    public static class JsonClipboard
    {
        private static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings {TypeNameHandling = TypeNameHandling.All};

        public static void SetObject(object clipboardObject)
        {
            string json = JsonConvert.SerializeObject(clipboardObject, JsonSettings);
            Clipboard.SetData("Artemis", json);
        }

        public static object GetData()
        {
            string json = Clipboard.GetData("Artemis")?.ToString();
            if (json != null)
                return JsonConvert.DeserializeObject(json, JsonSettings);
            return null;
        }

        public static T GetData<T>()
        {
            object data = GetData();
            return data is T castData ? castData : default;
        }

        public static bool ContainsArtemisData()
        {
            return Clipboard.ContainsData("Artemis");
        }
    }
}