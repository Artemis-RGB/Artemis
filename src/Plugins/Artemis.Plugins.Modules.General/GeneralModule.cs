using System;
using System.Collections.Generic;
using Artemis.Core.Plugins.Abstract;
using Artemis.Core.Plugins.Abstract.ViewModels;
using Artemis.Core.Plugins.Models;
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
            DataModel.UpdatesDividedByFour += 0.25;
            DataModel.Updates += 1;
            DataModel.PlayerInfo.Position = new SKPoint(_rand.Next(100), _rand.Next(100));

            DataModel.IntsList[0] = _rand.Next();
            DataModel.IntsList[2] = _rand.Next();
            base.Update(deltaTime);
        }

        public override void EnablePlugin()
        {
            DisplayName = "General";
            DisplayIcon = "AllInclusive";
            ExpandsDataModel = true;

            DataModel.IntsList = new List<int> {_rand.Next(), _rand.Next(), _rand.Next()};
            DataModel.PlayerInfosList = new List<PlayerInfo> {new PlayerInfo()};

            var testSetting = _settings.GetSetting("TestSetting", DateTime.Now);
        }

        public override void DisablePlugin()
        {
        }
    }
}