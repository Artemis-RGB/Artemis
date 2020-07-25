using System.Collections.Generic;
using System.Diagnostics;
using Artemis.Core.Plugins.Abstract;
using Artemis.Core.Plugins.Abstract.ViewModels;
using Artemis.Plugins.Modules.General.DataModel;
using Artemis.Plugins.Modules.General.DataModel.Windows;
using Artemis.Plugins.Modules.General.Utilities;
using Artemis.Plugins.Modules.General.ViewModels;

namespace Artemis.Plugins.Modules.General
{
    public class GeneralModule : ProfileModule<GeneralDataModel>
    {
        public override IEnumerable<ModuleViewModel> GetViewModels()
        {
            return new List<ModuleViewModel> {new GeneralViewModel(this)};
        }

        public override void Update(double deltaTime)
        {
            UpdateCurrentWindow();
            base.Update(deltaTime);
        }

        public override void EnablePlugin()
        {
            DisplayName = "General";
            DisplayIcon = "AllInclusive";
            ExpandsDataModel = true;
        }

        public override void DisablePlugin()
        {
        }

        #region Open windows

        public void UpdateCurrentWindow()
        {
            var processId = WindowUtilities.GetActiveProcessId();
            if (DataModel.ActiveWindow == null || DataModel.ActiveWindow.Process.Id != processId)
                DataModel.ActiveWindow = new WindowDataModel(Process.GetProcessById(processId));
        }

        #endregion
    }
}