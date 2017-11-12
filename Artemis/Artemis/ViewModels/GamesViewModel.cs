using System.Collections.Generic;
using System.Linq;
using Artemis.Modules.Abstract;
using Artemis.ViewModels.Abstract;

namespace Artemis.ViewModels
{
    public sealed class GamesViewModel : BaseViewModel
    {
        private readonly List<ModuleViewModel> _vms;

        public GamesViewModel(IEnumerable<ModuleViewModel> moduleViewModels)
        {
            DisplayName = "Games";

            _vms = moduleViewModels.Where(m => m.ModuleModel.IsBoundToProcess).OrderBy(g => g.DisplayName).ToList();
        }

        protected override void OnActivate()
        {
            base.OnActivate();
            Items.Clear();
            Items.AddRange(_vms);
        }
    }
}
