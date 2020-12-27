using System.Threading.Tasks;
using Artemis.UI.Installer.Utilities;
using Microsoft.Win32;

namespace Artemis.UI.Installer.Services.Prerequisites
{
    public class RedistPrerequisite : IPrerequisite
    {
        public string Title => "Visual C++ Redistributable for VS 2015, 2017 and 2019 x64";
        public string Description => "The C++ Redistributable is required for many device SDKs, the download is about 15 MB";
        public string DownloadUrl => "https://aka.ms/vs/16/release/vc_redist.x64.exe";

        public bool IsMet()
        {
            RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Microsoft\VisualStudio\14.0\VC\Runtimes\x64", false);
            object majorValue = key?.GetValue("Major");
            if (majorValue == null)
                return false;

            return int.Parse(majorValue.ToString()) >= 14;
        }

        public async Task Install(string file)
        {
            await ProcessUtilities.RunProcessAsync(file, "-passive");
        }
    }
}