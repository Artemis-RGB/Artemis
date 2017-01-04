using System.Collections.Generic;
using System.Linq;
using Artemis.Modules.Abstract;
using Artemis.ViewModels.Abstract;

namespace Artemis.ViewModels
{
    public sealed class OverlaysViewModel : BaseViewModel
    {
        private readonly List<ModuleViewModel> _vms;

        public OverlaysViewModel(IEnumerable<ModuleViewModel> moduleViewModels)
        {
            DisplayName = "Overlays";
            _vms = moduleViewModels.Where(m => m.ModuleModel.IsOverlay).OrderBy(m => m.DisplayName).ToList();
        }

        protected override void OnActivate()
        {
            base.OnActivate();
            Items.Clear();
            Items.AddRange(_vms);
        }
    }
}