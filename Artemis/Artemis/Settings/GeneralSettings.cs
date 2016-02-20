using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using Artemis.Utilities;

namespace Artemis.Settings
{
    public class GeneralSettings
    {
        public int GamestatePort
        {
            get { return General.Default.GamestatePort; }
            set
            {
                if (General.Default.GamestatePort == value) return;
                General.Default.GamestatePort = value;
                ApplyGamestatePort();
                General.Default.Save();
            }
        }

        public bool EnablePointersUpdate
        {
            get { return General.Default.EnablePointersUpdate; }
            set
            {
                if (General.Default.EnablePointersUpdate == value) return;
                General.Default.EnablePointersUpdate = value;
                General.Default.Save();
            }
        }

        public bool Autorun
        {
            get { return General.Default.Autorun; }
            set
            {
                if (General.Default.Autorun == value) return;
                General.Default.Autorun = value;
                ApplyAutorun();
                General.Default.Save();
            }
        }

        private void ApplyGamestatePort()
        {
            // TODO: Restart Gamestate server
        }

        private void ApplyAutorun()
        {
            var startupFolder = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            if (Autorun)
            {
                var link = (IShellLink) new ShellLink();
                link.SetPath(Assembly.GetExecutingAssembly().Location);
                var file = (IPersistFile) link;

                file.Save(startupFolder + @"\Artemis.lnk", false);
            }
            else if (File.Exists(startupFolder + @"\Artemis.lnk"))
                File.Delete(startupFolder + @"\Artemis.lnk");
        }
    }
}