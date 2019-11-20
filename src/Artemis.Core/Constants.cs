using System;

namespace Artemis.Core
{
    public static class Constants
    {
        public static readonly string DataFolder = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\Artemis\\";
        public static readonly string ConnectionString = $"FileName={DataFolder}\\database.db;Mode=Exclusive";
    }
}