using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Xml.Linq;
using Artemis.Core;
using Artemis.UI.Properties;

namespace Artemis.UI.Utilities
{
    public static class SettingsUtilities
    {
        public static bool IsAutoRunTaskCreated()
        {
            Process schtasks = new()
            {
                StartInfo =
                {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = true,
                    FileName = Path.Combine(Environment.SystemDirectory, "schtasks.exe"),
                    Arguments = "/TN \"Artemis 2 autorun\""
                }
            };

            schtasks.Start();
            schtasks.WaitForExit();
            return schtasks.ExitCode == 0;
        }

        public static void CreateAutoRunTask(TimeSpan autoRunDelay)
        {
            XDocument document = XDocument.Parse(Resources.artemis_autorun);
            XElement task = document.Descendants().First();

            task.Descendants().First(d => d.Name.LocalName == "RegistrationInfo").Descendants().First(d => d.Name.LocalName == "Date")
                .SetValue(DateTime.Now);
            task.Descendants().First(d => d.Name.LocalName == "RegistrationInfo").Descendants().First(d => d.Name.LocalName == "Author")
                .SetValue(WindowsIdentity.GetCurrent().Name);

            task.Descendants().First(d => d.Name.LocalName == "Triggers").Descendants().First(d => d.Name.LocalName == "LogonTrigger").Descendants().First(d => d.Name.LocalName == "Delay")
                .SetValue(autoRunDelay);

            task.Descendants().First(d => d.Name.LocalName == "Principals").Descendants().First(d => d.Name.LocalName == "Principal").Descendants().First(d => d.Name.LocalName == "UserId")
                .SetValue(WindowsIdentity.GetCurrent().User.Value);

            task.Descendants().First(d => d.Name.LocalName == "Actions").Descendants().First(d => d.Name.LocalName == "Exec").Descendants().First(d => d.Name.LocalName == "WorkingDirectory")
                .SetValue(Constants.ApplicationFolder);
            task.Descendants().First(d => d.Name.LocalName == "Actions").Descendants().First(d => d.Name.LocalName == "Exec").Descendants().First(d => d.Name.LocalName == "Command")
                .SetValue("\"" + Constants.ExecutablePath + "\"");

            string xmlPath = Path.GetTempFileName();
            using (Stream fileStream = new FileStream(xmlPath, FileMode.Create))
            {
                document.Save(fileStream);
            }

            Process schtasks = new()
            {
                StartInfo =
                {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = true,
                    Verb = "runas",
                    FileName = Path.Combine(Environment.SystemDirectory, "schtasks.exe"),
                    Arguments = $"/Create /XML \"{xmlPath}\" /tn \"Artemis 2 autorun\" /F"
                }
            };

            schtasks.Start();
            schtasks.WaitForExit();

            File.Delete(xmlPath);
        }

        public static void RemoveAutoRunTask()
        {
            Process schtasks = new()
            {
                StartInfo =
                {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = true,
                    Verb = "runas",
                    FileName = Path.Combine(Environment.SystemDirectory, "schtasks.exe"),
                    Arguments = "/Delete /TN \"Artemis 2 autorun\" /f"
                }
            };

            schtasks.Start();
            schtasks.WaitForExit();
        }
    }
}