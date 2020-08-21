using System.Diagnostics;
using Artemis.Core.Extensions;
using Artemis.Core.Plugins.DataModelExpansions.Attributes;
using Artemis.Plugins.Modules.General.Utilities;

namespace Artemis.Plugins.Modules.General.DataModel.Windows
{
    public class WindowDataModel
    {
        [DataModelIgnore]
        public Process Process { get; }

        public WindowDataModel(Process process)
        {
            Process = process;
            WindowTitle = process.MainWindowTitle;
            ProcessName = process.ProcessName;

            // Accessing MainModule requires admin privileges, this way does not
            ProgramLocation = process.GetProcessFilename();
        }

        public string WindowTitle { get; set; }
        public string ProcessName { get; set; }
        public string ProgramLocation { get; set; }
    }
}