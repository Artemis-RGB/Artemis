using System;
using System.Threading.Tasks;
using Artemis.UI.Installer.Utilities;
using Microsoft.Win32;

namespace Artemis.UI.Installer.Services.Prerequisites
{
    public class DotnetPrerequisite : IPrerequisite
    {
        public string Title => ".NET 5 runtime x64";

        public string Description => "The .NET 5 runtime is required for Artemis to run, the download is about 50 MB";

        public string DownloadUrl => "https://download.visualstudio.microsoft.com/download/pr/c6a74d6b-576c-4ab0-bf55-d46d45610730/" +
                                     "f70d2252c9f452c2eb679b8041846466/windowsdesktop-runtime-5.0.1-win-x64.exe";

        public bool IsMet()
        {
            RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\dotnet\Setup\InstalledVersions\x64\sharedhost");
            object versionValue = key?.GetValue("Version");
            if (versionValue == null)
                return false;

            Version dotnetVersion = Version.Parse(versionValue.ToString());
            return dotnetVersion.Major >= 5;
        }

        public async Task Install(string file)
        {
            await ProcessUtilities.RunProcessAsync(file, "-passive");
        }
    }
}