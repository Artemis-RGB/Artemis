using System;
using Artemis.Core.Plugins.Models;

namespace Artemis.Core
{
    public static class Constants
    {
        public static readonly string DataFolder = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\Artemis\\";
        public static readonly string ConnectionString = $"FileName={DataFolder}\\database.db;Mode=Exclusive";
        public static readonly PluginInfo CorePluginInfo = new PluginInfo {Guid = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff"), Name = "Artemis Core"};
    }
}