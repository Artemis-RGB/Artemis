using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Interop;
using Artemis.Core;
using Artemis.Core.Plugins.Abstract;
using Artemis.Core.Plugins.Abstract.ViewModels;
using Artemis.Core.Plugins.Models;
using Artemis.Plugins.Modules.General.DataModel;
using Artemis.Plugins.Modules.General.DataModel.Windows;
using Artemis.Plugins.Modules.General.Utilities;
using Artemis.Plugins.Modules.General.ViewModels;
using SkiaSharp;

namespace Artemis.Plugins.Modules.General
{
    public class GeneralModule : ProfileModule<GeneralDataModel>
    {
        private readonly PluginSettings _settings;
        private readonly Random _rand;


        public GeneralModule(PluginSettings settings)
        {
            _settings = settings;
            _rand = new Random();
        }

        public override IEnumerable<ModuleViewModel> GetViewModels()
        {
            return new List<ModuleViewModel> {new GeneralViewModel(this)};
        }

        public override void Update(double deltaTime)
        {
            DataModel.TestDataModel.UpdatesDividedByFour += 0.25;
            DataModel.TestDataModel.Updates += 1;
            DataModel.TestDataModel.PlayerInfo.Position = new SKPoint(_rand.Next(100), _rand.Next(100));
            DataModel.TestDataModel.PlayerInfo.Health++;
            if (DataModel.TestDataModel.PlayerInfo.Health > 200)
                DataModel.TestDataModel.PlayerInfo.Health = 0;

            DataModel.TestDataModel.IntsList[0] = _rand.Next();
            DataModel.TestDataModel.IntsList[2] = _rand.Next();

            UpdateCurrentWindow();
            UpdateBackgroundWindows();

            base.Update(deltaTime);
        }

        public override void EnablePlugin()
        {
            DisplayName = "General";
            DisplayIcon = "AllInclusive";
            ExpandsDataModel = true;

            DataModel.TestDataModel.IntsList = new List<int> {_rand.Next(), _rand.Next(), _rand.Next()};
            DataModel.TestDataModel.PlayerInfosList = new List<PlayerInfo> {new PlayerInfo()};

            var testSetting = _settings.GetSetting("TestSetting", DateTime.Now);
        }

        public override void DisablePlugin()
        {
        }

        #region Open windows

        private DateTime _lastBackgroundWindowsUpdate;

        public void UpdateCurrentWindow()
        {
            var processId = WindowMonitor.GetActiveProcessId();
            if (DataModel.Windows.ActiveWindow == null || DataModel.Windows.ActiveWindow.Process.Id != processId)
                DataModel.Windows.ActiveWindow = new WindowDataModel(Process.GetProcessById(processId));
        }

        public void UpdateBackgroundWindows()
        {
            // This is kinda slow so lets not do it very often and lets do it in a task
            if (DateTime.Now - _lastBackgroundWindowsUpdate < TimeSpan.FromSeconds(5))
                return;

            _lastBackgroundWindowsUpdate = DateTime.Now;
            Task.Run(() =>
            {
                // All processes with a main window handle are considered open windows
                DataModel.Windows.OpenWindows = Process.GetProcesses()
                    .Where(p => p.MainWindowHandle != IntPtr.Zero)
                    .Select(p => new WindowDataModel(p))
                    .Where(w => !string.IsNullOrEmpty(w.WindowTitle))
                    .ToList();
            });
        }

        #endregion
    }
}