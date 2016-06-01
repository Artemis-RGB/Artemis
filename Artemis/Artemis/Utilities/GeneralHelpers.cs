using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Xml.Serialization;
using Microsoft.Win32;
using static System.String;

namespace Artemis.Utilities
{
    public static class GeneralHelpers
    {
        public static void RunAsAdministrator()
        {
            var processInfo = new ProcessStartInfo(Assembly.GetExecutingAssembly().CodeBase)
            {
                UseShellExecute = true,
                Verb = "runas"
            };

            // The following properties run the new process as administrator

            // Start the new process
            try
            {
                Process.Start(processInfo);
            }
            catch (Exception)
            {
                // The user did not allow the application to run as administrator
                MessageBox.Show("Sorry, this application must be run as Administrator.");
            }

            // Shut down the current process
            Environment.Exit(0);
        }

        /// <summary>
        ///     Perform a deep Copy of the object.
        /// </summary>
        /// <typeparam name="T">The type of object being copied.</typeparam>
        /// <param name="source">The object instance to copy.</param>
        /// <returns>The copied object.</returns>
        public static T Clone<T>(T source)
        {
            // Don't serialize a null object, simply return the default for that object
            if (ReferenceEquals(source, null))
                return default(T);

            var serializer = new XmlSerializer(typeof(T));
            Stream stream = new MemoryStream();
            using (stream)
            {
                serializer.Serialize(stream, source);
                stream.Seek(0, SeekOrigin.Begin);
                return (T) serializer.Deserialize(stream);
            }
        }

        public static object GetPropertyValue(object o, string path)
        {
            var propertyNames = path.Split('.');
            var value = o.GetType().GetProperty(propertyNames[0]).GetValue(o, null);

            if (propertyNames.Length == 1 || value == null)
                return value;
            return GetPropertyValue(value, path.Replace(propertyNames[0] + ".", ""));
        }

        public static List<PropertyCollection> GenerateTypeMap(object o) => GenerateTypeMap(o.GetType().GetProperties());

        private static List<PropertyCollection> GenerateTypeMap(IEnumerable<PropertyInfo> getProperties,
            string path = "")
        {
            var list = new List<PropertyCollection>();
            foreach (var propertyInfo in getProperties)
            {
                var friendlyName = Empty;
                if (propertyInfo.PropertyType.Name == "Int32")
                    friendlyName = "(Number)";
                else if (propertyInfo.PropertyType.Name == "String")
                    friendlyName = "(Text)";
                else if (propertyInfo.PropertyType.Name == "Boolean")
                    friendlyName = "(Yes/no)";
                if (propertyInfo.PropertyType.BaseType?.Name == "Enum")
                    friendlyName = "(Choice)";

                var parent = new PropertyCollection
                {
                    Type = propertyInfo.PropertyType.Name,
                    DisplayType = friendlyName,
                    Display = $"{path.Replace(".", " → ")}{propertyInfo.Name}",
                    Path = $"{path}{propertyInfo.Name}"
                };

                if (propertyInfo.PropertyType.BaseType?.Name == "Enum")
                {
                    parent.EnumValues = Enum.GetNames(propertyInfo.PropertyType);
                    parent.Type = "Enum";
                }

                if (friendlyName != Empty)
                    list.Add(parent);

                if (propertyInfo.PropertyType.Name != "String")
                    list.AddRange(GenerateTypeMap(propertyInfo.PropertyType.GetProperties(),
                        path + $"{propertyInfo.Name}."));
            }
            return list;
        }

        public static string FindSteamGame(string relativePath)
        {
            var key = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam");
            if (key == null)
                return null;
            var path = key.GetValue("SteamPath").ToString();
            var libFile = path + @"\steamapps\libraryfolders.vdf";
            if (!File.Exists(libFile))
                return null;

            // Try the main SteamApps folder
            if (File.Exists(path + "\\SteamApps\\common" + relativePath))
                return Path.GetDirectoryName(path + "\\SteamApps\\common" + relativePath);

            // If not found in the main folder, try all the libraries found in the vdf file.
            var content = File.ReadAllText(libFile);
            var matches = Regex.Matches(content, "\"\\d\"\\t\\t\"(.*)\"");
            foreach (Match match in matches)
            {
                var library = match.Groups[1].Value;
                library = library.Replace("\\\\", "\\") + "\\SteamApps\\common";
                if (File.Exists(library + relativePath))
                    return Path.GetDirectoryName(library + relativePath);
            }
            return null;
        }

        public struct PropertyCollection
        {
            public string Display { get; set; }
            public string Path { get; set; }
            public string Type { get; set; }

            /// <summary>
            ///     Only used if Type is an enumerable
            /// </summary>
            public string[] EnumValues { get; set; }

            public List<PropertyCollection> Children { get; set; }
            public string DisplayType { get; set; }
        }
    }
}