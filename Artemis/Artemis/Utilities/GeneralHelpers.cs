using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

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
    }
}
