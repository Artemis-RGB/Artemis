using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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

        public static List<PropertyCollection> GetPropertyMap(object o)
        {
            var res = new List<PropertyCollection>();
            // No point reinventing the wheel, just serialize it to JSON and parse that
            var json = JObject.FromObject(o, JsonSerializer.CreateDefault());
            res.AddRange(JObjectToPropertyCollection(json));

            return res;
        }

        private static List<PropertyCollection> JObjectToPropertyCollection(JObject json)
        {
            var res = new List<PropertyCollection>();
            foreach (var property in json.Properties())
            {
                var parent = new PropertyCollection {Name = property.Name};
                foreach (var child in property.Children<JObject>())
                    parent.Children = JObjectToPropertyCollection(child);

                res.Add(parent);
            }
            return res;
        }

        public struct PropertyCollection
        {
            public string Name { get; set; }
            public List<PropertyCollection> Children { get; set; }
        }
    }
}