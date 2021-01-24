using System;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Screens.Settings.Debug.Tabs;
using Stylet;

namespace Artemis.UI.Screens.Settings.Debug
{
    public class DebugViewModel : Conductor<Screen>.Collection.OneActive
    {
        private readonly ICoreService _coreService;

        public DebugViewModel(
            ISettingsService settingsService,
            ICoreService coreService,
            RenderDebugViewModel renderDebugViewModel,
            DataModelDebugViewModel dataModelDebugViewModel,
            LogsDebugViewModel logsDebugViewModel)
        {
            _coreService = coreService;
            Items.Add(renderDebugViewModel);
            Items.Add(dataModelDebugViewModel);
            Items.Add(logsDebugViewModel);
            ActiveItem = renderDebugViewModel;

            StayOnTopSetting = settingsService.GetSetting("Debugger.StayOnTop", false);
        }

        public PluginSetting<bool> StayOnTopSetting { get; }

        public string Title => "Debugger";

        public void ToggleStayOnTop()
        {
            StayOnTopSetting.Value = !StayOnTopSetting.Value;
        }

        protected override void OnClose()
        {
            StayOnTopSetting.Save();
            base.OnClose();
        }

        public void ForceGarbageCollection()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        public void Elevate()
        {
            Core.Utilities.Restart(true, TimeSpan.FromMilliseconds(500));
        }

        public void Restart()
        {
            Core.Utilities.Restart(_coreService.IsElevated, TimeSpan.FromMilliseconds(500));
        }
    }
}