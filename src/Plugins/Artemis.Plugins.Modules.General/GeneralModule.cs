using System;
using System.Collections.Generic;
using Artemis.Core.Plugins.Abstract;
using Artemis.Core.Plugins.Abstract.ViewModels;
using Artemis.Core.Plugins.Models;
using Artemis.Plugins.Modules.General.ViewModels;

namespace Artemis.Plugins.Modules.General
{
    public class GeneralModule : ProfileModule
    {
        private readonly PluginSettings _settings;

        public GeneralModule(PluginInfo pluginInfo, PluginSettings settings) : base(pluginInfo)
        {
            _settings = settings;
            DisplayName = "General";
            DisplayIcon = "AllInclusive";
            ExpandsMainDataModel = true;
            DataModel = new GeneralDataModel(this);

            var testSetting = _settings.GetSetting("TestSetting", DateTime.Now);
        }

        public override IEnumerable<ModuleViewModel> GetViewModels()
        {
            return new List<ModuleViewModel> {new GeneralViewModel(this)};
        }

        public override void EnablePlugin()
        {
        }

        public override void DisablePlugin()
        {
        }
    }
}