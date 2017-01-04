using System.Collections.Generic;
using System.Linq;
using Artemis.DAL;
using Artemis.Managers;
using Artemis.Modules.Abstract;
using Artemis.Settings;
using Artemis.ViewModels.Abstract;

namespace Artemis.ViewModels
{
    public sealed class GamesViewModel : BaseViewModel
    {
        private readonly List<ModuleViewModel> _vms;

        public GamesViewModel(IEnumerable<ModuleViewModel> moduleViewModels)
        {
            DisplayName = "Games";

            // Currently WoW is locked behind a hidden trigger (obviously not that hidden since you're reading this)
            // It is using memory reading and lets first try to contact Blizzard
            if (SettingsProvider.Load<GeneralSettings>().GamestatePort == 62575)
            {
                _vms = moduleViewModels.Where(m => m.ModuleModel.IsBoundToProcess)
                    .OrderBy(g => g.DisplayName).ToList();
            }
            else
            {
                _vms = moduleViewModels.Where(m => m.ModuleModel.IsBoundToProcess && m.DisplayName != "WoW")
                    .OrderBy(g => g.DisplayName).ToList();
            }
        }

        protected override void OnActivate()
        {
            base.OnActivate();
            Items.Clear();
            Items.AddRange(_vms);
        }
    }
}