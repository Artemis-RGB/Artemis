using System;
using System.Collections.Generic;
using System.Text;

namespace Artemis.Plugins.Modules.General.DataModel.Windows
{
    public class WindowsDataModel
    {
        public WindowDataModel ActiveWindow { get; set; }
        public List<WindowDataModel> OpenWindows { get; set; }
    }
}
