using System;
using System.Collections.Generic;
using System.Diagnostics;
using Artemis.Core.Modules;
using Artemis.Plugins.Modules.General.DataModel;
using Artemis.Plugins.Modules.General.DataModel.Windows;
using Artemis.Plugins.Modules.General.Utilities;
using Artemis.Plugins.Modules.General.ViewModels;

namespace Artemis.Plugins.Modules.General
{
    public class GeneralModule : ProfileModule<GeneralDataModel>
    {
        public override void EnablePlugin()
        {
            DisplayName = "General";
            DisplayIcon = "AllInclusive";
            ExpandsDataModel = true;
            ModuleTabs = new List<ModuleTab> {new ModuleTab<GeneralViewModel>("General")};

            DataModel.TestTimeList.Add(new TimeDataModel {CurrentTime = DateTime.Now.AddDays(1), CurrentTimeUTC = DateTime.UtcNow.AddDays(1)});
            DataModel.TestTimeList.Add(new TimeDataModel {CurrentTime = DateTime.Now.AddDays(2), CurrentTimeUTC = DateTime.UtcNow.AddDays(2)});
            DataModel.TestTimeList.Add(new TimeDataModel {CurrentTime = DateTime.Now.AddDays(3), CurrentTimeUTC = DateTime.UtcNow.AddDays(3)});
            DataModel.TestTimeList.Add(new TimeDataModel {CurrentTime = DateTime.Now.AddDays(4), CurrentTimeUTC = DateTime.UtcNow.AddDays(4)});
        }

        public override void DisablePlugin()
        {
        }

        public override void ModuleActivated(bool isOverride)
        {
        }

        public override void ModuleDeactivated(bool isOverride)
        {
        }

        public override void ProfileUpdate(double deltaTime)
        {
            DataModel.TimeDataModel.CurrentTime = DateTime.Now;
            DataModel.TimeDataModel.CurrentTimeUTC = DateTime.UtcNow;

            UpdateCurrentWindow();
        }

        #region Open windows

        public void UpdateCurrentWindow()
        {
            var processId = WindowUtilities.GetActiveProcessId();
            if (DataModel.ActiveWindow == null || DataModel.ActiveWindow.Process.Id != processId)
                DataModel.ActiveWindow = new WindowDataModel(Process.GetProcessById(processId));
            if (DataModel.ActiveWindow != null && string.IsNullOrWhiteSpace(DataModel.ActiveWindow.WindowTitle))
                DataModel.ActiveWindow.WindowTitle = Process.GetProcessById(WindowUtilities.GetActiveProcessId()).MainWindowTitle;
        }

        #endregion
    }
}