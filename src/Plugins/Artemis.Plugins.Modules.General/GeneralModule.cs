using System;
using System.Collections.Generic;
using System.Diagnostics;
using Artemis.Core;
using Artemis.Core.Modules;
using Artemis.Plugins.Modules.General.DataModels;
using Artemis.Plugins.Modules.General.DataModels.Windows;
using Artemis.Plugins.Modules.General.Utilities;
using Artemis.Plugins.Modules.General.ViewModels;
using SkiaSharp;

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

        public override void Update(double deltaTime)
        {
            DataModel.TimeDataModel.CurrentTime = DateTimeOffset.Now;
            DataModel.TimeDataModel.TimeSinceMidnight = DateTimeOffset.Now - DateTimeOffset.Now.Date;
            UpdateCurrentWindow();
        }

        public override void Render(double deltaTime, ArtemisSurface surface, SKCanvas canvas, SKImageInfo canvasInfo)
        {
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