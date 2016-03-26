using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Windows;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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

        public static bool IsRunAsAdministrator()
        {
            var wi = WindowsIdentity.GetCurrent();
            var wp = new WindowsPrincipal(wi);

            return wp.IsInRole(WindowsBuiltInRole.Administrator);
        }

        public static void CopyProperties(object dest, object src)
        {
            foreach (PropertyDescriptor item in TypeDescriptor.GetProperties(src))
            {
                item.SetValue(dest, item.GetValue(src));
            }
        }

        public static List<PropertyCollection> GenerateTypeMap(object o) => GenerateTypeMap(o.GetType().GetProperties());
        public static List<PropertyCollection> GenerateTypeMap<T>() => GenerateTypeMap(typeof (T).GetProperties());

        private static List<PropertyCollection> GenerateTypeMap(IEnumerable<PropertyInfo> getProperties,
            string path = "")
        {
            var list = new List<PropertyCollection>();
            foreach (var propertyInfo in getProperties)
            {
                var parent = new PropertyCollection
                {
                    Type = propertyInfo.PropertyType.Name,
                    Display = $"{path.Replace(".", " → ")}{propertyInfo.Name}",
                    Path = $"{path}{propertyInfo.Name}"
                };

                if (propertyInfo.PropertyType.BaseType?.Name == "Enum")
                {
                    parent.EnumValues = Enum.GetNames(propertyInfo.PropertyType);
                    parent.Type = "Enum";
                }

                list.Add(parent);
                list.AddRange(GenerateTypeMap(propertyInfo.PropertyType.GetProperties(), path + $"{propertyInfo.Name}."));
            }
            return list;
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
        }
    }
}