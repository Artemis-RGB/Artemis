using System.Diagnostics;
using Artemis.Core;
using Artemis.Core.DataModelExpansions;

namespace Artemis.Plugins.Modules.General.DataModel.Windows
{
    public class WindowDataModel
    {
        public WindowDataModel(Process process)
        {
            Process = process;
            WindowTitle = process.MainWindowTitle;
            ProcessName = process.ProcessName;

            // Accessing MainModule requires admin privileges, this way does not
            ProgramLocation = process.GetProcessFilename();
        }

        [DataModelIgnore]
        public Process Process { get; }

        public string WindowTitle { get; set; }
        public string ProcessName { get; set; }
        public string ProgramLocation { get; set; }
    }
}