using System;
using System.IO;
using System.Reflection;
using Artemis.Core.Plugins.Models;

namespace Artemis.Core
{
    public static class Constants
    {
        public static readonly string ApplicationFolder = Path.GetDirectoryName(typeof(Constants).Assembly.Location);
        public static readonly string DataFolder = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\Artemis\\";
        public static readonly string ConnectionString = $"FileName={DataFolder}\\database.db";
        public static readonly PluginInfo CorePluginInfo = new PluginInfo {Guid = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff"), Name = "Artemis Core"};
    }
}