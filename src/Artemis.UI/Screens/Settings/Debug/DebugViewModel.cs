using System;
using Artemis.UI.Screens.Settings.Debug.Tabs;
using Stylet;

namespace Artemis.UI.Screens.Settings.Debug
{
    public class DebugViewModel : Conductor<Screen>.Collection.OneActive
    {
        public DebugViewModel(RenderDebugViewModel renderDebugViewModel, DataModelDebugViewModel dataModelDebugViewModel, LogsDebugViewModel logsDebugViewModel)
        {
            Items.Add(renderDebugViewModel);
            Items.Add(dataModelDebugViewModel);
            Items.Add(logsDebugViewModel);
            ActiveItem = renderDebugViewModel;
        }

        public string Title => "Debugger";

        public void ForceGarbageCollection()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
    }
}