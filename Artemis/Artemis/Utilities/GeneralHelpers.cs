﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.Win32;
using Newtonsoft.Json;
using static System.String;

namespace Artemis.Utilities
{
    public static class GeneralHelpers
    {
        public static string DataFolder = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\Artemis\\";

        public static void SetupDataFolder()
        {
            if (!Directory.Exists(DataFolder))
                Directory.CreateDirectory(DataFolder);

            var oldDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Artemis\\";

            try
            {
                if (!Directory.Exists(oldDataFolder))
                    return;

                // Migrate the old Artemis folder if it's present and access to it can be gained
                MoveDirectory(oldDataFolder, DataFolder);
            }
            catch (Exception)
            {
                // ignored, could be blocked, in that case tough luck!
            }
        }

        /// <summary>
        ///     Perform a deep Copy of the object, using Json as a serialisation method.
        /// </summary>
        /// <typeparam name="T">The type of object being copied.</typeparam>
        /// <param name="source">The object instance to copy.</param>
        /// <returns>The copied object.</returns>
        public static T Clone<T>(T source)
        {
            // Don't serialize a null object, simply return the default for that object
            if (ReferenceEquals(source, null))
                return default(T);

            return (T) JsonConvert.DeserializeObject(JsonConvert.SerializeObject(source), source.GetType());
        }

        public static object GetPropertyValue(object o, string path)
        {
            var propertyNames = path.Split('.');
            var prop = o.GetType().GetProperty(propertyNames[0]);
            if (prop == null)
                return null;
            var value = prop.GetValue(o, null);

            if (propertyNames.Length == 1 || value == null)
                return value;
            return GetPropertyValue(value, path.Replace(propertyNames[0] + ".", ""));
        }

        public static List<PropertyCollection> GenerateTypeMap(object o)
        {
            return GenerateTypeMap(o.GetType().GetProperties());
        }

        private static List<PropertyCollection> GenerateTypeMap(IEnumerable<PropertyInfo> getProperties, string path = "", bool inList = false)
        {
            var list = new List<PropertyCollection>();
            foreach (var propInfo in getProperties)
            {
                var friendlyName = Empty;
                if (propInfo.PropertyType.Name == "Int32")
                    friendlyName = "(Number)";
                else if (propInfo.PropertyType.Name == "Single")
                    friendlyName = "(Decimal)";
                else if (propInfo.PropertyType.Name == "String")
                    friendlyName = "(Text)";
                else if (propInfo.PropertyType.Name == "Boolean")
                    friendlyName = "(Yes/no)";
                if (propInfo.PropertyType.BaseType?.Name == "Enum")
                    friendlyName = "(Choice)";

                // At this point the loop is in the item type contained in the list
                PropertyCollection parent;
                if (path.Contains("Item") && inList)
                    parent = new PropertyCollection
                    {
                        Type = propInfo.PropertyType.Name,
                        DisplayType = friendlyName,
                        Display = $"{path.Replace("Item.", "").Replace(".", " → ")}{propInfo.Name}",
                        Path = $"{path.Replace("Item.", "")}{propInfo.Name}"
                    };
                else
                    parent = new PropertyCollection
                    {
                        Type = propInfo.PropertyType.Name,
                        DisplayType = friendlyName,
                        Display = $"{path.Replace(".", " → ")}{propInfo.Name}",
                        Path = $"{path}{propInfo.Name}"
                    };

                if (propInfo.PropertyType.BaseType?.Name == "Enum")
                {
                    parent.EnumValues = Enum.GetNames(propInfo.PropertyType);
                    parent.Type = "Enum";
                }

                if (friendlyName != Empty)
                    list.Add(parent);

                // Don't go into Strings, DateTimes or anything with JsonIgnore on it
                if (propInfo.PropertyType.Name != "String" &&
                    propInfo.PropertyType.Name != "DateTime" &&
                    propInfo.CustomAttributes.All(a => a.AttributeType != typeof(JsonIgnoreAttribute)))
                {
                    var newPath = $"{path}{propInfo.Name}.";
                    var toInList = propInfo.PropertyType.Name == "List`1";
                    if (toInList)
                    {
                        inList = true;
                        newPath = $"({path}{propInfo.Name}).";
                    }
                    list.AddRange(GenerateTypeMap(propInfo.PropertyType.GetProperties(), newPath, inList));
                }
            }
            return list;
        }

        public static string FindSteamGame(string relativePath)
        {
            var path = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam")?.GetValue("SteamPath")?.ToString();
            if (path == null)
                return null;
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

        public static void ExecuteSta(Action action)
        {
            var thread = new Thread(action.Invoke);
            thread.SetApartmentState(ApartmentState.STA); //Set the thread to STA
            thread.Start();
            thread.Join();
        }

        public static T ParseEnum<T>(string value, bool ignoreCase = true, bool stripWhitespaces = true)
        {
            if (stripWhitespaces)
                value = value.Replace(" ", "");
            return (T) Enum.Parse(typeof(T), value, true);
        }

        public static void MoveDirectory(string source, string target)
        {
            var sourcePath = source.TrimEnd('\\', ' ');
            var targetPath = target.TrimEnd('\\', ' ');
            var files = Directory.EnumerateFiles(sourcePath, "*", SearchOption.AllDirectories).GroupBy(Path.GetDirectoryName);
            foreach (var folder in files)
            {
                var targetFolder = folder.Key.Replace(sourcePath, targetPath);
                Directory.CreateDirectory(targetFolder);
                foreach (var file in folder)
                {
                    var fileName = Path.GetFileName(file);
                    if (fileName == null) continue;
                    var targetFile = Path.Combine(targetFolder, fileName);
                    if (File.Exists(targetFile)) File.Delete(targetFile);
                    File.Move(file, targetFile);
                }
            }
            Directory.Delete(source, true);
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